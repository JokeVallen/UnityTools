#if UNITY_EDITOR

using System;
using NUnit.Framework;
using UGUI.Layout.Extension;
using UnityEngine;

/// <summary>
/// <see cref="CurveIndexStepper"/> 单元测试
/// </summary>
public class CurveIndexStepperTests
{
    // ═══════════════════════════════════════════════════════════════════
    // 一、参数验证
    // ═══════════════════════════════════════════════════════════════════

    [Test]
    public void Create_MinGreaterThanMax_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CurveIndexStepper.Create(WrapMode.Default, 5, 3));
    }

    [Test]
    public void Create_StepZero_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CurveIndexStepper.Create(WrapMode.Default, 0, 5, 0));
    }

    [Test]
    public void Next_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        var stepper = CurveIndexStepper.Create(WrapMode.Default, 0, 2);
        Assert.Throws<ArgumentOutOfRangeException>(() => stepper.Next(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => stepper.Next(3));
    }

    // ═══════════════════════════════════════════════════════════════════
    // 二、Clamp / Default / Once 模式（步长 +1）
    // ═══════════════════════════════════════════════════════════════════

    [Test]
    public void Clamp_StepOne_ClampsAtMax()
    {
        var s = CurveIndexStepper.Create(WrapMode.Once, 0, 2);
        Assert.AreEqual(1, s.Next(0));
        Assert.AreEqual(2, s.Next(1));
        Assert.AreEqual(2, s.Next(2)); // 到顶后钉住
    }

    [Test]
    public void Default_StepOne_ClampsAtMax()
    {
        var s = CurveIndexStepper.Create(WrapMode.Default, 0, 2);
        Assert.AreEqual(1, s.Next(0));
        Assert.AreEqual(2, s.Next(1));
        Assert.AreEqual(2, s.Next(2));
    }

    [Test]
    public void ClampForever_StepOne_ClampsAtMax()
    {
        var s = CurveIndexStepper.Create(WrapMode.ClampForever, 0, 2);
        Assert.AreEqual(1, s.Next(0));
        Assert.AreEqual(2, s.Next(1));
        Assert.AreEqual(2, s.Next(2));
    }

    // ═══════════════════════════════════════════════════════════════════
    // 三、Clamp 模式 — 步长变体
    // ═══════════════════════════════════════════════════════════════════

    [Test]
    public void Clamp_StepTwo_SkipsAndClampsAtMax()
    {
        var s = CurveIndexStepper.Create(WrapMode.Default, 0, 4, 2);
        Assert.AreEqual(2, s.Next(0));
        Assert.AreEqual(4, s.Next(2));
        Assert.AreEqual(4, s.Next(4)); // 步长超出后钉住
    }

    [Test]
    public void Clamp_StepNegative_ClampsAtMin()
    {
        var s = CurveIndexStepper.Create(WrapMode.Default, 0, 4, -1);
        Assert.AreEqual(3, s.Next(4));
        Assert.AreEqual(2, s.Next(3));
        Assert.AreEqual(0, s.Next(1));
        Assert.AreEqual(0, s.Next(0)); // 到底后钉住
    }

    [Test]
    public void Clamp_LargeStep_ClampsAtBoundary()
    {
        var s = CurveIndexStepper.Create(WrapMode.Default, 0, 2, 10);
        Assert.AreEqual(2, s.Next(0)); // 0+10 超出，clamp 到 2
    }

    // ═══════════════════════════════════════════════════════════════════
    // 四、Loop 模式 — 步长变体
    // ═══════════════════════════════════════════════════════════════════

    [Test]
    public void Loop_StepOne_WrapsCorrectly()
    {
        var s = CurveIndexStepper.Create(WrapMode.Loop, 0, 2);
        int idx = 0;
        Assert.AreEqual(1, idx = s.Next(idx));
        Assert.AreEqual(2, idx = s.Next(idx));
        Assert.AreEqual(0, idx = s.Next(idx)); // 回绕
        Assert.AreEqual(1, s.Next(idx));
    }

    [Test]
    public void Loop_StepTwo_WrapsCorrectly()
    {
        // range=3 (0,1,2), step=2 → 0→2→1→0→...
        var s = CurveIndexStepper.Create(WrapMode.Loop, 0, 2, 2);
        int idx = 0;
        Assert.AreEqual(2, idx = s.Next(idx));
        Assert.AreEqual(1, idx = s.Next(idx));
        Assert.AreEqual(0, idx = s.Next(idx));
    }

    [Test]
    public void Loop_StepNegative_WrapsBackward()
    {
        // step=-1 → 0→2→1→0→...（反向循环）
        var s = CurveIndexStepper.Create(WrapMode.Loop, 0, 2, -1);
        int idx = 0;
        Assert.AreEqual(2, idx = s.Next(idx));
        Assert.AreEqual(1, idx = s.Next(idx));
        Assert.AreEqual(0, idx = s.Next(idx));
        Assert.AreEqual(2, s.Next(idx));
    }

    [Test]
    public void Loop_StepEqualsRange_StaysSame()
    {
        // step=range → 每次偏移整个周期，位置不变
        var s = CurveIndexStepper.Create(WrapMode.Loop, 0, 2, 3);
        Assert.AreEqual(0, s.Next(0));
        Assert.AreEqual(1, s.Next(1));
        Assert.AreEqual(2, s.Next(2));
    }

    [Test]
    public void Loop_LargeStep_WrapsCorrectly()
    {
        // step=5, range=3 → 等效步长 5%3=2 → 与 StepTwo 相同
        var s = CurveIndexStepper.Create(WrapMode.Loop, 0, 2, 5);
        int idx = 0;
        Assert.AreEqual(2, idx = s.Next(idx));
        Assert.AreEqual(1, idx = s.Next(idx));
        Assert.AreEqual(0, s.Next(idx));
    }

    [Test]
    public void Loop_NegativeRange_StepOne_WrapsCorrectly()
    {
        var s = CurveIndexStepper.Create(WrapMode.Loop, -2, 0);
        int idx = -2;
        Assert.AreEqual(-1, idx = s.Next(idx));
        Assert.AreEqual(0, idx = s.Next(idx));
        Assert.AreEqual(-2, s.Next(idx));
    }

    // ═══════════════════════════════════════════════════════════════════
    // 五、PingPong 模式 — 步长变体
    // ═══════════════════════════════════════════════════════════════════

    [Test]
    public void PingPong_StepOne_SequenceCorrect()
    {
        var s = CurveIndexStepper.Create(WrapMode.PingPong, 0, 2);
        int idx = 0;
        int[] expected = { 1, 2, 1, 0, 1, 2, 1, 0 };
        foreach (int e in expected)
            Assert.AreEqual(e, idx = s.Next(idx));
    }

    [Test]
    public void PingPong_StepTwo_SequenceCorrect()
    {
        // range 3 (0..2), step 2 → 0→2→0→2→...（触顶立即反弹回原点）
        var s = CurveIndexStepper.Create(WrapMode.PingPong, 0, 2, 2);
        int idx = 0;
        Assert.AreEqual(2, idx = s.Next(idx));
        Assert.AreEqual(0, idx = s.Next(idx));
        Assert.AreEqual(2, s.Next(idx));
    }

    [Test]
    public void PingPong_StepNegativeOne_StartsBackward()
    {
        // step=-1 初始方向向后，从 2 开始倒序
        var s = CurveIndexStepper.Create(WrapMode.PingPong, 0, 2, -1);
        int idx = 2;
        int[] expected = { 1, 0, 1, 2, 1, 0 };
        foreach (int e in expected)
            Assert.AreEqual(e, idx = s.Next(idx));
    }

    [Test]
    public void PingPong_LargeStep_FoldsCorrectly()
    {
        // step=5, range=[0,2], period=4
        // 从 0 出发（dir=+1）：absPos = 0 + 1*5 = 5，wrapped = 5%4 = 1，result = 0+1 = 1
        // 从 1 出发（dir=+1）：absPos = 1 + 1*5 = 6，wrapped = 6%4 = 2，result = 0+2 = 2，触顶 dir=-1
        // 从 2 出发（dir=-1）：absPos = 2 + (-1)*5 = -3，wrapped = (-3%4+4)%4 = 1，result = 0+1 = 1
        var s = CurveIndexStepper.Create(WrapMode.PingPong, 0, 2, 5);
        int idx = 0;
        Assert.AreEqual(1, idx = s.Next(idx));
        Assert.AreEqual(2, idx = s.Next(idx));
        Assert.AreEqual(1, s.Next(idx));
    }

    [Test]
    public void PingPong_Length2_StepOne()
    {
        var s = CurveIndexStepper.Create(WrapMode.PingPong, 0, 1);
        int idx = 0;
        Assert.AreEqual(1, idx = s.Next(idx));
        Assert.AreEqual(0, idx = s.Next(idx));
        Assert.AreEqual(1, s.Next(idx));
    }

    // ═══════════════════════════════════════════════════════════════════
    // 六、单元素范围（所有模式、所有步长，索引始终不变）
    // ═══════════════════════════════════════════════════════════════════

    [Test]
    public void SingleElement_AllModes_AllSteps_AlwaysSame()
    {
        WrapMode[] modes = { WrapMode.Once, WrapMode.Clamp, WrapMode.Default, WrapMode.ClampForever, WrapMode.Loop, WrapMode.PingPong };
        int[] steps = { 1, 2, -1, -3, 10 };
        foreach (var mode in modes)
            foreach (int step in steps)
            {
                var s = CurveIndexStepper.Create(mode, 5, 5, step);
                Assert.AreEqual(5, s.Next(5), $"mode={mode} step={step}");
            }
    }

    // ═══════════════════════════════════════════════════════════════════
    // 七、Resolve 静态方法 — 步长变体
    // ═══════════════════════════════════════════════════════════════════

    [Test]
    public void Resolve_StepOne_Loop_WrapsCorrectly()
    {
        // keyCount=3, step=1, Loop
        Assert.AreEqual(0, CurveIndexStepper.Resolve(0, 3, WrapMode.Loop));
        Assert.AreEqual(1, CurveIndexStepper.Resolve(1, 3, WrapMode.Loop));
        Assert.AreEqual(2, CurveIndexStepper.Resolve(2, 3, WrapMode.Loop));
        Assert.AreEqual(0, CurveIndexStepper.Resolve(3, 3, WrapMode.Loop)); // 回绕
        Assert.AreEqual(1, CurveIndexStepper.Resolve(4, 3, WrapMode.Loop));
    }

    [Test]
    public void Resolve_StepTwo_Loop_SkipsCorrectly()
    {
        // effectiveIndex = childIndex*2, keyCount=3
        // 0→0, 1→2, 2→4%3=1, 3→6%3=0
        Assert.AreEqual(0, CurveIndexStepper.Resolve(0, 3, WrapMode.Loop, 2));
        Assert.AreEqual(2, CurveIndexStepper.Resolve(1, 3, WrapMode.Loop, 2));
        Assert.AreEqual(1, CurveIndexStepper.Resolve(2, 3, WrapMode.Loop, 2));
        Assert.AreEqual(0, CurveIndexStepper.Resolve(3, 3, WrapMode.Loop, 2));
    }

    [Test]
    public void Resolve_StepNegative_Clamp_ReversesOrder()
    {
        // step=-1, keyCount=3, Clamp → 从末尾倒序映射
        // effectiveIndex = (keyCount-1) - childIndex = 2,1,0
        Assert.AreEqual(2, CurveIndexStepper.Resolve(0, 3, WrapMode.Default, -1));
        Assert.AreEqual(1, CurveIndexStepper.Resolve(1, 3, WrapMode.Default, -1));
        Assert.AreEqual(0, CurveIndexStepper.Resolve(2, 3, WrapMode.Default, -1));
        Assert.AreEqual(0, CurveIndexStepper.Resolve(3, 3, WrapMode.Default, -1)); // clamp
    }

    [Test]
    public void Resolve_StepOne_PingPong_BouncesCorrectly()
    {
        // keyCount=3, period=4
        // 0→0,1→1,2→2,3→1,4→0,5→1,...
        Assert.AreEqual(0, CurveIndexStepper.Resolve(0, 3, WrapMode.PingPong));
        Assert.AreEqual(1, CurveIndexStepper.Resolve(1, 3, WrapMode.PingPong));
        Assert.AreEqual(2, CurveIndexStepper.Resolve(2, 3, WrapMode.PingPong));
        Assert.AreEqual(1, CurveIndexStepper.Resolve(3, 3, WrapMode.PingPong));
        Assert.AreEqual(0, CurveIndexStepper.Resolve(4, 3, WrapMode.PingPong));
        Assert.AreEqual(1, CurveIndexStepper.Resolve(5, 3, WrapMode.PingPong));
    }

    [Test]
    public void Resolve_StepZero_ReturnsZero()
    {
        // step=0 视为无效，返回 0
        Assert.AreEqual(0, CurveIndexStepper.Resolve(5, 3, WrapMode.Loop, 0));
    }

    [Test]
    public void Resolve_ZeroKeyCount_ReturnsZero()
    {
        Assert.AreEqual(0, CurveIndexStepper.Resolve(3, 0, WrapMode.Loop));
    }
}

#endif