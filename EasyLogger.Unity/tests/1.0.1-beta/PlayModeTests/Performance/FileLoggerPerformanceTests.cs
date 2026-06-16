// 文件: Tests/Performance/FileLoggerPerformanceTests.cs
using NUnit.Framework;
using Unity.PerformanceTesting;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace EasyLogger.Unity.PerformanceTests
{
    [TestFixture]
    public class FileLoggerPerformanceTests
    {
        private string tempLogDir;
        private ICoroutineProxy coroutineProxy;

        [SetUp]
        public void SetUp()
        {
            tempLogDir = Path.Combine(Application.temporaryCachePath, "PerformanceLogs");
            if (!Directory.Exists(tempLogDir))
                Directory.CreateDirectory(tempLogDir);

            // 创建协程代理
            var go = new GameObject("CoroutineProxy");
            coroutineProxy = go.AddComponent<TestCoroutineProxy>();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempLogDir))
                Directory.Delete(tempLogDir, true);

            if (coroutineProxy != null)
                Object.DestroyImmediate(((MonoBehaviour)coroutineProxy).gameObject);
        }

        [UnityTest]
        [Performance]
        public IEnumerator Measure_FileLogger_Sync_Write_Performance([Values(10, 100, 500)] int messageCount)
        {
            var config = LoggerConfig.Builder.Create()
                .SetMinLevel(LogLevel.Trace)
                .Build();

            var fileConfig = FileLoggerConfig.Builder.Create(config, coroutineProxy)
                .SetLogDirectory(tempLogDir)
                .SetFileNamePrefix("sync_test")
                .SetUseAsync(false)
                .SetAutoFlush(true)
                .Build();

            var fileLogger = new FileLogger(fileConfig);

            Measure.Method(() =>
            {
                for (int i = 0; i < messageCount; i++)
                {
                    fileLogger.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            fileLogger.DisposeOnUnityThread();
            fileLogger.Dispose();
            yield return null;
        }

        [UnityTest]
        [Performance]
        public IEnumerator Measure_FileLogger_Async_Write_Performance([Values(10, 100, 500)] int messageCount)
        {
            var config = LoggerConfig.Builder.Create()
                .SetMinLevel(LogLevel.Trace)
                .Build();

            var fileConfig = FileLoggerConfig.Builder.Create(config, coroutineProxy)
                .SetLogDirectory(tempLogDir)
                .SetFileNamePrefix("async_test")
                .SetUseAsync(true)
                .SetAutoFlush(false)
                .SetFlushIntervalMilliseconds(100)
                .Build();

            var fileLogger = new FileLogger(fileConfig);

            Measure.Method(() =>
            {
                for (int i = 0; i < messageCount; i++)
                {
                    fileLogger.Info(TestConfig.TestMessage, i);
                }
            })
            .WarmupCount(TestConfig.WarmupCount)
            .MeasurementCount(TestConfig.MeasureCount)
            .GC()
            .Run();

            // 等待异步写入完成
            System.Threading.Thread.Sleep(200);
            fileLogger.DisposeOnUnityThread();
            fileLogger.Dispose();
            yield return null;
        }

        [Test]
        [Performance]
        public void Measure_FileLogger_Rotation_Performance()
        {
            var config = LoggerConfig.Builder.Create().Build();
            var fileConfig = FileLoggerConfig.Builder.Create(config, coroutineProxy)
                .SetLogDirectory(tempLogDir)
                .SetFileNamePrefix("rotation_test")
                .SetMaxFileSizeBytes(1024) // 1KB，频繁触发轮转
                .Build();

            var fileLogger = new FileLogger(fileConfig);

            Measure.Method(() =>
            {
                // 写入足够触发多次轮转的数据
                for (int i = 0; i < 500; i++)
                {
                    fileLogger.Info(new string('A', 100) + i);
                }
            })
            .WarmupCount(1)
            .MeasurementCount(3)
            .GC()
            .Run();

            fileLogger.DisposeOnUnityThread();
            fileLogger.Dispose();
        }
    }

    public class TestCoroutineProxy : MonoBehaviour, ICoroutineProxy
    {
        public Coroutine StartCoroutine(System.Collections.IEnumerator enumerator)
        {
            return ((MonoBehaviour)this).StartCoroutine(enumerator);
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            ((MonoBehaviour)this).StopCoroutine(coroutine);
        }
    }
}