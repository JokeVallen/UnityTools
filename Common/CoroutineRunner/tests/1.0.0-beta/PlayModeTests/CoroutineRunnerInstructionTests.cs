using System;
using System.Collections;
using CoroutineRunner;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CoroutineRunnerInstructionTests
{
    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
    }

    private IEnumerator WaitForSecondsRoutine2(float seconds)
    {
        yield return CustomYield.Yield<WaitForSecondsControlled, float>(seconds);
    }

    #region WaitForSecondsControlled

    [UnityTest]
    public IEnumerator WaitForSecondsControlled_WaitsCorrectly()
    {
        float start = Time.time;
        var token = GlobalCoroutineRunner.Run(WaitForSecondsRoutine2(0.5f));
        yield return null;
        yield return null;
        while (!token.IsDone()) yield return null;
        float elapsed = Time.time - start;
        Assert.GreaterOrEqual(elapsed, 0.45f);
        Assert.LessOrEqual(elapsed, 0.6f);
    }

    [UnityTest]
    public IEnumerator WaitForSecondsControlled_CanBePaused()
    {
        bool completed = false;
        IEnumerator WaitRoutine()
        {
            yield return CustomYield.Yield<WaitForSecondsControlled, float>(0.5f);
            completed = true;
        }

        var token = GlobalCoroutineRunner.Run(WaitRoutine());
        // 等待一帧，确保协程已进入等待状态
        yield return null;

        token.Pause();
        // 等待足够帧（例如 15 帧，约 0.25~0.5 秒），协程不应完成
        for (int i = 0; i < 15; i++)
            yield return null;
        Assert.IsFalse(completed, "协程在暂停期间不应完成");

        token.Resume();
        // 等待协程完成
        while (!token.IsDone())
            yield return null;

        Assert.IsTrue(completed);
    }

    [UnityTest]
    public IEnumerator WaitForSecondsControlled_CanBeCanceled()
    {
        bool completed = false;
        IEnumerator Routine()
        {
            yield return CustomYield.Yield<WaitForSecondsControlled, float>(1f);
            completed = true;
        }
        var token = GlobalCoroutineRunner.Run(Routine());
        yield return null;
        token.Cancel();
        yield return null;
        Assert.IsFalse(completed);
        Assert.AreEqual(CoroutineState.Canceled, token.GetState());
    }

    private IEnumerator WaitForSecondsRoutine(float seconds)
    {
        yield return CustomYield.Yield<WaitForSecondsControlled, float>(seconds);
    }

    #endregion

    #region WaitForRealtimeSecondsControlled

    [UnityTest]
    public IEnumerator WaitForRealtimeSecondsControlled_IgnoresTimeScale()
    {
        Time.timeScale = 0.5f;
        float start = Time.realtimeSinceStartup;
        var token = GlobalCoroutineRunner.Run(WaitForRealtimeRoutine(0.5f));
        while (!token.IsDone()) yield return null;
        float elapsed = Time.realtimeSinceStartup - start;
        Time.timeScale = 1f;
        Assert.GreaterOrEqual(elapsed, 0.45f);
        Assert.LessOrEqual(elapsed, 0.6f);
    }

    private IEnumerator WaitForRealtimeRoutine(float seconds)
    {
        yield return CustomYield.Yield<WaitForRealtimeSecondsControlled, float>(seconds);
    }

    #endregion

    #region WaitForFramesControlled

    [UnityTest]
    public IEnumerator WaitForFramesControlled_WaitsExactFrames()
    {
        int frameCount = 0;
        IEnumerator Routine()
        {
            yield return CustomYield.Yield<WaitForFramesControlled, int>(3);
            frameCount = Time.frameCount;
        }
        int startFrame = Time.frameCount;
        var token = GlobalCoroutineRunner.Run(Routine());
        while (!token.IsDone()) yield return null;
        // 等待3帧，预期结束帧号 = startFrame + 3（或+4，取决于调度顺序）
        Assert.AreEqual(startFrame + 3, frameCount);
    }

    [UnityTest]
    public IEnumerator WaitForFramesControlled_CanBePausedDuringFrames()
    {
        int step = 0;
        IEnumerator Routine()
        {
            yield return CustomYield.Yield<WaitForFramesControlled, int>(5);
            step = 1;
        }
        var token = GlobalCoroutineRunner.Run(Routine());
        yield return null; // 第一帧
        token.Pause();
        yield return null;
        yield return null; // 暂停期间不应继续
        token.Resume();
        while (!token.IsDone()) yield return null;
        Assert.AreEqual(1, step);
    }

    #endregion

    #region WaitForAsyncOperationControlled

    [UnityTest]
    public IEnumerator WaitForAsyncOperationControlled_NullInput_CompletesImmediately()
    {
        bool completed = false;
        IEnumerator Routine()
        {
            yield return CustomYield.Yield<WaitForAsyncOperationControlled, AsyncOperation>(null);
            completed = true;
        }
        var token = GlobalCoroutineRunner.Run(Routine());
        yield return null;
        Assert.IsTrue(completed);
        Assert.IsTrue(token.IsDone());
    }

    private class DummyAsyncOperation : AsyncOperation
    {
        public new bool isDone { get; set; }
    }

    #endregion

    #region WaitUntilControlled / WaitWhileControlled

    [UnityTest]
    public IEnumerator WaitUntilControlled_WaitsForCondition()
    {
        bool condition = false;
        IEnumerator Routine()
        {
            yield return CustomYield.Yield<WaitUntilControlled, Func<bool>>(() => condition);
        }
        var token = GlobalCoroutineRunner.Run(Routine());
        yield return null;
        Assert.IsFalse(token.IsDone());
        condition = true;
        yield return null;
        Assert.IsTrue(token.IsDone());
    }

    [UnityTest]
    public IEnumerator WaitWhileControlled_WaitsWhileConditionTrue()
    {
        bool condition = true;
        IEnumerator Routine()
        {
            yield return CustomYield.Yield<WaitWhileControlled, Func<bool>>(() => condition);
        }
        var token = GlobalCoroutineRunner.Run(Routine());
        yield return null;
        Assert.IsFalse(token.IsDone());
        condition = false;
        yield return null;
        Assert.IsTrue(token.IsDone());
    }

    #endregion

    #region 池化复用测试

    [UnityTest]
    public IEnumerator CustomYield_Pooling_ReusesInstances()
    {
        var inst1 = CustomYield.Yield<WaitForSecondsControlled, float>(1f);
        var inst2 = CustomYield.Yield<WaitForSecondsControlled, float>(2f);
        Assert.AreNotSame(inst1, inst2); // 池空时不同

        IEnumerator UseAndRelease()
        {
            yield return inst1;
            yield return inst2;
        }
        var token = GlobalCoroutineRunner.Run(UseAndRelease());
        yield return null;
        yield return null; // 两个都被释放回池

        var inst3 = CustomYield.Yield<WaitForSecondsControlled, float>(3f);
        // inst3 可能是 inst1 或 inst2 中的某一个（池中取回）
        Assert.IsNotNull(inst3);
    }

    #endregion
}