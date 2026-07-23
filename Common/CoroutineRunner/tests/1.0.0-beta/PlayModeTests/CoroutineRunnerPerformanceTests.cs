using System.Collections;
using System.Collections.Generic;
using CoroutineRunner;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

public class CoroutineRunnerPerformanceTests
{
    private const int WarmupCount = 5;
    private const int MeasurementCount = 10;

    [SetUp]
    public void SetUp()
    {

    }

    #region 启动开销

    [Test]
    [Performance]
    public void Run_EmptyCoroutine_StartupTime()
    {
        IEnumerator Empty()
        {
            yield break;
        }

        Measure.Method(() =>
        {
            GlobalCoroutineRunner.Run(Empty());
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [Test]
    [Performance]
    public void RunQueued_WithChannel_StartupTime()
    {
        const string channel = "perfChannel";
        GlobalCoroutineRunner.ConfigureChannel(channel, 1);
        IEnumerator Empty()
        {
            yield break;
        }

        Measure.Method(() =>
        {
            GlobalCoroutineRunner.RunQueued(Empty(), channel);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    #endregion

    #region 协程生命周期控制开销

    [UnityTest]
    [Performance]
    public IEnumerator PauseAndResume_Overhead()
    {
        IEnumerator LongRunning()
        {
            while (true) yield return null;
        }

        var token = GlobalCoroutineRunner.Run(LongRunning());
        yield return null; // 确保启动

        Measure.Method(() =>
        {
            token.Pause();
            token.Resume();
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        token.Cancel();
    }

    [UnityTest]
    [Performance]
    public IEnumerator Cancel_Overhead()
    {
        IEnumerator Short()
        {
            yield return null;
        }

        var token = GlobalCoroutineRunner.Run(Short());
        yield return null;

        Measure.Method(() => token.Cancel())
            .WarmupCount(WarmupCount)
            .MeasurementCount(MeasurementCount)
            .GC()
            .Run();
    }

    #endregion

    #region 自定义 Yield 指令池化性能

    [Test]
    [Performance]
    public void CustomYield_GetAndRelease_GC()
    {
        // 预热池
        var warm = CustomYield.Yield<WaitForSecondsControlled>(0);
        CustomYield.Release(warm);

        Measure.Method(() =>
        {
            var inst = CustomYield.Yield<WaitForSecondsControlled>(1f);
            CustomYield.Release(inst);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [Test]
    [Performance]
    public void NonPooledYield_GC_Baseline()
    {
        Measure.Method(() =>
        {
            var inst = new WaitForSecondsControlled();
            inst.Reset(1f);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [Test]
    [Performance]
    public void CustomYield_NonGenericArg_Boxing()
    {
        // 预热池
        var warm = CustomYield.Yield<WaitForSecondsControlled>(0f);
        CustomYield.Release(warm);

        Measure.Method(() =>
        {
            // 调用 Yield<T>(object arg) → 参数 1f 装箱为 object
            var inst = CustomYield.Yield<WaitForSecondsControlled>(1f);
            CustomYield.Release(inst);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()               // 启用 GC 统计
        .Run();
    }

    [Test]
    [Performance]
    public void CustomYield_GenericArg_NoBoxing()
    {
        var warm = CustomYield.Yield<WaitForSecondsControlled, float>(0f);
        CustomYield.Release(warm);

        Measure.Method(() =>
        {
            // 调用 Yield<T1, T2>(T2 arg) → 参数为 float，无装箱
            var inst = CustomYield.Yield<WaitForSecondsControlled, float>(1f);
            CustomYield.Release(inst);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    // 可选：直接 new 非池化版本作为基线对比
    [Test]
    [Performance]
    public void CustomYield_NewInstance_NonPooled()
    {
        Measure.Method(() =>
        {
            var inst = new WaitForSecondsControlled();
            inst.Reset(1f);
            // 注意：这里没有 Release，因为非池化
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    // 额外测试：使用不同的值类型（如 int）验证装箱同样存在
    [Test]
    [Performance]
    public void CustomYield_NonGenericArg_IntBoxing()
    {
        var warm = CustomYield.Yield<WaitForSecondsControlled>(0);
        CustomYield.Release(warm);

        Measure.Method(() =>
        {
            var inst = CustomYield.Yield<WaitForSecondsControlled>(1); // int -> object 装箱
            CustomYield.Release(inst);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [Test]
    [Performance]
    public void CustomYield_GenericArg_IntNoBoxing()
    {
        var warm = CustomYield.Yield<WaitForSecondsControlled, int>(0);
        CustomYield.Release(warm);

        Measure.Method(() =>
        {
            var inst = CustomYield.Yield<WaitForSecondsControlled, int>(1);
            CustomYield.Release(inst);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    #endregion

    #region 通道排队压力测试（不同数量级）

    [UnityTest]
    [Performance]
    public IEnumerator Channel_QueueThroughput_Pressure([Values(10, 100, 500)] int coroutineCount)
    {
        string channelName = $"pressureChannel_{coroutineCount}";
        GlobalCoroutineRunner.ConfigureChannel(channelName, 5);

        IEnumerator Dummy()
        {
            yield return null;
        }

        var tokens = new CoroutineHandleToken[coroutineCount];

        Measure.Method(() =>
        {
            for (int i = 0; i < coroutineCount; i++)
            {
                tokens[i] = GlobalCoroutineRunner.RunQueued(Dummy(), channelName);
            }
        })
        .WarmupCount(1)
        .MeasurementCount(3)
        .GC()
        .Run();

        // 等待所有完成（使用 IsDone 扩展方法）
        foreach (var t in tokens)
        {
            while (!t.IsDone()) yield return null;
        }
    }

    [UnityTest]
    [Performance]
    public IEnumerator Channel_QueueWithWork_Pressure([Values(20, 100)] int count)
    {
        string channelName = $"workChannel_{count}";
        GlobalCoroutineRunner.ConfigureChannel(channelName, 3);

        IEnumerator Work()
        {
            for (int i = 0; i < 5; i++)
                yield return null;
        }

        var tokens = new CoroutineHandleToken[count];

        Measure.Method(() =>
        {
            for (int i = 0; i < count; i++)
            {
                tokens[i] = GlobalCoroutineRunner.RunQueued(Work(), channelName);
            }
        })
        .WarmupCount(1)
        .MeasurementCount(3)
        .GC()
        .Run();

        foreach (var t in tokens)
        {
            while (!t.IsDone()) yield return null;
        }
    }

    #endregion

    #region 句柄 Token 操作基准

    [Test]
    [Performance]
    public void CoroutineHandleToken_Equality()
    {
        var token1 = new CoroutineHandleToken(1, 100);
        var token2 = new CoroutineHandleToken(1, 100);
        var token3 = new CoroutineHandleToken(2, 100);

        Measure.Method(() =>
        {
            bool eq = token1 == token2;
            bool neq = token1 != token3;
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    #endregion

    #region 多通道并发调度开销

    [UnityTest]
    [Performance]
    public IEnumerator MultipleChannels_ConcurrentScheduling()
    {
        const int channelCount = 10;
        const int corosPerChannel = 20;

        for (int i = 0; i < channelCount; i++)
        {
            GlobalCoroutineRunner.ConfigureChannel($"multi_{i}", 2);
        }

        IEnumerator Empty()
        {
            yield return null;
        }

        var allTokens = new List<CoroutineHandleToken>();

        Measure.Method(() =>
        {
            for (int c = 0; c < channelCount; c++)
            {
                for (int k = 0; k < corosPerChannel; k++)
                {
                    allTokens.Add(GlobalCoroutineRunner.RunQueued(Empty(), $"multi_{c}"));
                }
            }
        })
        .WarmupCount(1)
        .MeasurementCount(3)
        .GC()
        .Run();

        foreach (var t in allTokens)
        {
            while (!t.IsDone()) yield return null;
        }
    }

    #endregion

    #region 帧时间测量

    [UnityTest]
    [Performance]
    public IEnumerator CustomYield_AllocateAndRelease_GC_FrameMeasurement()
    {
        IEnumerator UseYield()
        {
            for (int i = 0; i < 100; i++)
            {
                yield return CustomYield.Yield<WaitForSecondsControlled>(0.01f);
            }
        }

        var token = GlobalCoroutineRunner.Run(UseYield());
        // 测量帧时间，同时等待协程完成
        yield return Measure.Frames()
            .WarmupCount(1)
            .MeasurementCount(10)
            .Run();
        while (!token.IsDone()) yield return null;
    }

    #endregion
}