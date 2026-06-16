// 文件: Tests/Performance/CompositeLoggerPerformanceTests.cs
using System;
using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace EasyLogger.Unity.PerformanceTests
{
    [TestFixture]
    public class CompositeLoggerPerformanceTests
    {
        [Test]
        [Performance]
        public void Measure_CompositeLogger_Two_Loggers_Performance()
        {
            var config = LoggerConfig.Builder.Create()
                .SetMinLevel(LogLevel.Trace)
                .Build();

            var consoleLogger = new ConsoleLogger(config);
            var compositeLogger = new CompositeLogger(config, consoleLogger);

            // 添加第二个 Logger（内存 Logger 模拟）
            compositeLogger.Add(new MemoryLogger(config));

            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    compositeLogger.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            compositeLogger.DisposeOnUnityThread();
            compositeLogger.Dispose();
        }

        [UnityTest]
        [Performance]
        public IEnumerator Measure_CompositeLogger_Many_Loggers_Performance([Values(1, 3, 5)] int loggerCount)
        {
            var config = LoggerConfig.Builder.Create()
                .SetMinLevel(LogLevel.Trace)
                .Build();

            var compositeLogger = new CompositeLogger(config);
            for (int i = 0; i < loggerCount; i++)
            {
                compositeLogger.Add(new MemoryLogger(config));
            }

            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    compositeLogger.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            compositeLogger.DisposeOnUnityThread();
            compositeLogger.Dispose();
            yield return null;
        }

        [UnityTest]
        [Performance]
        public IEnumerator Measure_CompositeLogger_Dynamic_Add_Remove_Performance()
        {
            var config = LoggerConfig.Builder.Create()
                .SetMinLevel(LogLevel.Trace)
                .Build();

            var compositeLogger = new CompositeLogger(config);

            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var logger = new MemoryLogger(config);
                    compositeLogger.Add(logger);
                    compositeLogger.Remove(logger);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            compositeLogger.DisposeOnUnityThread();
            compositeLogger.Dispose();
            yield return null;
        }
    }

    // 内存日志记录器，用于性能测试（不涉及 IO）
    public class MemoryLogger : LoggerBase, IDisposable
    {
        private System.Collections.Generic.List<string> logs = new System.Collections.Generic.List<string>();

        public MemoryLogger(LoggerConfig config) : base(config) { }

        protected override void DoLog(LogLevel level, string message, params object[] args)
        {
            logs.Add($"{level}:{message}");
        }

        public void Dispose()
        {
            logs.Clear();
        }
    }
}