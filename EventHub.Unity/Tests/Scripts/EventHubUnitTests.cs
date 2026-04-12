using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using EventHub.Unity;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class EventHubUnitTests
{
    private TestEvent testEvent;
    private int syncCallCount;
    private int asyncCallCount;
    private static int mainThreadId;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    [SetUp]
    public void Setup()
    {
        testEvent = new TestEvent { Value = 42 };
        syncCallCount = 0;
        asyncCallCount = 0;
        EventDispatcher.Dispatcher = new EventDispatcherInternal();
    }

    [TearDown]
    public void Teardown()
    {
        EventDispatcher.Logger = null;
        EventDispatcher.ClearErrorEvents();
        EventDispatcher.Dispatcher = null;
        EventDispatcher.ExceptionCatchEnabled = true;
        EventDispatcher.LogEnabled = true;
    }

    private bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == mainThreadId;

    #region 基本订阅与发布

    [Test]
    public void SyncSubscribe_And_Publish_ExecutesHandler()
    {
        EventDispatcher.Subscribe<TestEvent>(e => syncCallCount++);
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(1, syncCallCount);
    }

    [UnityTest]
    public IEnumerator AsyncSubscribe_And_Publish_ExecutesHandler()
    {
        yield return AsyncSubscribe_And_Publish_ExecutesHandler_Internal().ToCoroutine();
    }
    private async UniTask AsyncSubscribe_And_Publish_ExecutesHandler_Internal()
    {
        EventDispatcher.Subscribe<TestEvent>(async (e, ct) =>
        {
            await UniTask.Yield();
            asyncCallCount++;
        });
        await EventDispatcher.PublishAsync(testEvent);
        Assert.AreEqual(1, asyncCallCount);
    }

    #endregion

    #region 取消订阅

    [Test]
    public void Unsubscribe_Sync_RemovesHandler()
    {
        void Handler(TestEvent e) => syncCallCount++;
        EventDispatcher.Subscribe<TestEvent>(Handler);
        EventDispatcher.Unsubscribe<TestEvent>(Handler);
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(0, syncCallCount);
    }

    [UnityTest]
    public IEnumerator Unsubscribe_Async_RemovesHandler()
    {
        yield return Unsubscribe_Async_RemovesHandler_Internal().ToCoroutine();
    }
    private async UniTask Unsubscribe_Async_RemovesHandler_Internal()
    {
        async UniTask Handler(TestEvent e, CancellationToken ct)
        {
            await UniTask.Yield();
            asyncCallCount++;
        }
        EventDispatcher.Subscribe<TestEvent>(Handler);
        EventDispatcher.Unsubscribe<TestEvent>(Handler);
        await EventDispatcher.PublishAsync(testEvent);
        Assert.AreEqual(0, asyncCallCount);
    }

    [Test]
    public void Unsubscribe_UsingSubscriptionToken_Sync()
    {
        var sub = EventDispatcher.Subscribe<TestEvent>(__ => syncCallCount++);
        sub.Dispose();
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(0, syncCallCount);
    }

    [UnityTest]
    public IEnumerator Unsubscribe_UsingSubscriptionToken_Async()
    {
        yield return Unsubscribe_UsingSubscriptionToken_Async_Internal().ToCoroutine();
    }
    private async UniTask Unsubscribe_UsingSubscriptionToken_Async_Internal()
    {
        var sub = EventDispatcher.Subscribe<TestEvent>(async (__, ___) =>
        {
            await UniTask.Yield();
            asyncCallCount++;
        });
        sub.Dispose();
        await EventDispatcher.PublishAsync(testEvent);
        Assert.AreEqual(0, asyncCallCount);
    }

    [Test]
    public void Unsubscribe_NonExistentDelegate_ReturnsZero()
    {
        void Handler(TestEvent e) { }
        var removed = EventDispatcher.Unsubscribe<TestEvent>(Handler);
        Assert.AreEqual(0, removed);
    }

    [UnityTest]
    public IEnumerator UnsubscribeAsync_NonExistentDelegate_ReturnsZero()
    {
        yield return UnsubscribeAsync_NonExistentDelegate_ReturnsZero_Internal().ToCoroutine();
    }
    private async UniTask UnsubscribeAsync_NonExistentDelegate_ReturnsZero_Internal()
    {
        async UniTask Handler(TestEvent e, CancellationToken ct) { await UniTask.Yield(); }
        var removed = EventDispatcher.Unsubscribe<TestEvent>(Handler);
        Assert.AreEqual(0, removed);
    }

    [Test]
    public void Unsubscribe_RemovesAllMatchingDelegates()
    {
        void Handler(TestEvent e) => syncCallCount++;
        EventDispatcher.Subscribe<TestEvent>(Handler);
        EventDispatcher.Subscribe<TestEvent>(Handler);
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(2, syncCallCount);

        int removed = EventDispatcher.Unsubscribe<TestEvent>(Handler);
        Assert.AreEqual(2, removed);

        syncCallCount = 0;
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(0, syncCallCount);
    }

    #endregion

    #region 优先级

    [Test]
    public void SyncPublish_ExecutesByPriority_Descending()
    {
        int order = 0;
        int first = 0, second = 0, third = 0;
        EventDispatcher.Subscribe<TestEvent>(__ => first = ++order, 10);
        EventDispatcher.Subscribe<TestEvent>(__ => second = ++order, 5);
        EventDispatcher.Subscribe<TestEvent>(__ => third = ++order, 0);
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(1, first);
        Assert.AreEqual(2, second);
        Assert.AreEqual(3, third);
    }

    [UnityTest]
    public IEnumerator AsyncSerialPublish_ExecutesByPriority_Descending()
    {
        yield return AsyncSerialPublish_ExecutesByPriority_Descending_Internal().ToCoroutine();
    }
    private async UniTask AsyncSerialPublish_ExecutesByPriority_Descending_Internal()
    {
        int order = 0;
        int first = 0, second = 0, third = 0;
        EventDispatcher.Subscribe<TestEvent>(async (__, ___) => { await UniTask.Yield(); first = ++order; }, 10);
        EventDispatcher.Subscribe<TestEvent>(async (__, ___) => { await UniTask.Yield(); second = ++order; }, 5);
        EventDispatcher.Subscribe<TestEvent>(async (__, ___) => { await UniTask.Yield(); third = ++order; }, 0);
        await EventDispatcher.PublishAsync(testEvent);
        Assert.AreEqual(1, first);
        Assert.AreEqual(2, second);
        Assert.AreEqual(3, third);
    }

    #endregion

    #region 一次性订阅

    [Test]
    public void SubscribeOnce_Sync_ExecutesOnlyOnce()
    {
        EventDispatcher.SubscribeOnce<TestEvent>(__ => syncCallCount++);
        EventDispatcher.Publish(testEvent);
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(1, syncCallCount);
    }

    [UnityTest]
    public IEnumerator SubscribeOnce_Async_ExecutesOnlyOnce()
    {
        yield return SubscribeOnce_Async_ExecutesOnlyOnce_Internal().ToCoroutine();
    }
    private async UniTask SubscribeOnce_Async_ExecutesOnlyOnce_Internal()
    {
        EventDispatcher.SubscribeOnce<TestEvent>(async (__, ___) =>
        {
            await UniTask.Yield();
            asyncCallCount++;
        });
        await EventDispatcher.PublishAsync(testEvent);
        await EventDispatcher.PublishAsync(testEvent);
        Assert.AreEqual(1, asyncCallCount);
    }

    [UnityTest]
    public IEnumerator SubscribeOnce_Async_ConcurrentPublish_OnlyOnce()
    {
        yield return SubscribeOnce_Async_ConcurrentPublish_OnlyOnce_Internal().ToCoroutine();
    }
    private async UniTask SubscribeOnce_Async_ConcurrentPublish_OnlyOnce_Internal()
    {
        int count = 0;
        Func<TestEvent, CancellationToken, UniTask> handler = async (e, ct) =>
        {
            await UniTask.Delay(5);
            Interlocked.Increment(ref count);
        };

        var subscription = EventDispatcher.SubscribeOnce(handler);
        var tasks = Enumerable.Range(0, 10).Select(_ => EventDispatcher.PublishAsync(testEvent));
        await UniTask.WhenAll(tasks);

        Assert.AreEqual(1, count);
        await EventDispatcher.PublishAsync(testEvent);
        Assert.AreEqual(1, count);
    }

    #endregion

    #region 条件订阅

    [Test]
    public void SubscribeIf_Sync_OnlyExecutesWhenFilterPasses()
    {
        EventDispatcher.SubscribeIf<TestEvent>(e => e.Value > 100, __ => syncCallCount++);
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(0, syncCallCount);
    }

    [UnityTest]
    public IEnumerator SubscribeIf_Async_OnlyExecutesWhenFilterPasses()
    {
        yield return SubscribeIf_Async_OnlyExecutesWhenFilterPasses_Internal().ToCoroutine();
    }
    private async UniTask SubscribeIf_Async_OnlyExecutesWhenFilterPasses_Internal()
    {
        EventDispatcher.SubscribeIf<TestEvent>(e => e.Value > 100, async (__, ___) =>
        {
            await UniTask.Yield();
            asyncCallCount++;
        });
        await EventDispatcher.PublishAsync(testEvent);
        Assert.AreEqual(0, asyncCallCount);
    }

    [Test]
    public void SubscribeIf_WithPriority_ExecutesInOrder()
    {
        int order = 0;
        int first = 0, second = 0;
        EventDispatcher.SubscribeIf<TestEvent>(e => e.Value > 100, __ => first = ++order, 10);
        EventDispatcher.SubscribeIf<TestEvent>(e => e.Value > 0, __ => second = ++order, 0);
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(0, first);
        Assert.AreEqual(1, second);
    }

    [Test]
    public void SubscribeIf_FilterThrows_DoesNotInvokeHandlerAndLogsError()
    {
        bool errorLogged = false;
        EventDispatcher.Logger = new TestLogger
        {
            OnLogError = (_, __, ___) => errorLogged = true
        };

        EventDispatcher.SubscribeIf<TestEvent>(
            filter: e => throw new Exception("Filter error"),
            handler: __ => syncCallCount++
        );
        EventDispatcher.Publish(testEvent);

        Assert.AreEqual(0, syncCallCount);
        Assert.IsTrue(errorLogged);

        EventDispatcher.Logger = null;
    }

    #endregion

    #region 发布可中断事件

    [Test]
    public void PublishUntilInterrupt_StopsOnInterrupt()
    {
        int executed = 0;
        var interruptEvent = new InterruptibleEvent();
        EventDispatcher.Subscribe<InterruptibleEvent>(e => { executed++; e.Interrupt(); }, 10);
        EventDispatcher.Subscribe<InterruptibleEvent>(__ => executed++, 5);
        EventDispatcher.PublishInterruptableEvents(interruptEvent);
        Assert.AreEqual(1, executed);
    }

    #endregion

    #region 发布可取消事件

    [Test]
    public void PublishCancelableEvents_SkipsCancelledHandlers()
    {
        var cancelEvent = new CancelableEvent();
        int executed = 0;

        EventDispatcher.Subscribe<CancelableEvent>(e => { executed++; e.Cancel(); }, 10);
        EventDispatcher.Subscribe<CancelableEvent>(__ => executed++, 5);
        EventDispatcher.PublishCancelableEvents(cancelEvent);

        Assert.AreEqual(1, executed);
    }

    #endregion

    #region 并行发布

    [UnityTest]
    public IEnumerator ParallelPublish_ExecutesAllHandlersConcurrently()
    {
        yield return ParallelPublish_ExecutesAllHandlersConcurrently_Internal().ToCoroutine();
    }
    private async UniTask ParallelPublish_ExecutesAllHandlersConcurrently_Internal()
    {
        int counter = 0;
        var tcs = new TaskCompletionSource<bool>();
        int readyCount = 0;

        for (int i = 0; i < 10; i++)
        {
            EventDispatcher.Subscribe<TestEvent>(async (__, ___) =>
            {
                int val = Interlocked.Increment(ref counter);
                if (val == 10) tcs.SetResult(true);
                await tcs.Task;
            });
        }

        var publishTask = EventDispatcher.PublishParallelAsync(testEvent);
        await tcs.Task;
        await publishTask;
        Assert.AreEqual(10, counter);
    }

    #endregion

    #region 多事件类型隔离

    public class EventA { }
    public class EventB { }

    [Test]
    public void DifferentEventTypes_DoNotInterfere()
    {
        int aCount = 0, bCount = 0;
        EventDispatcher.Subscribe<EventA>(__ => aCount++);
        EventDispatcher.Subscribe<EventB>(__ => bCount++);
        EventDispatcher.Publish(new EventA());
        Assert.AreEqual(1, aCount);
        Assert.AreEqual(0, bCount);
    }

    #endregion

    #region 错误处理

    [Test]
    public void SyncPublish_ExceptionInHandler_TriggersLoggerError()
    {
        bool errorLogged = false;
        EventDispatcher.Logger = new TestLogger
        {
            OnLogError = (_, __, ___) => errorLogged = true
        };
        EventDispatcher.Subscribe<TestEvent>(__ => throw new Exception("Test"));
        EventDispatcher.Publish(testEvent);
        Assert.IsTrue(errorLogged);
        EventDispatcher.Logger = null;
    }

    #endregion

    #region 高并发测试

    [UnityTest]
    public IEnumerator HighConcurrency_SubscribeAndPublish()
    {
        yield return HighConcurrency_SubscribeAndPublish_Internal().ToCoroutine();
    }
    private async UniTask HighConcurrency_SubscribeAndPublish_Internal()
    {
        int completed = 0;
        const int iterations = 1000;
        var handlers = new List<Action<TestEvent>>();

        for (int i = 0; i < 100; i++)
        {
            var h = new Action<TestEvent>(_ => Interlocked.Increment(ref completed));
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }

        var tasks = Enumerable.Range(0, iterations).Select(_ =>
            UniTask.RunOnThreadPool(() => EventDispatcher.Publish(testEvent))
        );
        await UniTask.WhenAll(tasks);

        Assert.AreEqual(iterations * handlers.Count, completed);
        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    #endregion
}