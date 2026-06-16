// 文件: Tests/Performance/LogUtilityEnqueuePerformanceTests.cs
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace EasyLogger.Unity.PerformanceTests
{
    [TestFixture]
    public class LogUtilityEnqueuePerformanceTests
    {
        [SetUp]
        public void SetUp()
        {
            var config = LoggerConfig.Builder.Create().Build();
            LogUtility.Configure(new SilentTestLogger());
        }

        [Test]
        [Performance]
        public void Measure_Info_Enqueue_Performance()
        {
            // 预热
            for (int i = 0; i < TestConfig.WarmupCount; i++)
            {
                LogUtility.Info(TestConfig.TestMessage, i);
            }
            LogUtility.Flush();

            // 测量
            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    LogUtility.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            LogUtility.Flush();
        }

        [Test]
        [Performance]
        public void Measure_Warning_Enqueue_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    LogUtility.Warning(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            LogUtility.Flush();
        }

        [Test]
        [Performance]
        public void Measure_Error_Enqueue_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    LogUtility.Error(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            LogUtility.Flush();
        }

        [Test]
        [Performance]
        public void Measure_Trace_Enqueue_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    LogUtility.Trace(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            LogUtility.Flush();
        }

        [Test]
        [Performance]
        public void Measure_Mixed_Levels_Enqueue_Performance()
        {
            var levels = new[] { LogLevel.Info, LogLevel.Warning, LogLevel.Error };

            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    var level = levels[i % levels.Length];
                    switch (level)
                    {
                        case LogLevel.Info:
                            LogUtility.Info(TestConfig.TestMessage, i);
                            break;
                        case LogLevel.Warning:
                            LogUtility.Warning(TestConfig.TestMessage, i);
                            break;
                        case LogLevel.Error:
                            LogUtility.Error(TestConfig.TestMessage, i);
                            break;
                    }
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            LogUtility.Flush();
        }
    }
}