// 文件: Tests/Performance/LogUtilityFlushPerformanceTests.cs
using NUnit.Framework;
using Unity.PerformanceTesting;
using System.Collections.Generic;

namespace EasyLogger.Unity.PerformanceTests
{
    [TestFixture]
    public class LogUtilityFlushPerformanceTests
    {
        [Test]
        [Performance]
        public void Measure_Flush_Empty_Queue()
        {
            // 确保队列为空
            LogUtility.Flush();

            Measure.Method(() =>
            {
                LogUtility.Flush();
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();
        }

        [Test]
        [Performance]
        public void Measure_Flush_Queue_With_Messages([Values(10, 100, 1000)] int messageCount)
        {
            // 准备消息
            for (int i = 0; i < messageCount; i++)
            {
                LogUtility.Info(TestConfig.TestMessage, i);
            }

            Measure.Method(() =>
            {
                LogUtility.Flush();
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();
        }

        [Test]
        [Performance]
        public void Measure_Batch_Enqueue_And_Flush([Values(10, 100, 1000)] int batchSize)
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < batchSize; i++)
                {
                    LogUtility.Info(TestConfig.TestMessage, i);
                }
                LogUtility.Flush();
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();
        }
    }
}