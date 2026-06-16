// 文件: Tests/Performance/StressTests.cs
using NUnit.Framework;
using Unity.PerformanceTesting;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EasyLogger.Unity.PerformanceTests
{
    [TestFixture]
    public class StressTests
    {
        [Test]
        [Performance]
        public void Measure_High_Frequency_Logging_Stress()
        {
            const int totalLogs = 10000;
            const int batchSize = 100;

            Measure.Method(() =>
            {
                for (int i = 0; i < totalLogs / batchSize; i++)
                {
                    for (int j = 0; j < batchSize; j++)
                    {
                        LogUtility.Info(TestConfig.TestMessage, i * batchSize + j);
                    }
                    LogUtility.Flush(); // 定期刷新防止队列积压
                }
            })
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();

            LogUtility.Flush();
        }

        [Test]
        [Performance]
        public void Measure_MultiThread_Logging_Stress()
        {
            const int threadCount = 4;
            const int logsPerThread = 2500;

            var tasks = new List<Task>();

            Measure.Method(() =>
            {
                for (int t = 0; t < threadCount; t++)
                {
                    int threadId = t;
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < logsPerThread; i++)
                        {
                            LogUtility.Info($"Thread {threadId} - Log {i}");
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
            })
            .WarmupCount(1)
            .MeasurementCount(3)
            .GC()
            .Run();

            LogUtility.Flush();
            tasks.Clear();
        }

        [Test]
        [Performance]
        public void Measure_Queue_Overflow_Stress()
        {
            const int overflowRatio = 10;
            const int totalLogs = 5000;

            Measure.Method(() =>
            {
                for (int i = 0; i < totalLogs; i++)
                {
                    LogUtility.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(1)
            .MeasurementCount(3)
            .GC()
            .Run();

            LogUtility.Flush();
        }

        [Test]
        [Performance]
        public void Measure_AutoFlush_Stress()
        {
            // 启用自动刷新（每0.1秒）
            LogUtility.EnableAutoFlush(0.1f);

            const int totalLogs = 5000;

            Measure.Method(() =>
            {
                for (int i = 0; i < totalLogs; i++)
                {
                    LogUtility.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(1)
            .MeasurementCount(3)
            .GC()
            .Run();

            // 等待自动刷新
            System.Threading.Thread.Sleep(200);

            LogUtility.DisableAutoFlush();
            LogUtility.Flush();
        }
    }
}