// 文件: Tests/Performance/ConsoleLoggerPerformanceTests.cs
using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace EasyLogger.Unity.PerformanceTests
{
    [TestFixture]
    public class ConsoleLoggerPerformanceTests
    {
        private ILogger consoleLogger;

        [SetUp]
        public void SetUp()
        {
            var config = LoggerConfig.Builder.Create()
                .SetMinLevel(LogLevel.Trace)
                .Build();
            consoleLogger = new ConsoleLogger(config);

            // 临时替换默认 Logger
            LogUtility.Configure(consoleLogger);
        }

        [TearDown]
        public void TearDown()
        {
            // 恢复默认 Logger
            var defaultConfig = LoggerConfig.Builder.Create().Build();
            LogUtility.Configure(new ConsoleLogger(defaultConfig));
        }

        [UnityTest]
        [Performance]
        public IEnumerator Measure_ConsoleLogger_Info_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    consoleLogger.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            yield return null;
        }

        [UnityTest]
        [Performance]
        public IEnumerator Measure_ConsoleLogger_With_Formatter_Performance()
        {
            var config = LoggerConfig.Builder.Create()
                .SetFormatter(new CustomFormatter())
                .SetMinLevel(LogLevel.Trace)
                .Build();
            var logger = new ConsoleLogger(config);

            Measure.Method(() =>
            {
                for (int i = 0; i < TestConfig.Iterations; i++)
                {
                    logger.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            yield return null;
        }

        private class CustomFormatter : ILogFormatter
        {
            public string Format(LogLevel level, string message)
            {
                return $"[Custom][{System.DateTime.Now:HH:mm:ss}] {message}";
            }
        }
    }
}