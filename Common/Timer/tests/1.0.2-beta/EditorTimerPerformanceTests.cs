#if UNITY_EDITOR

using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Timer;
using UnityEngine;

/// <summary>
/// 编辑器计时器性能测试（在非运行模式下执行）
/// </summary>
public class EditorTimerPerformanceTests
{
    private const int WARMUP_COUNT = 3;
    private const int MEASURE_COUNT = 5;

    [SetUp]
    public void SetUp()
    {
        if (Application.isPlaying)
        {
            Assert.Ignore("EditorTimer performance tests must be run in EditMode (non-PlayMode).");
        }
        EditorTimer.CancelAll();
    }

    [TearDown]
    public void TearDown()
    {
        EditorTimer.CancelAll();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Scaled_Editor()
    {
        Measure.Method(() =>
        {
            var handle = EditorTimer.RegisterScaled(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Unscaled_Editor()
    {
        Measure.Method(() =>
        {
            var handle = EditorTimer.RegisterUnscaled(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Frame_Editor()
    {
        Measure.Method(() =>
        {
            var handle = EditorTimer.RegisterFrame(1, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Independent_Editor()
    {
        Measure.Method(() =>
        {
            var handle = EditorTimer.RegisterIndependent(0.1f, () => { }, customScale: 2f, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_WallClock_Editor()
    {
        Measure.Method(() =>
        {
            var handle = EditorTimer.RegisterWallClock(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Manual_Editor()
    {
        Measure.Method(() =>
        {
            var handle = EditorTimer.RegisterManual(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_CancelAll_Editor([Values(100, 500)] int count)
    {
        var handles = new List<EditorTimerHandle>(count);
        for (int i = 0; i < count; i++)
        {
            handles.Add(EditorTimer.RegisterScaled(10f, () => { }, loop: true));
        }
        Measure.Method(() => EditorTimer.CancelAll())
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();
        EditorTimer.CancelAll();
    }

    [Test, Performance]
    public void Measure_GroupCancel_Editor([Values(100, 500)] int count)
    {
        int groupId = 999;
        var handles = new List<EditorTimerHandle>(count);
        for (int i = 0; i < count; i++)
        {
            handles.Add(EditorTimer.RegisterScaled(10f, () => { }, loop: true, groupID: groupId));
        }
        Measure.Method(() => EditorTimer.CancelGroup(groupId))
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();
        EditorTimer.CancelGroup(groupId);
    }

    [Test, Performance]
    public void Measure_GroupPause_Editor([Values(100, 500)] int count)
    {
        int groupId = 888;
        var handles = new List<EditorTimerHandle>(count);
        for (int i = 0; i < count; i++)
        {
            handles.Add(EditorTimer.RegisterScaled(10f, () => { }, loop: true, groupID: groupId));
        }
        Measure.Method(() => EditorTimer.SetGroupPaused(groupId, true))
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();
        EditorTimer.CancelGroup(groupId);
    }

    [Test, Performance]
    public void Measure_QueryAPIs_Editor([Values(10000)] int iterations)
    {
        var handle = EditorTimer.RegisterScaled(60f, () => { }, loop: true);
        Measure.Method(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                handle.TryGetTimeRemaining(out _);
                handle.TryGetProgress(out _);
                handle.TryGetGroupId(out _);
                handle.TryGetInterval(out _);
                handle.TryGetIsLoop(out _);
                handle.TryGetFramesRemainingInt(out _);
            }
        })
        .WarmupCount(1)
        .MeasurementCount(5)
        .GC()
        .Run();
        handle.Cancel();
    }

    [Test, Performance]
    public void Measure_SetInterval_Editor([Values(10000)] int iterations)
    {
        var handle = EditorTimer.RegisterScaled(60f, () => { }, loop: true);
        Measure.Method(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                handle.SetInterval(30f);
            }
        })
        .WarmupCount(1)
        .MeasurementCount(5)
        .GC()
        .Run();
        handle.Cancel();
    }

    [Test, Performance]
    public void Measure_Reset_Editor([Values(10000)] int iterations)
    {
        var handle = EditorTimer.RegisterScaled(60f, () => { }, loop: true);
        Measure.Method(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                handle.Reset();
            }
        })
        .WarmupCount(1)
        .MeasurementCount(5)
        .GC()
        .Run();
        handle.Cancel();
    }

    [Test, Performance]
    public void Measure_TryGetFramesRemaining_FrameDriven_Editor([Values(10000)] int iterations)
    {
        var handle = EditorTimer.RegisterFrame(10, () => { }, loop: true);
        Measure.Method(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                handle.TryGetFramesRemainingInt(out _);
            }
        })
        .WarmupCount(1)
        .MeasurementCount(5)
        .GC()
        .Run();
        handle.Cancel();
    }
}

#endif