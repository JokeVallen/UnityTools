using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using Timer;
using UnityEngine.TestTools;
using System.Collections;

public class TimerPerformanceTests
{
    private const int WARMUP_COUNT = 3;
    private const int MEASURE_COUNT = 5;

    [SetUp]
    public void SetUp()
    {
        GlobalTimer.CancelAll();
    }

    [TearDown]
    public void TearDown()
    {
        GlobalTimer.CancelAll();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Scaled()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterScaled(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Unscaled()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterUnscaled(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Frame()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterFrame(1, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    // 压力测试：大量循环计时同时运行
    [UnityTest, Performance]
    public IEnumerator StressTest_ManyLoopingTimers([Values(200, 500, 1000)] int count)
    {
        var handles = new List<TimerHandle>(count);
        try
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    handles.Add(GlobalTimer.RegisterScaled(0.5f, () => { }, loop: true));
                }
            })
            .WarmupCount(1)
            .MeasurementCount(1)
            .GC()
            .Run();

            yield return new WaitForSeconds(2f);

            Measure.Method(() =>
            {
                foreach (var h in handles) h.Cancel();
            })
            .WarmupCount(1)
            .MeasurementCount(1)
            .GC()
            .Run();
        }
        finally
        {
            // 双重保障：强制清除所有计时器
            GlobalTimer.CancelAll();
        }
    }

    // 测量组操作的性能
    [Test, Performance]
    public void Measure_GroupCancel_Performance([Values(100, 500)] int count)
    {
        int groupId = 999;
        var handles = new List<TimerHandle>(count);
        for (int i = 0; i < count; i++)
        {
            handles.Add(GlobalTimer.RegisterScaled(10f, () => { }, loop: true, groupID: groupId));
        }

        Measure.Method(() =>
        {
            GlobalTimer.CancelGroup(groupId);
        })
        .WarmupCount(1)
        .MeasurementCount(5)
        .GC()
        .Run();

        // 确保清理
        GlobalTimer.CancelGroup(groupId);
    }

    [Test, Performance]
    public void Measure_GroupPause_Performance([Values(100, 500)] int count)
    {
        int groupId = 888;
        var handles = new List<TimerHandle>(count);
        for (int i = 0; i < count; i++)
        {
            handles.Add(GlobalTimer.RegisterScaled(10f, () => { }, loop: true, groupID: groupId));
        }

        Measure.Method(() =>
        {
            GlobalTimer.SetGroupPaused(groupId, true);
        })
        .WarmupCount(1)
        .MeasurementCount(5)
        .GC()
        .Run();

        GlobalTimer.CancelGroup(groupId);
    }

    [Test, Performance]
    public void Measure_RegisterCancel_MonoFixedUpdate()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterMonoFixedUpdate(() => { });
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_GroupCancel_MonoFixedUpdate([Values(100, 500)] int count)
    {
        int groupId = 777;
        var handles = new List<TimerHandle>(count);
        for (int i = 0; i < count; i++)
        {
            handles.Add(GlobalTimer.RegisterMonoFixedUpdate(() => { }, groupID: groupId));
        }
        Measure.Method(() => GlobalTimer.CancelGroup(groupId))
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();
        GlobalTimer.CancelGroup(groupId);
    }

    [Test, Performance]
    public void Measure_GroupPause_MonoFixedUpdate([Values(100, 500)] int count)
    {
        int groupId = 666;
        var handles = new List<TimerHandle>(count);
        for (int i = 0; i < count; i++)
        {
            handles.Add(GlobalTimer.RegisterMonoFixedUpdate(() => { }, groupID: groupId));
        }
        Measure.Method(() => GlobalTimer.SetGroupPaused(groupId, true))
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();
        GlobalTimer.CancelGroup(groupId);
    }

    // ==================== 补充：不同时间源注册/取消 ====================

    [Test, Performance]
    public void Measure_RegisterCancel_MonoUpdate()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterMonoUpdate(() => { });
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_MonoLateUpdate()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterMonoLateUpdate(() => { });
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_CoroutineUpdate()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterCoroutineUpdate(() => { });
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_CoroutineEndOfFrame()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterCoroutineEndOfFrame(1, () => { });
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    // ==================== 补充：查询 API 性能 ====================

    [Test, Performance]
    public void Measure_QueryAPIs_Overhead([Values(10000)] int iterations)
    {
        var handle = GlobalTimer.RegisterScaled(60f, () => { }, loop: true);
        Measure.Method(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                handle.TryGetTimeRemaining(out _);
                handle.TryGetProgress(out _);
                handle.TryGetGroupId(out _);
                handle.TryGetInterval(out _);
                handle.TryGetIsLoop(out _);
                handle.TryGetFramesRemainingInt(out _); // 对非帧驱动返回 false，但开销很小
            }
        })
        .WarmupCount(1)
        .MeasurementCount(5)
        .GC()
        .Run();
        handle.Cancel();
    }

    // 单独测试帧驱动任务的 TryGetFramesRemaining 性能
    [Test, Performance]
    public void Measure_TryGetFramesRemaining_FrameDriven([Values(10000)] int iterations)
    {
        var handle = GlobalTimer.RegisterFrame(10, () => { }, loop: true);
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

    // ==================== 补充：修改 API 性能 ====================

    [Test, Performance]
    public void Measure_SetInterval_Overhead([Values(10000)] int iterations)
    {
        var handle = GlobalTimer.RegisterScaled(60f, () => { }, loop: true);
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
    public void Measure_Reset_Overhead([Values(10000)] int iterations)
    {
        var handle = GlobalTimer.RegisterScaled(60f, () => { }, loop: true);
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
    public void Measure_SetLoop_Overhead([Values(10000)] int iterations)
    {
        var handle = GlobalTimer.RegisterScaled(60f, () => { }, loop: true);
        Measure.Method(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                handle.SetLoop(i % 2 == 0);
            }
        })
        .WarmupCount(1)
        .MeasurementCount(5)
        .GC()
        .Run();
        handle.Cancel();
    }

    // ==================== 补充：容量边界测试 ====================

    [Test, Performance]
    public void Measure_CapacityBoundary_RegistrationBeyondLimit()
    {
        const int CAPACITY = 2048;
        var handles = new List<TimerHandle>(CAPACITY + 100);
        try
        {
            // 先填满容量
            for (int i = 0; i < CAPACITY; i++)
            {
                var h = GlobalTimer.RegisterScaled(60f, () => { }, loop: true);
                Assert.IsFalse(h.IsNull, $"Failed to register at index {i}");
                handles.Add(h);
            }

            // 测量超额注册的耗时和返回值
            TimerHandle overflowHandle = TimerHandle.Null;
            Measure.Method(() =>
            {
                overflowHandle = GlobalTimer.RegisterScaled(1f, () => { }, loop: false);
            })
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();

            Assert.IsTrue(overflowHandle.IsNull, "Should return null when capacity exceeded");
        }
        finally
        {
            foreach (var h in handles) h.Cancel();
            GlobalTimer.CancelAll();
        }
    }

    // 测量释放一半后再注册的性能（验证空闲链表复用效率）
    [Test, Performance]
    public void Measure_FreeSlotReuse_Performance()
    {
        const int COUNT = 1000;
        var handles = new List<TimerHandle>(COUNT);
        try
        {
            // 注册 COUNT 个计时器
            for (int i = 0; i < COUNT; i++)
            {
                handles.Add(GlobalTimer.RegisterScaled(60f, () => { }, loop: true));
            }
            // 取消一半（奇数索引）
            for (int i = 1; i < COUNT; i += 2)
            {
                handles[i].Cancel();
            }
            // 测量重新注册的性能（应该复用空闲槽位）
            Measure.Method(() =>
            {
                for (int i = 0; i < 500; i++)
                {
                    var h = GlobalTimer.RegisterScaled(30f, () => { }, loop: true);
                    h.Cancel(); // 立即取消，避免累积
                }
            })
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();
        }
        finally
        {
            foreach (var h in handles) h.Cancel();
            GlobalTimer.CancelAll();
        }
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Independent()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterIndependent(0.1f, () => { }, customScale: 2f, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_WallClock()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterWallClock(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_Manual()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterManual(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_MonoFixedUnscaled()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterMonoFixedUnscaled(0.1f, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_RegisterCancel_CoroutineWaitForFixedUpdate()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.RegisterCoroutineWaitForFixedUpdate(1, () => { }, loop: false);
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void Measure_CancelAll_WithTimers([Values(100, 500)] int count)
    {
        var handles = new List<TimerHandle>(count);
        for (int i = 0; i < count; i++)
        {
            handles.Add(GlobalTimer.RegisterScaled(10f, () => { }, loop: true));
        }
        Measure.Method(() => GlobalTimer.CancelAll())
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();
        // 确保清理
        GlobalTimer.CancelAll();
    }

    [Test, Performance]
    public void Measure_RegisterCustomCombination()
    {
        Measure.Method(() =>
        {
            var handle = GlobalTimer.Register(
                interval: 0.1f,
                callback: () => { },
                delta: TimeDelta.Unscaled,
                schedule: TimeSchedule.LateUpdate,
                loop: false
            );
            handle.Cancel();
        })
        .WarmupCount(WARMUP_COUNT)
        .MeasurementCount(MEASURE_COUNT)
        .GC()
        .Run();
    }

    // 大容量注册后 CancelAll 的性能
    [Test, Performance]
    public void Measure_CancelAll_AfterMassRegistration([Values(1000)] int count)
    {
        var handles = new List<TimerHandle>(count);
        try
        {
            for (int i = 0; i < count; i++)
            {
                handles.Add(GlobalTimer.RegisterScaled(60f, () => { }, loop: true));
            }
            Measure.Method(() => GlobalTimer.CancelAll())
                .WarmupCount(1)
                .MeasurementCount(3)
                .GC()
                .Run();
        }
        finally
        {
            GlobalTimer.CancelAll();
        }
    }
}