#if UNITY_EDITOR

using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Timer;

/// <summary>
/// 编辑器计时器单元测试（在非运行模式下执行）
/// </summary>
public class EditorTimerUnitTests
{
    private const int TEST_GROUP = 54321;
    private const float FIXED_DELTA = 0.008f; // 模拟 120 FPS，提高精度

    [SetUp]
    public void SetUp()
    {
        if (Application.isPlaying)
        {
            Assert.Ignore("EditorTimer tests must be run in EditMode (non-PlayMode).");
        }

        EditorTimer.CancelAll();
    }

    [TearDown]
    public void TearDown()
    {
        EditorTimer.CancelAll();
    }

    /// <summary>
    /// 推进编辑器时间（基于固定帧率模拟）
    /// </summary>
    private void AdvanceEditorTime(float seconds)
    {
        int ticks = Mathf.Max(1, Mathf.CeilToInt(seconds / FIXED_DELTA));
        for (int i = 0; i < ticks; i++)
        {
            EditorTimer.TestTickUpdate();
        }
    }

    /// <summary>
    /// 推进一帧（用于帧驱动测试）
    /// </summary>
    private void AdvanceOneFrame()
    {
        EditorTimer.TestTickUpdate();
    }

    [Test]
    public void RegisterScaled_TriggersAfterInterval()
    {
        bool triggered = false;
        var handle = EditorTimer.RegisterScaled(0.2f, () => triggered = true, loop: false);
        Assert.IsFalse(handle.IsNull);
        AdvanceEditorTime(0.3f);
        Assert.IsTrue(triggered);
        Assert.IsFalse(handle.IsActive());
    }

    [Test]
    public void Register_Loop_TriggersMultipleTimes()
    {
        int count = 0;
        var handle = EditorTimer.RegisterScaled(0.1f, () => count++, loop: true);
        AdvanceEditorTime(0.35f);
        handle.Cancel();
        Assert.That(count, Is.GreaterThanOrEqualTo(2).And.LessThan(6));
    }

    [Test]
    public void Register_WithGroup_CancelGroupCancelsAll()
    {
        bool flag1 = false, flag2 = false;
        var h1 = EditorTimer.RegisterScaled(0.5f, () => flag1 = true, loop: false, groupID: TEST_GROUP);
        var h2 = EditorTimer.RegisterScaled(0.5f, () => flag2 = true, loop: false, groupID: TEST_GROUP);
        EditorTimer.CancelGroup(TEST_GROUP);
        AdvanceEditorTime(0.6f);
        Assert.IsFalse(flag1);
        Assert.IsFalse(flag2);
        Assert.IsFalse(h1.IsActive());
        Assert.IsFalse(h2.IsActive());
    }

    [Test]
    public void Pause_Resume_StopsAndResumes()
    {
        int count = 0;
        var handle = EditorTimer.RegisterScaled(0.1f, () => count++, loop: true);
        AdvanceEditorTime(0.15f);
        int snapshotBeforePause = count;
        Assert.That(snapshotBeforePause, Is.GreaterThan(0));

        handle.Pause();
        AdvanceEditorTime(0.2f);
        Assert.AreEqual(snapshotBeforePause, count);

        handle.Resume();
        AdvanceEditorTime(0.15f);
        Assert.That(count, Is.GreaterThan(snapshotBeforePause));
        handle.Cancel();
    }

    [Test]
    public void SetPaused_Group_PausesAllGroupMembers()
    {
        int countA = 0, countB = 0;
        var h1 = EditorTimer.RegisterScaled(0.1f, () => countA++, loop: true, groupID: TEST_GROUP);
        var h2 = EditorTimer.RegisterScaled(0.1f, () => countB++, loop: true, groupID: TEST_GROUP);

        AdvanceEditorTime(0.15f);
        int snapshotA = countA, snapshotB = countB;
        Assert.That(snapshotA, Is.GreaterThan(0));
        Assert.That(snapshotB, Is.GreaterThan(0));

        EditorTimer.SetGroupPaused(TEST_GROUP, true);
        AdvanceEditorTime(0.2f);
        Assert.AreEqual(snapshotA, countA);
        Assert.AreEqual(snapshotB, countB);

        EditorTimer.SetGroupPaused(TEST_GROUP, false);
        AdvanceEditorTime(0.15f);
        Assert.That(countA, Is.GreaterThan(snapshotA));
        Assert.That(countB, Is.GreaterThan(snapshotB));

        EditorTimer.CancelGroup(TEST_GROUP);
    }

    [Test]
    public void Reset_ResetsRemainingTime()
    {
        var handle = EditorTimer.RegisterScaled(1f, () => { }, loop: false);
        AdvanceEditorTime(0.5f);
        handle.TryGetTimeRemaining(out float remaining);
        Assert.That(remaining, Is.LessThan(0.6f).And.GreaterThan(0.4f));

        handle.Reset();
        handle.TryGetTimeRemaining(out float newRemaining);
        Assert.That(newRemaining, Is.EqualTo(1f).Within(0.05f));
        handle.Cancel();
    }

    [Test]
    public void SetInterval_ChangesInterval()
    {
        int count = 0;
        var handle = EditorTimer.RegisterScaled(1f, () => count++, loop: true);
        AdvanceEditorTime(1.2f);
        Assert.AreEqual(1, count);

        handle.SetInterval(0.2f);
        AdvanceEditorTime(0.5f);
        Assert.That(count, Is.GreaterThanOrEqualTo(2));
        handle.Cancel();
    }

    [Test]
    public void TryGetProgress_ReturnsValue()
    {
        var handle = EditorTimer.RegisterScaled(2f, () => { }, loop: false);
        AdvanceEditorTime(0.5f);
        Assert.IsTrue(handle.TryGetProgress(out float progress));
        Assert.That(progress, Is.EqualTo(0.25f).Within(0.05f));
        handle.Cancel();
    }

    [Test]
    public void FrameTimer_RemainingFrames()
    {
        var handle = EditorTimer.RegisterFrame(5, () => { }, loop: false);
        Assert.IsTrue(handle.TryGetFramesRemainingInt(out int frames));
        Assert.AreEqual(5, frames);

        AdvanceOneFrame();
        handle.TryGetFramesRemainingInt(out frames);
        Assert.AreEqual(4, frames);
        handle.Cancel();
    }

    [Test]
    public void SetLoop_ChangesLoopBehavior()
    {
        int count = 0;
        var handle = EditorTimer.RegisterScaled(0.2f, () => count++, loop: false);
        handle.SetLoop(true);
        AdvanceEditorTime(0.5f);
        Assert.That(count, Is.GreaterThanOrEqualTo(2));
        handle.Cancel();
    }

    [Test]
    public void Register_NullCallback_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => EditorTimer.RegisterScaled(1f, null));
        Assert.Throws<ArgumentNullException>(() => EditorTimer.RegisterFrame(1, null));
        Assert.Throws<ArgumentNullException>(() => EditorTimer.RegisterManual(1f, null));
        Assert.Throws<ArgumentNullException>(() => EditorTimer.RegisterWallClock(1f, null));
        Assert.Throws<ArgumentNullException>(() => EditorTimer.RegisterIndependent(1f, null, 1f));
    }

    [Test]
    public void Register_NegativeInterval_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EditorTimer.RegisterScaled(-1f, () => { }));
        Assert.Throws<ArgumentOutOfRangeException>(() => EditorTimer.RegisterFrame(-1, () => { }));
    }

    [Test]
    public void Cancel_InvalidHandle_DoesNothing()
    {
        var invalidHandle = EditorTimerHandle.Null;
        Assert.DoesNotThrow(() => invalidHandle.Cancel());
        Assert.IsFalse(invalidHandle.IsActive());
    }

    [Test]
    public void TryGetGroupId_ReturnsCorrectGroup()
    {
        var handle = EditorTimer.RegisterScaled(1f, () => { }, groupID: 42);
        Assert.IsTrue(handle.TryGetGroupId(out int gid));
        Assert.AreEqual(42, gid);
        handle.Cancel();
    }

    [Test]
    public void TryGetGroupId_NoGroup_ReturnsZero()
    {
        var handle = EditorTimer.RegisterScaled(1f, () => { });
        Assert.IsTrue(handle.TryGetGroupId(out int gid));
        Assert.AreEqual(0, gid);
        handle.Cancel();
    }

    [Test]
    public void TryGetInterval_ReturnsInterval()
    {
        var handle = EditorTimer.RegisterScaled(3.5f, () => { });
        Assert.IsTrue(handle.TryGetInterval(out float interval));
        Assert.AreEqual(3.5f, interval);
        handle.Cancel();
    }

    [Test]
    public void TryGetIsLoop_ReturnsLoopStatus()
    {
        var handle = EditorTimer.RegisterScaled(1f, () => { }, loop: true);
        Assert.IsTrue(handle.TryGetIsLoop(out bool isLoop));
        Assert.IsTrue(isLoop);

        handle.SetLoop(false);
        Assert.IsTrue(handle.TryGetIsLoop(out isLoop));
        Assert.IsFalse(isLoop);
        handle.Cancel();
    }

    [Test]
    public void RegisterIndependent_WithScale_TriggersAtScaledRate()
    {
        int count = 0;
        var handle = EditorTimer.RegisterIndependent(0.2f, () => count++, customScale: 2f, loop: true);
        AdvanceEditorTime(0.35f);
        handle.Cancel();
        Assert.That(count, Is.GreaterThanOrEqualTo(2).And.LessThan(6));
    }

    [Test]
    public void RegisterWallClock_TriggersAfterRealTime()
    {
        bool triggered = false;
        var handle = EditorTimer.RegisterWallClock(0.2f, () => triggered = true, loop: false);
        AdvanceEditorTime(0.3f);
        Assert.IsTrue(triggered);
        Assert.IsFalse(handle.IsActive());
    }

    [Test]
    public void RegisterManual_OnlyAdvancesWhenManualUpdateCalled()
    {
        bool triggered = false;
        var handle = EditorTimer.RegisterManual(0.1f, () => triggered = true, loop: false);

        AdvanceEditorTime(0.2f);
        Assert.IsFalse(triggered);

        EditorTimer.ManualUpdate(0.15f);
        Assert.IsTrue(triggered);
        handle.Cancel();
    }

    [Test]
    public void RegisterIndependentFrame_WithScale_AdvancesFrameRateScaled()
    {
        int count = 0;
        var handle = EditorTimer.RegisterIndependentFrame(1, () => count++, customScale: 2f, loop: true);

        AdvanceOneFrame();
        Assert.AreEqual(1, count, "2倍速，一帧后触发1次");

        AdvanceOneFrame();
        Assert.AreEqual(2, count, "2倍速，两帧后触发2次");

        handle.Cancel();
    }

    [Test]
    public void CancelAll_CancelsAllTimers()
    {
        bool flag1 = false, flag2 = false;
        // 使用循环计时器，确保 CancelAll 之前不会自动消失
        var h1 = EditorTimer.RegisterScaled(0.5f, () => flag1 = true, loop: true);
        var h2 = EditorTimer.RegisterScaled(0.5f, () => flag2 = true, loop: true);

        AdvanceEditorTime(0.1f);
        EditorTimer.CancelAll();
        AdvanceEditorTime(0.6f);

        Assert.IsFalse(flag1);
        Assert.IsFalse(flag2);
        Assert.IsFalse(h1.IsActive());
        Assert.IsFalse(h2.IsActive());

        // 验证可以继续注册
        bool newTrigger = false;
        var h3 = EditorTimer.RegisterScaled(0.1f, () => newTrigger = true, loop: false);
        AdvanceEditorTime(0.15f);
        Assert.IsTrue(newTrigger);
        h3.Cancel();
    }

    [Test]
    public void CancelAll_WithNoTimers_DoesNothing()
    {
        Assert.DoesNotThrow(() => EditorTimer.CancelAll());
        Assert.AreEqual(0, EditorTimer.TestActiveCount);
    }

    [Test]
    public void CancelGroup_WithInvalidGroup_DoesNothing()
    {
        var handle = EditorTimer.RegisterScaled(1f, () => { }, groupID: 42);
        Assert.DoesNotThrow(() => EditorTimer.CancelGroup(999));
        Assert.IsTrue(handle.IsActive());
        handle.Cancel();
    }

    [Test]
    public void RegisterUnsupportedSchedule_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
        {
            EditorTimer.Register(
                0.1f,
                () => { },
                TimeDelta.Scaled,
                TimeSchedule.LateUpdate
            );
        });

        Assert.Throws<NotSupportedException>(() =>
        {
            EditorTimer.Register(
                0.1f,
                () => { },
                TimeDelta.Unscaled,
                TimeSchedule.FixedUpdate
            );
        });

        Assert.Throws<NotSupportedException>(() =>
        {
            EditorTimer.Register(
                0.1f,
                () => { },
                TimeDelta.Frame,
                TimeSchedule.Coroutine
            );
        });
    }

    [Test]
    public void RegisterSupportedSchedule_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            EditorTimer.Register(
                0.1f,
                () => { },
                TimeDelta.Scaled,
                TimeSchedule.Update
            );
        });

        Assert.DoesNotThrow(() =>
        {
            EditorTimer.Register(
                0.1f,
                () => { },
                TimeDelta.Manual,
                TimeSchedule.Manual
            );
        });
    }

    [Test]
    public void StressTest_ManyTimers()
    {
        const int TIMER_COUNT = 100;
        int triggerCount = 0;
        var handles = new EditorTimerHandle[TIMER_COUNT];

        for (int i = 0; i < TIMER_COUNT; i++)
        {
            handles[i] = EditorTimer.RegisterScaled(0.1f, () => triggerCount++, loop: false);
        }

        AdvanceEditorTime(0.2f);

        Assert.AreEqual(TIMER_COUNT, triggerCount);

        foreach (var h in handles)
        {
            Assert.IsFalse(h.IsActive());
        }
    }

    [Test]
    public void StressTest_ManyLoopingTimers()
    {
        const int TIMER_COUNT = 50;
        int triggerCount = 0;
        var handles = new EditorTimerHandle[TIMER_COUNT];

        for (int i = 0; i < TIMER_COUNT; i++)
        {
            handles[i] = EditorTimer.RegisterScaled(0.05f, () => triggerCount++, loop: true);
        }

        AdvanceEditorTime(0.25f);

        Assert.That(triggerCount, Is.GreaterThanOrEqualTo(TIMER_COUNT * 3));

        foreach (var h in handles)
        {
            h.Cancel();
        }
        Assert.AreEqual(0, EditorTimer.TestActiveCount);
    }
}

#endif