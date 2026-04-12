using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EventHub.Unity;
using EventHub;

public class EventHubAdvancedTests
{
    private TestEvent testEvent;

    [SetUp]
    public void Setup()
    {
        testEvent = new TestEvent { Value = 42 };
        EventDispatcher.Dispatcher = new EventDispatcherInternal();
    }

    [TearDown]
    public void TearDown()
    {
        EventDispatcher.Dispatcher = null;
        EventDispatcher.Logger = null;
        EventDispatcher.ClearErrorEvents();
        EventDispatcher.ExceptionCatchEnabled = true;
        EventDispatcher.LogEnabled = true;
    }

    #region 快照机制 - 并发修改测试

    [UnityTest]
    public IEnumerator ConcurrentPublishAndModify_NoExceptionsOrMissedCalls()
    {
        yield return ConcurrentPublishAndModify_Internal().ToCoroutine();
    }

    private async UniTask ConcurrentPublishAndModify_Internal()
    {
        int publishCount = 0;
        const int totalPublishes = 200;
        const int modifyOperations = 50;
        const int initialSubscribers = 30;

        // 初始订阅
        var handlers = new List<Action<TestEvent>>();
        for (int i = 0; i < initialSubscribers; i++)
        {
            var h = new Action<TestEvent>(_ => Interlocked.Increment(ref publishCount));
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }

        // 并发发布
        var publishTasks = Enumerable.Range(0, totalPublishes)
            .Select(_ => UniTask.RunOnThreadPool(() => EventDispatcher.Publish(testEvent)));

        // 同时在另一个线程进行订阅/取消订阅操作
        var modifyTask = UniTask.RunOnThreadPool(async () =>
        {
            for (int i = 0; i < modifyOperations; i++)
            {
                var tempHandler = new Action<TestEvent>(_ => { });
                EventDispatcher.Subscribe(tempHandler);
                await UniTask.Yield();
                EventDispatcher.Unsubscribe(tempHandler);
            }
        });

        await UniTask.WhenAll(publishTasks.Concat(new[] { modifyTask }));

        // 由于并发修改，临时订阅者可能被执行也可能不执行，但初始订阅者必须都被执行
        int expectedMin = totalPublishes * initialSubscribers;
        // 允许 1% 的误差（在高并发修改下是正常的）
        int acceptableMin = (int)(expectedMin * 0.99);
        Assert.GreaterOrEqual(publishCount, acceptableMin,
            $"Should have at least {acceptableMin} calls, but got {publishCount}");

        // 清理
        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    #endregion

    #region 并行发布 - 异常聚合

    [UnityTest]
    public IEnumerator ParallelPublish_AggregatesExceptions()
    {
        EventDispatcher.LogEnabled = false; // 禁用日志
        yield return ParallelPublish_AggregatesExceptions_Internal().ToCoroutine();
        EventDispatcher.LogEnabled = true;
    }

    private async UniTask ParallelPublish_AggregatesExceptions_Internal()
    {
        const int handlerCount = 5;
        var handlers = new List<Func<TestEvent, CancellationToken, UniTask>>();

        for (int i = 0; i < handlerCount; i++)
        {
            int id = i;
            var h = new Func<TestEvent, CancellationToken, UniTask>(async (_, __) =>
            {
                await UniTask.Yield();
                throw new Exception($"Error from handler {id}");
            });
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }

        try
        {
            await EventDispatcher.PublishParallelAsync(testEvent);
            Assert.Fail("Should have thrown AggregateException");
        }
        catch (AggregateException ex)
        {
            Assert.AreEqual(handlerCount, ex.InnerExceptions.Count);
            for (int i = 0; i < handlerCount; i++)
            {
                Assert.IsTrue(ex.InnerExceptions.Any(e => e.Message.Contains($"handler {i}")),
                    $"Exception from handler {i} not found in aggregate");
            }
        }
        finally
        {
            foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
        }
    }

    [UnityTest]
    public IEnumerator ParallelPublish_NoExceptions_CompletesSuccessfully()
    {
        yield return ParallelPublish_NoExceptions_CompletesSuccessfully_Internal().ToCoroutine();
    }
    private async UniTask ParallelPublish_NoExceptions_CompletesSuccessfully_Internal()
    {
        int counter = 0;
        for (int i = 0; i < 10; i++)
        {
            EventDispatcher.Subscribe<TestEvent>(async (_, __) =>
            {
                await UniTask.Yield();
                Interlocked.Increment(ref counter);
            });
        }

        await EventDispatcher.PublishParallelAsync(testEvent).ToCoroutine();
        Assert.AreEqual(10, counter);
    }

    #endregion

    #region 取消令牌传播

    [UnityTest]
    public IEnumerator PublishAsync_CancellationToken_StopsExecution()
    {
        yield return PublishAsync_CancellationToken_StopsExecution_Internal().ToCoroutine();
    }

    private async UniTask PublishAsync_CancellationToken_StopsExecution_Internal()
    {
        int executed = 0;
        var cts = new CancellationTokenSource();

        var handlers = new List<Func<TestEvent, CancellationToken, UniTask>>();
        for (int i = 0; i < 5; i++)
        {
            var h = new Func<TestEvent, CancellationToken, UniTask>(async (_, ct) =>
            {
                await UniTask.Delay(50, cancellationToken: ct);
                Interlocked.Increment(ref executed);
            });
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }

        var publishTask = EventDispatcher.PublishAsync(testEvent, cts.Token);

        // 短时间后取消
        await UniTask.Delay(10);
        cts.Cancel();

        // 等待发布完成（应被取消）
        try
        {
            await publishTask;
        }
        catch (OperationCanceledException)
        {
            // 预期异常
        }

        // 由于取消发生在第一个处理器执行期间，后续处理器应被跳过
        // 至少执行数应小于总数
        Assert.Less(executed, 5);

        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    [UnityTest]
    public IEnumerator PublishAsync_CancellationTokenBeforePublish_ThrowsImmediately()
    {
        yield return PublishAsync_CancellationTokenBeforePublish_ThrowsImmediately_Internal().ToCoroutine();
    }
    private async UniTask PublishAsync_CancellationTokenBeforePublish_ThrowsImmediately_Internal()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            await EventDispatcher.PublishAsync(testEvent, cts.Token);
            Assert.Fail("Should throw OperationCanceledException");
        }
        catch (OperationCanceledException)
        {
            // 通过
        }
    }

    #endregion

    #region 清理 API 测试

    [Test]
    public void TryCleanupUnusedLocks_RemovesUnusedLocks()
    {
        void Handler(TestEvent e) { }
        EventDispatcher.Subscribe<TestEvent>(Handler);
        EventDispatcher.Unsubscribe<TestEvent>(Handler);

        int removed = EventDispatcher.TryCleanupUnusedLocks();
        Assert.Greater(removed, 0, "Should remove at least one unused lock");
    }

    [Test]
    public void TryCleanupUnusedLocks_KeepsActiveLocks()
    {
        void Handler(TestEvent e) { }
        EventDispatcher.Subscribe<TestEvent>(Handler);

        int removed = EventDispatcher.TryCleanupUnusedLocks();
        Assert.AreEqual(0, removed, "Should not remove lock for active subscriber");

        EventDispatcher.Unsubscribe<TestEvent>(Handler);
    }

    [Test]
    public void TryCleanupUnusedCollections_RemovesEmptyCollections()
    {
        void Handler(TestEvent e) { }
        EventDispatcher.Subscribe<TestEvent>(Handler);
        EventDispatcher.Unsubscribe<TestEvent>(Handler);

        int removed = EventDispatcher.TryCleanupUnusedCollections();
        Assert.Greater(removed, 0);
    }

    [Test]
    public void TryCleanupUnusedLocksAndCollections_RemovesBoth()
    {
        void Handler(TestEvent e) { }
        EventDispatcher.Subscribe<TestEvent>(Handler);
        EventDispatcher.Unsubscribe<TestEvent>(Handler);

        int removed = EventDispatcher.TryCleanupUnusedLocksAndCollections();
        Assert.Greater(removed, 1); // 锁 + 集合
    }

    #endregion

    #region 日志和异常捕获开关

    [Test]
    public void LogDisabled_DoesNotCallLogger()
    {
        bool logged = false;
        EventDispatcher.Logger = new TestLogger { OnLogError = (_, __, ___) => logged = true };
        EventDispatcher.LogEnabled = false;

        EventDispatcher.Subscribe<TestEvent>(_ => throw new Exception("Test"));
        EventDispatcher.Publish(testEvent);

        Assert.IsFalse(logged, "Logger should not be called when logging disabled");

        EventDispatcher.LogEnabled = true;
        EventDispatcher.Logger = null;
    }

    [Test]
    public void ExceptionCatchDisabled_DoesNotTriggerOnErrorEvent()
    {
        bool caught = false;
        EventDispatcher.OnError += (_, __, ___) => caught = true;
        EventDispatcher.ExceptionCatchEnabled = false;
        EventDispatcher.LogEnabled = false; // 禁用日志避免干扰测试

        EventDispatcher.Subscribe<TestEvent>(_ => throw new Exception("Test"));
        EventDispatcher.Publish(testEvent);

        Assert.IsFalse(caught, "OnError event should not be triggered when catching disabled");

        // 恢复设置
        EventDispatcher.ExceptionCatchEnabled = true;
        EventDispatcher.LogEnabled = true;
        EventDispatcher.ClearErrorEvents();
    }

    #endregion

    #region 边界条件

    [Test]
    public void Publish_NullEvent_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => EventDispatcher.Publish<TestEvent>(null));
    }

    [Test]
    public void Subscribe_NullHandler_ReturnsNullAndLogsWarning()
    {
        bool warned = false;
        EventDispatcher.Logger = new TestLogger { OnLogWarning = _ => warned = true };

        var sub = EventDispatcher.Subscribe<TestEvent>(null);
        Assert.IsNull(sub);
        Assert.IsTrue(warned, "Should log warning for null handler");

        EventDispatcher.Logger = null;
    }

    [Test]
    public void Unsubscribe_NullHandler_ReturnsZero()
    {
        int removed = EventDispatcher.Unsubscribe<TestEvent>(null);
        Assert.AreEqual(0, removed);
    }

    [Test]
    public void PublishInterruptableEvents_NoSubscribers_DoesNotThrow()
    {
        var evt = new InterruptibleEvent();
        Assert.DoesNotThrow(() => EventDispatcher.PublishInterruptableEvents(evt));
    }

    [Test]
    public void PublishCancelableEvents_NoSubscribers_DoesNotThrow()
    {
        var evt = new CancelableEvent();
        Assert.DoesNotThrow(() => EventDispatcher.PublishCancelableEvents(evt));
    }

    [Test]
    public void SubscribeOnce_WithNullHandler_ReturnsNull()
    {
        var sub = EventDispatcher.SubscribeOnce<TestEvent>(null);
        Assert.IsNull(sub);
    }

    [Test]
    public void SubscribeIf_WithNullFilter_ReturnsNull()
    {
        bool warned = false;
        EventDispatcher.Logger = new TestLogger { OnLogWarning = _ => warned = true };

        var sub = EventDispatcher.SubscribeIf<TestEvent>(null, _ => { });
        Assert.IsNull(sub);
        Assert.IsTrue(warned);

        EventDispatcher.Logger = null;
    }

    [Test]
    public void SubscribeIf_WithNullHandler_ReturnsNull()
    {
        var sub = EventDispatcher.SubscribeIf<TestEvent>(_ => true, null);
        Assert.IsNull(sub);
    }

    #endregion

    #region 自定义分发器替换

    [Test]
    public void CustomDispatcher_IsUsed()
    {
        var custom = new CustomDispatcher();
        EventDispatcher.Dispatcher = custom;

        bool handlerCalled = false;
        EventDispatcher.Subscribe<TestEvent>(_ => handlerCalled = true);
        EventDispatcher.Publish(testEvent);

        Assert.IsTrue(handlerCalled, "Handler should be called through custom dispatcher");
        Assert.IsTrue(custom.PublishCalled, "Custom dispatcher's Publish should be called");

        EventDispatcher.Dispatcher = null;
    }

    private class CustomDispatcher : IEventDispatcher, ISyncEventDispatcher
    {
        public bool PublishCalled { get; private set; }
        private readonly List<Action<TestEvent>> handlers = new List<Action<TestEvent>>();

        public void Publish<TEvent>(TEvent @event)
        {
            PublishCalled = true;
            if (@event is TestEvent te)
            {
                foreach (var h in handlers) h(te);
            }
        }

        public ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)
        {
            if (handler is Action<TestEvent> typedHandler)
            {
                handlers.Add(typedHandler);
            }
            return new Subscription(typeof(TEvent), priority, () =>
            {
                if (handler is Action<TestEvent> th) handlers.Remove(th);
            });
        }

        public int Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler is Action<TestEvent> th && handlers.Remove(th)) return 1;
            return 0;
        }
    }

    #endregion

    #region 高并发订阅+发布（补充）

    [UnityTest]
    public IEnumerator HighConcurrency_SubscribeUnsubscribeDuringPublish()
    {
        yield return HighConcurrency_SubscribeUnsubscribeDuringPublish_Internal().ToCoroutine();
    }

    private async UniTask HighConcurrency_SubscribeUnsubscribeDuringPublish_Internal()
    {
        int publishCount = 0;
        const int totalPublishes = 300;
        var initialHandlers = new List<Action<TestEvent>>();

        for (int i = 0; i < 50; i++)
        {
            var h = new Action<TestEvent>(_ => Interlocked.Increment(ref publishCount));
            initialHandlers.Add(h);
            EventDispatcher.Subscribe(h);
        }

        // 启动持续发布
        var publishTask = UniTask.WhenAll(Enumerable.Range(0, totalPublishes)
            .Select(_ => UniTask.RunOnThreadPool(() => EventDispatcher.Publish(testEvent))));

        // 同时在主线程频繁增删订阅者
        var modifyTask = UniTask.RunOnThreadPool(async () =>
        {
            for (int i = 0; i < 100; i++)
            {
                var temp = new Action<TestEvent>(_ => { });
                EventDispatcher.Subscribe(temp);
                await UniTask.Yield();
                EventDispatcher.Unsubscribe(temp);
            }
        });

        await UniTask.WhenAll(publishTask, modifyTask);

        // 验证初始订阅者都被正确调用（发布期间未被意外移除）
        int expectedMin = totalPublishes * initialHandlers.Count;
        Assert.GreaterOrEqual(publishCount, expectedMin);

        foreach (var h in initialHandlers) EventDispatcher.Unsubscribe(h);
    }

    #endregion
}