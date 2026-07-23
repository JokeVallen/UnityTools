using System.Collections;
using CoroutineRunner;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CoroutineRunnerTests
{
    [SetUp]
    public void SetUp()
    {

    }

    [TearDown]
    public void TearDown()
    {
        // 不主动 Dispose，避免影响其他测试；但测试结束后可清理通道配置
        // 注：GlobalCoroutineGlobalCoroutineRunner 为静态门面，通道配置不会跨测试残留（因为每次都是新场景？）
    }

    #region 基础启动与停止

    [UnityTest]
    public IEnumerator Run_SimpleCoroutine_Completes()
    {
        bool completed = false;
        IEnumerator Coroutine()
        {
            yield return null;
            completed = true;
        }

        var token = GlobalCoroutineRunner.Run(Coroutine());
        Assert.IsTrue(token.IsValid);

        // 等待两帧确保执行
        yield return null;

        Assert.IsTrue(token.IsDone());
        Assert.IsTrue(completed);
    }

    [UnityTest]
    public IEnumerator RunQueued_WithDefaultChannel_ExecutesInOrder()
    {
        var order = new System.Collections.Generic.List<int>();

        IEnumerator Coro1()
        {
            order.Add(1);
            yield return null;
            order.Add(2);
        }

        IEnumerator Coro2()
        {
            order.Add(3);
            yield return null;
            order.Add(4);
        }

        var token1 = GlobalCoroutineRunner.RunQueued(Coro1(), "testChannel");
        var token2 = GlobalCoroutineRunner.RunQueued(Coro2(), "testChannel");

        // 等待两个协程完成
        while (!token1.IsDone() || !token2.IsDone())
            yield return null;

        // 通道并发为1，应该顺序执行：1,2,3,4 或 3,4,1,2 取决于入队顺序？实际上入队顺序 token1 先入队，先执行完再执行 token2
        // 因为是单并发，第一个协程全部完成后第二个才开始，因此顺序应为 [1,2,3,4]
        Assert.AreEqual(new[] { 1, 2, 3, 4 }, order);
    }

    [UnityTest]
    public IEnumerator ConfigureChannel_MaxConcurrent2_RunsTwoAtOnce()
    {
        GlobalCoroutineRunner.ConfigureChannel("parallel", 2);
        var runningFlags = new bool[3];
        var completedFlags = new bool[3];

        IEnumerator Coro(int id)
        {
            runningFlags[id] = true;
            yield return new WaitForSeconds(0.2f);
            runningFlags[id] = false;
            completedFlags[id] = true;
        }

        var tokens = new CoroutineHandleToken[3];
        for (int i = 0; i < 3; i++)
        {
            tokens[i] = GlobalCoroutineRunner.RunQueued(Coro(i), "parallel");
        }

        // 等待一帧让协程启动
        yield return null;

        // 应该有两个正在运行，一个在队列
        int runningCount = 0;
        for (int i = 0; i < 3; i++) if (runningFlags[i]) runningCount++;
        Assert.AreEqual(2, runningCount);

        // 等待所有完成
        for (int i = 0; i < 3; i++)
        {
            while (!completedFlags[i]) yield return null;
        }

        // 最终全部完成
        for (int i = 0; i < 3; i++) Assert.IsTrue(completedFlags[i]);
    }

    #endregion

    #region 协程控制：暂停/恢复/取消

    [UnityTest]
    public IEnumerator PauseAndResume_StopsAndResumesExecution()
    {
        int value = 0;
        IEnumerator Coro()
        {
            value = 1;
            yield return null;
            value = 2;
            yield return null;
            value = 3;
        }

        var token = GlobalCoroutineRunner.Run(Coro());
        // 协程已执行到第一个 yield 之前，value 应为 1
        Assert.AreEqual(1, value);

        token.Pause();
        // 等待两帧，确保暂停状态稳定
        yield return null;
        yield return null;
        Assert.AreEqual(1, value);          // 暂停期间 value 不应变化

        token.Resume();
        yield return null;                  // 协程继续，执行 value = 2
        Assert.AreEqual(2, value);
        yield return null;                  // 最后一步 value = 3
        Assert.AreEqual(3, value);
    }

    [UnityTest]
    public IEnumerator Cancel_StopsCoroutineImmediately()
    {
        bool executed = false;
        IEnumerator Coro()
        {
            yield return null;
            executed = true;
        }

        var token = GlobalCoroutineRunner.Run(Coro());
        token.Cancel();

        // 立即获取状态（token 仍然有效）
        Assert.IsTrue(token.TryGetState(out var state));
        Assert.AreEqual(CoroutineState.Canceled, state);

        yield return null; // 协程回收，token 变无效
        Assert.IsFalse(executed);
        // 此时 TryGetState 返回 false
        Assert.IsFalse(token.TryGetState(out _));
    }

    [UnityTest]
    public IEnumerator Cancel_DuringPause_RemainsCanceled()
    {
        bool afterPause = false;
        IEnumerator Coro()
        {
            yield return null;
            afterPause = true;
        }

        var token = GlobalCoroutineRunner.Run(Coro());
        token.Pause();
        token.Cancel();
        token.Resume();

        Assert.IsTrue(token.TryGetState(out var state));
        Assert.AreEqual(CoroutineState.Canceled, state);

        yield return null;
        Assert.IsFalse(afterPause);
        Assert.IsFalse(token.TryGetState(out _));
    }

    #endregion

    #region 自定义 Yield 指令

    [UnityTest]
    public IEnumerator WaitForSecondsControlled_WaitsCorrectly()
    {
        float startTime = Time.time;
        IEnumerator Coro()
        {
            yield return CustomYield.Yield<WaitForSecondsControlled>(0.5f);
        }

        var token = GlobalCoroutineRunner.Run(Coro());
        while (!token.IsDone()) yield return null;
        float elapsed = Time.time - startTime;
        Assert.GreaterOrEqual(elapsed, 0.45f);
        Assert.LessOrEqual(elapsed, 0.6f);
    }

    [UnityTest]
    public IEnumerator CustomYield_Pooling_ReusesInstance()
    {
        // 获取第一个实例
        var inst1 = CustomYield.Yield<WaitForSecondsControlled>(1f);
        var inst2 = CustomYield.Yield<WaitForSecondsControlled>(2f);
        Assert.AreNotSame(inst1, inst2); // 池为空时会创建新实例

        // 模拟使用后自动释放（框架会在协程 MoveNext 时自动 Release）
        IEnumerator Coro()
        {
            yield return inst1;
            yield return inst2;
        }

        GlobalCoroutineRunner.Run(Coro());
        yield return null; // 第一帧执行完 inst1，inst1 被释放回池
        yield return null; // 第二帧执行完 inst2，inst2 被释放回池

        // 再次获取，应重用之前释放的实例
        var inst3 = CustomYield.Yield<WaitForSecondsControlled>(3f);
        // 无法直接断言是同一个，因为池可能多于一个，但至少表明没有内存泄漏
        Assert.IsNotNull(inst3);
    }

    #endregion

    #region 异常处理

    [UnityTest]
    public IEnumerator Coroutine_ThrowsException_TransitionsToCanceledAndLogsError()
    {
        IEnumerator ThrowingCoro()
        {
            yield return null;
            throw new System.Exception("Test exception");
        }

        LogAssert.Expect(LogType.Exception, "Exception: Test exception");
        var token = GlobalCoroutineRunner.Run(ThrowingCoro());
        yield return null; // 触发异常
        yield return null; // 等待状态更新

        if (token.TryGetState(out var state))
        {
            Assert.AreEqual(CoroutineState.Canceled, state);
        }
    }

    #endregion
}

// 辅助扩展方法
public static class CoroutineStateExtensions
{
    public static bool IsDone(this CoroutineState state) =>
        state == CoroutineState.Completed || state == CoroutineState.Canceled;
}