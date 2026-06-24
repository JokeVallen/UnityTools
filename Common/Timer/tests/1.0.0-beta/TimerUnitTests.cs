using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Timer;
using System;

public class TimerUnitTests
{
    private const int TEST_GROUP = 12345;
    private const int INVALID_GROUP = 99999;

    [SetUp]
    public void SetUp()
    {
        // 确保任何残留计时被清理（通过组取消）
        GlobalTimer.CancelGroup(TEST_GROUP);
    }

    [UnityTest]
    public IEnumerator Register_Scaled_TriggersAfterInterval()
    {
        bool triggered = false;
        var handle = GlobalTimer.RegisterScaled(0.2f, () => triggered = true, loop: false);
        Assert.IsFalse(handle.IsNull);
        yield return new WaitForSeconds(0.3f);
        Assert.IsTrue(triggered);
        Assert.IsFalse(handle.IsActive());
    }

    [UnityTest]
    public IEnumerator Register_Loop_TriggersMultipleTimes()
    {
        int count = 0;
        var handle = GlobalTimer.RegisterScaled(0.1f, () => count++, loop: true);
        yield return new WaitForSeconds(0.35f);
        handle.Cancel();
        Assert.That(count, Is.GreaterThanOrEqualTo(3).And.LessThan(5));
    }

    [UnityTest]
    public IEnumerator Register_WithGroup_CancelGroupCancelsAll()
    {
        bool flag1 = false, flag2 = false;
        var h1 = GlobalTimer.RegisterScaled(0.5f, () => flag1 = true, loop: false, groupID: TEST_GROUP);
        var h2 = GlobalTimer.RegisterScaled(0.5f, () => flag2 = true, loop: false, groupID: TEST_GROUP);
        GlobalTimer.CancelGroup(TEST_GROUP);
        yield return new WaitForSeconds(0.6f);
        Assert.IsFalse(flag1);
        Assert.IsFalse(flag2);
        Assert.IsFalse(h1.IsActive());
        Assert.IsFalse(h2.IsActive());
    }

    [UnityTest]
    public IEnumerator Pause_Resume_StopsAndResumes()
    {
        int count = 0;
        var handle = GlobalTimer.RegisterScaled(0.1f, () => count++, loop: true);
        yield return new WaitForSeconds(0.15f);
        handle.Pause();
        int countAfterPause = count;
        yield return new WaitForSeconds(0.2f);
        Assert.AreEqual(countAfterPause, count);
        handle.Resume();
        yield return new WaitForSeconds(0.15f);
        Assert.That(count, Is.GreaterThan(countAfterPause));
        handle.Cancel();
    }

    [UnityTest]
    public IEnumerator SetPaused_Group_PausesAllGroupMembers()
    {
        int countA = 0, countB = 0;
        var h1 = GlobalTimer.RegisterScaled(0.1f, () => countA++, loop: true, groupID: TEST_GROUP);
        var h2 = GlobalTimer.RegisterScaled(0.1f, () => countB++, loop: true, groupID: TEST_GROUP);
        yield return new WaitForSeconds(0.15f);
        GlobalTimer.SetGroupPaused(TEST_GROUP, true);
        int snapshotA = countA, snapshotB = countB;
        yield return new WaitForSeconds(0.2f);
        Assert.AreEqual(snapshotA, countA);
        Assert.AreEqual(snapshotB, countB);
        GlobalTimer.SetGroupPaused(TEST_GROUP, false);
        yield return new WaitForSeconds(0.15f);
        Assert.That(countA, Is.GreaterThan(snapshotA));
        Assert.That(countB, Is.GreaterThan(snapshotB));
        GlobalTimer.CancelGroup(TEST_GROUP);
    }

    [UnityTest]
    public IEnumerator Reset_ResetsRemainingTime()
    {
        var handle = GlobalTimer.RegisterScaled(1f, () => { }, loop: false);
        yield return new WaitForSeconds(0.5f);
        handle.TryGetTimeRemaining(out float remaining);
        Assert.That(remaining, Is.LessThan(0.6f).And.GreaterThan(0.4f));
        handle.Reset();
        handle.TryGetTimeRemaining(out float newRemaining);
        Assert.That(newRemaining, Is.EqualTo(1f).Within(0.05f));
        handle.Cancel();
    }

    [UnityTest]
    public IEnumerator SetInterval_ChangesInterval()
    {
        int count = 0;
        var handle = GlobalTimer.RegisterScaled(1f, () => count++, loop: true);
        yield return new WaitForSeconds(1.2f);
        Assert.AreEqual(1, count);
        handle.SetInterval(0.2f);
        yield return new WaitForSeconds(0.5f);
        Assert.That(count, Is.GreaterThanOrEqualTo(2));
        handle.Cancel();
    }

    [UnityTest]
    public IEnumerator TryGetProgress_ReturnsValue()
    {
        var handle = GlobalTimer.RegisterScaled(2f, () => { }, loop: false);
        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(handle.TryGetProgress(out float progress));
        Assert.That(progress, Is.EqualTo(0.25f).Within(0.05f));
        handle.Cancel();
    }

    [UnityTest]
    public IEnumerator FrameTimer_RemainingFrames()
    {
        var handle = GlobalTimer.RegisterFrame(5, () => { }, loop: false);
        Assert.IsTrue(handle.TryGetFramesRemaining(out int frames));
        Assert.AreEqual(5, frames);
        yield return null; // 一帧后
        handle.TryGetFramesRemaining(out frames);
        Assert.AreEqual(4, frames);
        handle.Cancel();
    }

    [UnityTest]
    public IEnumerator SetLoop_ChangesLoopBehavior()
    {
        int count = 0;
        var handle = GlobalTimer.RegisterScaled(0.2f, () => count++, loop: false);
        handle.SetLoop(true);
        yield return new WaitForSeconds(0.5f);
        Assert.That(count, Is.GreaterThanOrEqualTo(2));
        handle.Cancel();
    }

    [Test]
    public void Register_NullCallback_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => GlobalTimer.RegisterScaled(1f, null));
        Assert.Throws<ArgumentNullException>(() => GlobalTimer.RegisterMonoUpdate(null));
        Assert.Throws<ArgumentNullException>(() => GlobalTimer.RegisterFrame(1, null));
    }

    [Test]
    public void Cancel_InvalidHandle_DoesNothing()
    {
        var invalidHandle = TimerHandle.Null;
        Assert.DoesNotThrow(() => invalidHandle.Cancel());
        Assert.IsFalse(invalidHandle.IsActive());
    }

    [Test]
    public void TryGetGroupId_ReturnsCorrectGroup()
    {
        var handle = GlobalTimer.RegisterScaled(1f, () => { }, groupID: 42);
        Assert.IsTrue(handle.TryGetGroupId(out int gid));
        Assert.AreEqual(42, gid);
        handle.Cancel();
    }

    [Test]
    public void TryGetInterval_ReturnsInterval()
    {
        var handle = GlobalTimer.RegisterScaled(3.5f, () => { });
        Assert.IsTrue(handle.TryGetInterval(out float interval));
        Assert.AreEqual(3.5f, interval);
        handle.Cancel();
    }

    [Test]
    public void TryGetIsLoop_ReturnsLoopStatus()
    {
        var handle = GlobalTimer.RegisterScaled(1f, () => { }, loop: true);
        Assert.IsTrue(handle.TryGetIsLoop(out bool isLoop));
        Assert.IsTrue(isLoop);
        handle.SetLoop(false);
        Assert.IsTrue(handle.TryGetIsLoop(out isLoop));
        Assert.IsFalse(isLoop);
        handle.Cancel();
    }

    [UnityTest]
    public IEnumerator MonoFixedUpdate_TriggersAtFixedDeltaRate()
    {
        int count = 0;
        var handle = GlobalTimer.RegisterMonoFixedUpdate(() => count++);
        // 等待约 0.2 秒，物理帧通常每 0.02 秒一次，预期约 10 次
        yield return new WaitForSeconds(0.2f);
        handle.Cancel();
        // 允许浮动，约 8~12 次
        Assert.That(count, Is.GreaterThanOrEqualTo(8).And.LessThan(13));
    }

    [UnityTest]
    public IEnumerator MonoFixedUpdate_GroupPause_Works()
    {
        int count = 0;
        int group = 999;
        var handle = GlobalTimer.RegisterMonoFixedUpdate(() => count++, group);
        yield return new WaitForSeconds(0.1f);
        GlobalTimer.PauseGroup(group);
        int snapshot = count;
        yield return new WaitForSeconds(0.1f);
        Assert.AreEqual(snapshot, count);
        GlobalTimer.ResumeGroup(group);
        yield return new WaitForSeconds(0.1f);
        Assert.That(count, Is.GreaterThan(snapshot));
        handle.Cancel();
    }

    [Test]
    public void RegisterMonoFixedUpdate_NullCallback_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => GlobalTimer.RegisterMonoFixedUpdate(null));
    }

    [UnityTest]
    public IEnumerator MonoFixedUpdate_AfterCancel_NotTriggered()
    {
        bool triggered = false;
        var handle = GlobalTimer.RegisterMonoFixedUpdate(() => triggered = true);
        handle.Cancel();
        yield return new WaitForSeconds(0.1f);
        Assert.IsFalse(triggered);
    }

    [TearDown]
    public void TearDown()
    {
        GlobalTimer.CancelGroup(TEST_GROUP);
    }
}