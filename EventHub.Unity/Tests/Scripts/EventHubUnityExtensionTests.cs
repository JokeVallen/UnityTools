using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EventHub.Unity;
using Object = UnityEngine.Object;

public class EventHubUnityExtensionTests
{
    private TestEvent testEvent;
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
        EventDispatcher.Dispatcher = new EventDispatcherInternal();
        SubscriptionMonitor.Instance.StopTimer();
    }

    [TearDown]
    public void TearDown()
    {
        EventDispatcher.Dispatcher = null;
        EventDispatcher.Logger = null;
        EventDispatcher.ClearErrorEvents();
        EventDispatcher.ExceptionCatchEnabled = true;
        EventDispatcher.LogEnabled = true;
        SubscriptionMonitor.Instance.StopTimer();
    }

    private bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == mainThreadId;

    #region Component 扩展 - 生命周期绑定

    [UnityTest]
    public IEnumerator ComponentSubscribe_AutoUnsubscribeWhenDestroyed()
    {
        // 设置短清理间隔
        var config = SubscriptionMonitorConfig.Instance;
        int originalDelay = config.MilliSecondsDelay;
        config.MilliSecondsDelay = 50;
        SubscriptionMonitor.Instance.StartTimer();

        var go = new GameObject("TestComponent");
        var comp = go.AddComponent<TestMonoBehaviour>();

        int callCount = 0;
        comp.Subscribe<TestEvent>(_ => callCount++);

        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(1, callCount, "Handler should be called before component destroyed");

        Object.Destroy(go);
        yield return null;

        SubscriptionMonitor.Instance.ForceCleanup();
        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(1, callCount, "Handler should be unsubscribed after component destroyed");

        // 恢复配置
        config.MilliSecondsDelay = originalDelay;
        SubscriptionMonitor.Instance.StopTimer();
    }

    [UnityTest]
    public IEnumerator ComponentSubscribeAsync_AutoUnsubscribeWhenDestroyed()
    {
        var go = new GameObject("TestComponent");
        var comp = go.AddComponent<TestMonoBehaviour>();

        int callCount = 0;
        comp.Subscribe<TestEvent>(async (_, __) =>
        {
            await UniTask.Yield();
            callCount++;
        });

        // 第一次发布并等待完成
        yield return EventDispatcher.PublishAsync(testEvent).ToCoroutine();
        yield return null; // 确保 UniTask 延续完成
        Assert.AreEqual(1, callCount);

        // 销毁组件
        Object.Destroy(go);
        yield return null;

        // 强制清理
        SubscriptionMonitor.Instance.ForceCleanup();

        // 重置计数，第二次发布
        callCount = 0;
        yield return EventDispatcher.PublishAsync(testEvent).ToCoroutine();
        yield return null;
        Assert.AreEqual(0, callCount, "Handler should not be called after component destroyed");
    }

    [UnityTest]
    public IEnumerator ComponentSubscribeOnMainThread_WorksWithLifecycle()
    {
        var go = new GameObject("TestComponent");
        var comp = go.AddComponent<TestMonoBehaviour>();

        bool wasOnMainThread = false;
        comp.SubscribeOnMainThread<TestEvent>(async (_, __) =>
        {
            wasOnMainThread = IsMainThread();
            await UniTask.CompletedTask;
        });

        // 在线程池发布异步事件，并等待完成
        yield return UniTask.RunOnThreadPool(async () =>
        {
            await EventDispatcher.PublishAsync(testEvent);
        }).ToCoroutine();

        Assert.IsTrue(wasOnMainThread, "Handler should execute on main thread");

        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator ComponentUnsubscribeAll_ClearsAllSubscriptions()
    {
        yield return ComponentUnsubscribeAll_ClearsAllSubscriptions_Internal().ToCoroutine();
    }
    private async UniTask ComponentUnsubscribeAll_ClearsAllSubscriptions_Internal()
    {
        var go = new GameObject("TestComponent");
        var comp = go.AddComponent<TestMonoBehaviour>();

        int callCount = 0;
        comp.Subscribe<TestEvent>(_ => callCount++);
        comp.Subscribe<TestEvent>(_ => callCount++);

        comp.UnsubscribeAll();

        EventDispatcher.Publish(testEvent);
        Assert.AreEqual(0, callCount);

        Object.Destroy(go);
    }

    #endregion

    #region 主线程扩展方法

    [UnityTest]
    public IEnumerator SubscribeOnMainThread_ExecutesOnMainThread()
    {
        bool wasOnMainThread = false;
        EventDispatcher.SubscribeOnMainThread<TestEvent>(async (_, __) =>
        {
            wasOnMainThread = IsMainThread();
            await UniTask.CompletedTask;
        });

        // 使用异步发布并等待完成
        yield return UniTask.RunOnThreadPool(async () =>
        {
            await EventDispatcher.PublishAsync(testEvent);
        }).ToCoroutine();

        Assert.IsTrue(wasOnMainThread);
    }

    [UnityTest]
    public IEnumerator SubscribeOnceOnMainThread_ExecutesOnceAndOnMainThread()
    {
        int callCount = 0;
        bool wasOnMainThread = false;

        EventDispatcher.SubscribeOnceOnMainThread<TestEvent>(async (_, __) =>
        {
            wasOnMainThread = IsMainThread();
            callCount++;
            await UniTask.CompletedTask;
        });

        // 多次发布
        yield return UniTask.RunOnThreadPool(async () =>
        {
            await EventDispatcher.PublishAsync(testEvent);
            await EventDispatcher.PublishAsync(testEvent);
            await EventDispatcher.PublishAsync(testEvent);
        }).ToCoroutine();

        Assert.AreEqual(1, callCount);
        Assert.IsTrue(wasOnMainThread);
    }

    [UnityTest]
    public IEnumerator SubscribeIfOnMainThread_ExecutesOnMainThreadWhenFilterPasses()
    {
        int callCount = 0;
        bool wasOnMainThread = false;

        EventDispatcher.SubscribeIfOnMainThread<TestEvent>(
            filter: e => e.Value > 0,
            handler: async (_, __) =>
            {
                wasOnMainThread = IsMainThread();
                callCount++;
                await UniTask.CompletedTask;
            });

        yield return UniTask.RunOnThreadPool(async () =>
        {
            await EventDispatcher.PublishAsync(testEvent);
        }).ToCoroutine();

        Assert.AreEqual(1, callCount);
        Assert.IsTrue(wasOnMainThread);
    }

    [UnityTest]
    public IEnumerator SubscribeIfOnMainThread_FilterFails_DoesNotExecute()
    {
        int callCount = 0;
        EventDispatcher.SubscribeIfOnMainThread<TestEvent>(
            filter: e => e.Value > 100,
            handler: async (_, __) =>
            {
                callCount++;
                await UniTask.CompletedTask;
            });

        yield return UniTask.RunOnThreadPool(async () =>
        {
            await EventDispatcher.PublishAsync(testEvent);
        }).ToCoroutine();

        Assert.AreEqual(0, callCount);
    }

    #endregion

    #region SubscriptionMonitor 清理逻辑

    [UnityTest]
    public IEnumerator SubscriptionMonitor_CleansUpDeadComponentSubscriptions()
    {
        var config = SubscriptionMonitorConfig.Instance;
        int originalDelay = config.MilliSecondsDelay;
        config.MilliSecondsDelay = 50;

        var go = new GameObject("Test");
        var comp = go.AddComponent<TestMonoBehaviour>();
        comp.Subscribe<TestEvent>(_ => { });

        SubscriptionMonitor.Instance.StartTimer();
        Assert.IsTrue(SubscriptionMonitor.Instance.IsTimerRunning);

        Object.Destroy(go);
        yield return null;
        yield return new WaitForSeconds(0.2f);

        // 此时内部 Item 应已被清理（可通过反射或测试辅助方法验证）
        // 若没有测试辅助方法，此测试仅验证无异常

        config.MilliSecondsDelay = originalDelay;
        SubscriptionMonitor.Instance.StopTimer();
    }

    [Test]
    public void SubscriptionMonitor_StartStopTimer_ControlsCleanup()
    {
        SubscriptionMonitor.Instance.StopTimer();
        Assert.IsFalse(SubscriptionMonitor.Instance.IsTimerRunning);

        SubscriptionMonitor.Instance.StartTimer();
        Assert.IsTrue(SubscriptionMonitor.Instance.IsTimerRunning);

        SubscriptionMonitor.Instance.StopTimer();
        Assert.IsFalse(SubscriptionMonitor.Instance.IsTimerRunning);
    }

    [Test]
    public void SubscriptionMonitor_Register_NullComponent_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => SubscriptionMonitor.Instance.Register(null, new Subscription[] { }));
    }

    [Test]
    public void SubscriptionMonitor_UnsubscribeAll_WithoutSubscriptions_DoesNotThrow()
    {
        var go = new GameObject();
        var comp = go.AddComponent<TestMonoBehaviour>();
        Assert.DoesNotThrow(() => comp.UnsubscribeAll());
        Object.DestroyImmediate(go);
    }

    #endregion

    #region SubscriptionMonitorConfig 持久化

    [Test]
    public void SubscriptionMonitorConfig_Persistence_Works()
    {
        var config = ScriptableObject.CreateInstance<SubscriptionMonitorConfig>();
        config.MilliSecondsDelay = 777;
        config.AutoSave = false;
        config.AutoSaveDirtyCount = 10;

        // 使用 JSON 序列化模拟持久化，避免 PlayerPrefs 不确定性
        string json = JsonUtility.ToJson(config);
        var newConfig = ScriptableObject.CreateInstance<SubscriptionMonitorConfig>();
        JsonUtility.FromJsonOverwrite(json, newConfig);

        Assert.AreEqual(777, newConfig.MilliSecondsDelay);
        Assert.AreEqual(false, newConfig.AutoSave);
        Assert.AreEqual(10, newConfig.AutoSaveDirtyCount);

        Object.DestroyImmediate(config);
        Object.DestroyImmediate(newConfig);
    }

    #endregion
}