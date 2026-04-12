using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;
using EventHub.Unity;

[TestFixture]
public class EventHubPerformanceTests
{
    private TestEvent testEvent;

    public class PerfEventA { }
    public class PerfEventB { }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (SubscriptionMonitor.Instance != null)
            SubscriptionMonitor.Instance.StopTimer();
        Application.targetFrameRate = -1;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Application.targetFrameRate = 60;
    }

    [SetUp]
    public void Setup()
    {
        testEvent = new TestEvent { Value = 42 };
        EventDispatcher.Dispatcher = new EventDispatcherInternal();
        EventDispatcher.LogEnabled = false;
        EventDispatcher.ExceptionCatchEnabled = false;
    }

    [TearDown]
    public void TearDown()
    {
        EventDispatcher.Dispatcher = null;
        EventDispatcher.Logger = null;
        EventDispatcher.ClearErrorEvents();
        EventDispatcher.LogEnabled = true;
        EventDispatcher.ExceptionCatchEnabled = true;
    }

    // ========== 同步测试：使用 Measure.Method ==========

    [Test, Performance]
    public void Subscribe_Sync()
    {
        var handler = new Action<TestEvent>(_ => { });
        Measure.Method(() =>
        {
            var sub = EventDispatcher.Subscribe(handler);
            sub.Dispose();
        }).Run();
    }

    [Test, Performance]
    public void Unsubscribe_Sync()
    {
        var handler = new Action<TestEvent>(_ => { });
        var sub = EventDispatcher.Subscribe(handler);
        Measure.Method(() =>
        {
            sub.Dispose();
            sub = EventDispatcher.Subscribe(handler);
        }).Run();
        sub.Dispose();
    }

    [Test, Performance]
    public void SubscribeOnce_Sync()
    {
        var handler = new Action<TestEvent>(_ => { });
        Measure.Method(() =>
        {
            var sub = EventDispatcher.SubscribeOnce(handler);
            sub.Dispose();
        }).Run();
    }

    [Test, Performance]
    public void Subscribe_WithRandomPriorities()
    {
        var random = new System.Random(42);
        var handlers = new List<Action<TestEvent>>();
        Measure.Method(() =>
        {
            var h = new Action<TestEvent>(_ => { });
            handlers.Add(h);
            EventDispatcher.Subscribe(h, random.Next(0, 1000));
        }).Run();
        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    // ========== 异步测试：使用 Measure.Scope + 协程 ==========

    [UnityTest, Performance]
    public IEnumerator Subscribe_Async()
    {
        var handler = new Func<TestEvent, CancellationToken, UniTask>((_, __) => UniTask.CompletedTask);
        // 预热
        for (int i = 0; i < 5; i++)
        {
            var s = EventDispatcher.Subscribe(handler);
            s.Dispose();
        }
        yield return null;

        for (int i = 0; i < 50; i++)
        {
            using (Measure.Scope("Subscribe_Async"))
            {
                var sub = EventDispatcher.Subscribe(handler);
                sub.Dispose();
            }
            yield return null;
        }
    }

    [UnityTest, Performance]
    public IEnumerator Unsubscribe_Async()
    {
        var handler = new Func<TestEvent, CancellationToken, UniTask>((_, __) => UniTask.CompletedTask);
        for (int i = 0; i < 50; i++)
        {
            var sub = EventDispatcher.Subscribe(handler);
            using (Measure.Scope("Unsubscribe_Async"))
            {
                sub.Dispose();
            }
            yield return null;
        }
    }

    [UnityTest, Performance]
    public IEnumerator SubscribeOnce_Async()
    {
        var handler = new Func<TestEvent, CancellationToken, UniTask>((_, __) => UniTask.CompletedTask);
        for (int i = 0; i < 5; i++)
        {
            var s = EventDispatcher.SubscribeOnce(handler);
            s.Dispose();
        }
        yield return null;

        for (int i = 0; i < 50; i++)
        {
            using (Measure.Scope("SubscribeOnce_Async"))
            {
                var sub = EventDispatcher.SubscribeOnce(handler);
                sub.Dispose();
            }
            yield return null;
        }
    }

    // ========== 同步发布梯度测试 ==========

    [Test, Performance] public void SyncPublish_0() => RunSyncPublish(0);
    [Test, Performance] public void SyncPublish_1() => RunSyncPublish(1);
    [Test, Performance] public void SyncPublish_10() => RunSyncPublish(10);
    [Test, Performance] public void SyncPublish_50() => RunSyncPublish(50);
    [Test, Performance] public void SyncPublish_100() => RunSyncPublish(100);
    [Test, Performance] public void SyncPublish_200() => RunSyncPublish(200);
    [Test, Performance] public void SyncPublish_500() => RunSyncPublish(500);
    [Test, Performance] public void SyncPublish_1000() => RunSyncPublish(1000);

    private void RunSyncPublish(int subscriberCount)
    {
        var handlers = new List<Action<TestEvent>>();
        for (int i = 0; i < subscriberCount; i++)
        {
            var h = new Action<TestEvent>(_ => { });
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }

        Measure.Method(() => EventDispatcher.Publish(testEvent))
            .SampleGroup($"SyncPublish_{subscriberCount}Subs")
            .Run();

        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    // ========== 异步串行发布（使用 Measure.Scope + 协程） ==========

    [UnityTest, Performance] public IEnumerator AsyncSerialPublish_0() => RunAsyncSerial(0);
    [UnityTest, Performance] public IEnumerator AsyncSerialPublish_1() => RunAsyncSerial(1);
    [UnityTest, Performance] public IEnumerator AsyncSerialPublish_10() => RunAsyncSerial(10);
    [UnityTest, Performance] public IEnumerator AsyncSerialPublish_50() => RunAsyncSerial(50);
    [UnityTest, Performance] public IEnumerator AsyncSerialPublish_100() => RunAsyncSerial(100);
    [UnityTest, Performance] public IEnumerator AsyncSerialPublish_200() => RunAsyncSerial(200);
    [UnityTest, Performance] public IEnumerator AsyncSerialPublish_500() => RunAsyncSerial(500);
    [UnityTest, Performance] public IEnumerator AsyncSerialPublish_1000() => RunAsyncSerial(1000);

    private IEnumerator RunAsyncSerial(int subscriberCount)
    {
        var handlers = new List<Func<TestEvent, CancellationToken, UniTask>>();
        for (int i = 0; i < subscriberCount; i++)
        {
            var h = new Func<TestEvent, CancellationToken, UniTask>((_, __) => UniTask.CompletedTask);
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }

        // 预热
        yield return EventDispatcher.PublishAsync(testEvent).ToCoroutine();
        yield return null;

        var measurementCount = subscriberCount == 0 ? 100 : 50;
        for (int i = 0; i < measurementCount; i++)
        {
            var task = EventDispatcher.PublishAsync(testEvent);
            using (Measure.Scope($"AsyncSerial_{subscriberCount}Subs"))
            {
                yield return task.ToCoroutine();
            }
        }

        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    // ========== 异步并行发布 ==========

    [UnityTest, Performance] public IEnumerator AsyncParallelPublish_0() => RunAsyncParallel(0);
    [UnityTest, Performance] public IEnumerator AsyncParallelPublish_1() => RunAsyncParallel(1);
    [UnityTest, Performance] public IEnumerator AsyncParallelPublish_10() => RunAsyncParallel(10);
    [UnityTest, Performance] public IEnumerator AsyncParallelPublish_50() => RunAsyncParallel(50);
    [UnityTest, Performance] public IEnumerator AsyncParallelPublish_100() => RunAsyncParallel(100);
    [UnityTest, Performance] public IEnumerator AsyncParallelPublish_200() => RunAsyncParallel(200);
    [UnityTest, Performance] public IEnumerator AsyncParallelPublish_500() => RunAsyncParallel(500);
    [UnityTest, Performance] public IEnumerator AsyncParallelPublish_1000() => RunAsyncParallel(1000);

    private IEnumerator RunAsyncParallel(int subscriberCount)
    {
        var handlers = new List<Func<TestEvent, CancellationToken, UniTask>>();
        for (int i = 0; i < subscriberCount; i++)
        {
            var h = new Func<TestEvent, CancellationToken, UniTask>((_, __) => UniTask.CompletedTask);
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }

        yield return EventDispatcher.PublishParallelAsync(testEvent).ToCoroutine();
        yield return null;

        var measurementCount = subscriberCount == 0 ? 100 : 50;
        for (int i = 0; i < measurementCount; i++)
        {
            var task = EventDispatcher.PublishParallelAsync(testEvent);
            using (Measure.Scope($"AsyncParallel_{subscriberCount}Subs"))
            {
                yield return task.ToCoroutine();
            }
        }

        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    // ========== 异步延迟场景 ==========

    [UnityTest, Performance]
    public IEnumerator AsyncSerialPublish_WithYield()
    {
        for (int i = 0; i < 30; i++)
            EventDispatcher.Subscribe<TestEvent>(async (_, __) => await UniTask.Yield());

        yield return EventDispatcher.PublishAsync(testEvent).ToCoroutine();
        yield return null;

        for (int i = 0; i < 20; i++)
        {
            var task = EventDispatcher.PublishAsync(testEvent);
            using (Measure.Scope("AsyncSerial_WithYield"))
            {
                yield return task.ToCoroutine();
            }
        }
    }

    [UnityTest, Performance]
    public IEnumerator AsyncParallelPublish_WithYield()
    {
        for (int i = 0; i < 30; i++)
            EventDispatcher.Subscribe<TestEvent>(async (_, __) => await UniTask.Yield());

        yield return EventDispatcher.PublishParallelAsync(testEvent).ToCoroutine();
        yield return null;

        for (int i = 0; i < 20; i++)
        {
            var task = EventDispatcher.PublishParallelAsync(testEvent);
            using (Measure.Scope("AsyncParallel_WithYield"))
            {
                yield return task.ToCoroutine();
            }
        }
    }

    // ========== 主线程切换开销 ==========

    [UnityTest, Performance]
    public IEnumerator SubscribeOnMainThread_Overhead()
    {
        EventDispatcher.SubscribeOnMainThread<TestEvent>(async (_, __) => await UniTask.CompletedTask);
        yield return EventDispatcher.PublishAsync(testEvent).ToCoroutine();
        yield return null;

        for (int i = 0; i < 30; i++)
        {
            var task = EventDispatcher.PublishAsync(testEvent);
            using (Measure.Scope("SubscribeOnMainThread_Overhead"))
            {
                yield return task.ToCoroutine();
            }
        }
    }

    // ========== 高并发吞吐 ==========

    [UnityTest, Performance]
    public IEnumerator ConcurrentPublish_Throughput()
    {
        const int subscriberCount = 50;
        for (int i = 0; i < subscriberCount; i++)
            EventDispatcher.Subscribe<TestEvent>(_ => { });

        const int publishCount = 500;

        // 预热
        var tasks = Enumerable.Range(0, publishCount)
            .Select(_ => UniTask.RunOnThreadPool(() => EventDispatcher.Publish(testEvent)));
        yield return UniTask.WhenAll(tasks).ToCoroutine();
        yield return null;

        for (int i = 0; i < 5; i++)
        {
            using (Measure.Scope("ConcurrentPublish_Throughput"))
            {
                var t = Enumerable.Range(0, publishCount)
                    .Select(_ => UniTask.RunOnThreadPool(() => EventDispatcher.Publish(testEvent)));
                yield return UniTask.WhenAll(t).ToCoroutine();
            }
        }
    }

    // ========== 快照刷新 ==========

    [Test, Performance]
    public void Publish_AfterModification_MeasuresSnapshotRefresh()
    {
        const int stableSubs = 100;
        var handlers = new List<Action<TestEvent>>();
        for (int i = 0; i < stableSubs; i++)
        {
            var h = new Action<TestEvent>(_ => { });
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }
        for (int i = 0; i < 10; i++) EventDispatcher.Publish(testEvent);
        Measure.Method(() =>
        {
            var temp = new Action<TestEvent>(_ => { });
            EventDispatcher.Subscribe(temp);
            EventDispatcher.Unsubscribe(temp);
            EventDispatcher.Publish(testEvent);
        }).Run();
        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    [Test, Performance]
    public void StablePublish_Allocation()
    {
        for (int i = 0; i < 20; i++)
            EventDispatcher.Subscribe<TestEvent>(_ => { });
        for (int i = 0; i < 5; i++) EventDispatcher.Publish(testEvent);
        Measure.Method(() => EventDispatcher.Publish(testEvent))
            .GC()
            .Run();
    }

    // ========== 中断发布 ==========

    [Test, Performance] public void PublishUntilInterrupt_NoInterrupt_0() => RunPublishUntilInterruptNoInterrupt(0);
    [Test, Performance] public void PublishUntilInterrupt_NoInterrupt_1() => RunPublishUntilInterruptNoInterrupt(1);
    [Test, Performance] public void PublishUntilInterrupt_NoInterrupt_10() => RunPublishUntilInterruptNoInterrupt(10);
    [Test, Performance] public void PublishUntilInterrupt_NoInterrupt_50() => RunPublishUntilInterruptNoInterrupt(50);
    [Test, Performance] public void PublishUntilInterrupt_NoInterrupt_100() => RunPublishUntilInterruptNoInterrupt(100);

    private void RunPublishUntilInterruptNoInterrupt(int subscriberCount)
    {
        var handlers = new List<Action<InterruptibleEvent>>();
        for (int i = 0; i < subscriberCount; i++)
        {
            var h = new Action<InterruptibleEvent>(_ => { });
            handlers.Add(h);
            EventDispatcher.Subscribe(h);
        }
        var evt = new InterruptibleEvent();
        Measure.Method(() => EventDispatcher.PublishInterruptableEvents(evt)).Run();
        foreach (var h in handlers) EventDispatcher.Unsubscribe(h);
    }

    [Test, Performance] public void PublishUntilInterrupt_InterruptAtFirst_0() => RunPublishUntilInterruptFirst(0);
    [Test, Performance] public void PublishUntilInterrupt_InterruptAtFirst_1() => RunPublishUntilInterruptFirst(1);
    [Test, Performance] public void PublishUntilInterrupt_InterruptAtFirst_10() => RunPublishUntilInterruptFirst(10);
    [Test, Performance] public void PublishUntilInterrupt_InterruptAtFirst_50() => RunPublishUntilInterruptFirst(50);
    [Test, Performance] public void PublishUntilInterrupt_InterruptAtFirst_100() => RunPublishUntilInterruptFirst(100);

    private void RunPublishUntilInterruptFirst(int subscriberCount)
    {
        var firstHandler = new Action<InterruptibleEvent>(e => e.Interrupt());
        EventDispatcher.Subscribe(firstHandler, 100);
        var otherHandlers = new List<Action<InterruptibleEvent>>();
        for (int i = 1; i < subscriberCount; i++)
        {
            var h = new Action<InterruptibleEvent>(_ => { });
            otherHandlers.Add(h);
            EventDispatcher.Subscribe(h);
        }
        var evt = new InterruptibleEvent();
        Measure.Method(() => EventDispatcher.PublishInterruptableEvents(evt)).Run();
        EventDispatcher.Unsubscribe(firstHandler);
        foreach (var h in otherHandlers) EventDispatcher.Unsubscribe(h);
    }

    [Test, Performance]
    public void Publish_MultipleEventTypes_NoInterference()
    {
        EventDispatcher.Subscribe<PerfEventA>(_ => { });
        EventDispatcher.Subscribe<PerfEventB>(_ => { });
        for (int i = 0; i < 50; i++) EventDispatcher.Subscribe<TestEvent>(_ => { });
        var evtA = new PerfEventA();
        Measure.Method(() => EventDispatcher.Publish(evtA)).Run();
    }

    [Test, Performance]
    public void TryCleanupUnusedLocks_Performance()
    {
        for (int i = 0; i < 50; i++)
        {
            void Handler(TestEvent e) { }
            EventDispatcher.Subscribe<TestEvent>(Handler);
            EventDispatcher.Unsubscribe<TestEvent>(Handler);
        }
        Measure.Method(() => EventDispatcher.TryCleanupUnusedLocks()).Run();
    }

    #region GC 分配专项测试

    [Test, Performance]
    public void Subscribe_Sync_GCAllocation()
    {
        var handler = new Action<TestEvent>(_ => { });
        Measure.Method(() =>
        {
            var sub = EventDispatcher.Subscribe(handler);
            sub.Dispose();
        })
        .GC()
        .Run();
    }

    [Test, Performance]
    public void SubscribeOnce_Sync_GCAllocation()
    {
        var handler = new Action<TestEvent>(_ => { });
        Measure.Method(() =>
        {
            var sub = EventDispatcher.SubscribeOnce(handler);
            sub.Dispose();
        })
        .GC()
        .Run();
    }

    [Test, Performance]
    public void SyncPublish_Stable_GCAllocation()
    {
        // 准备 100 个稳定订阅者，预热后测量单次发布的 GC
        for (int i = 0; i < 100; i++)
            EventDispatcher.Subscribe<TestEvent>(_ => { });

        for (int i = 0; i < 10; i++) EventDispatcher.Publish(testEvent);

        Measure.Method(() => EventDispatcher.Publish(testEvent))
            .WarmupCount(5)
            .MeasurementCount(30)
            .GC()
            .Run();
    }

    [Test, Performance]
    public void Publish_AfterModification_GCAllocation()
    {
        const int stableSubs = 100;
        for (int i = 0; i < stableSubs; i++)
            EventDispatcher.Subscribe<TestEvent>(_ => { });

        for (int i = 0; i < 10; i++) EventDispatcher.Publish(testEvent);

        Measure.Method(() =>
        {
            var temp = new Action<TestEvent>(_ => { });
            EventDispatcher.Subscribe(temp);
            EventDispatcher.Unsubscribe(temp);
            EventDispatcher.Publish(testEvent);
        })
        .WarmupCount(3)
        .MeasurementCount(15)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void PublishInterruptableEvents_GCAllocation()
    {
        for (int i = 0; i < 50; i++)
            EventDispatcher.Subscribe<InterruptibleEvent>(_ => { });

        var evt = new InterruptibleEvent();
        for (int i = 0; i < 5; i++) EventDispatcher.PublishInterruptableEvents(evt);

        Measure.Method(() => EventDispatcher.PublishInterruptableEvents(evt))
            .WarmupCount(5)
            .MeasurementCount(20)
            .GC()
            .Run();
    }

    [Test, Performance]
    public void TryCleanupUnusedLocks_GCAllocation()
    {
        // 创建并释放 50 个锁
        for (int i = 0; i < 50; i++)
        {
            void Handler(TestEvent e) { }
            EventDispatcher.Subscribe<TestEvent>(Handler);
            EventDispatcher.Unsubscribe<TestEvent>(Handler);
        }

        Measure.Method(() => EventDispatcher.TryCleanupUnusedLocks())
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
    }

    [UnityTest, Performance]
    public IEnumerator AsyncPublish_GCAllocation()
    {
        // 使用 UniTask 的 ToCoroutine 只在外层调用一次
        yield return AsyncPublish_GCAllocation_Internal().ToCoroutine();
    }

    private async UniTask AsyncPublish_GCAllocation_Internal()
    {
        const int subCount = 50;
        for (int i = 0; i < subCount; i++)
            EventDispatcher.Subscribe<TestEvent>((_, __) => UniTask.CompletedTask); // 先使用无分配版本

        // 预热
        await EventDispatcher.PublishAsync(testEvent);
        await UniTask.Yield();

        long totalAllocated = 0;
        const int measurementCount = 20;
        for (int i = 0; i < measurementCount; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long memBefore = GC.GetTotalMemory(true);

            await EventDispatcher.PublishAsync(testEvent);

            long memAfter = GC.GetTotalMemory(false);
            totalAllocated += Math.Max(0, memAfter - memBefore);
        }

        float avgAllocation = totalAllocated / (float)measurementCount;
        TestContext.Out.WriteLine($"AsyncPublish (CompletedTask) average allocation: {avgAllocation:F2} bytes");
        Assert.Less(avgAllocation, 500, "Async publish with CompletedTask should allocate < 500 bytes");
    }

    #endregion
}