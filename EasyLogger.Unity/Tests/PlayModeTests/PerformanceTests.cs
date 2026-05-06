using System.IO;
using EasyLogger.Unity;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using Debug = EasyLogger.Unity.Debug;

public class PerformanceTests
{
    private string _tempDir;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _tempDir = Path.Combine(Application.temporaryCachePath, "PerfTest_" + System.Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Test, Performance]
    public void GameLog_Enqueue_WithNullLogger()
    {
        Debug.Configure(NullLogger.Instance);
        Measure.Method(() => Debug.Info("benchmark"))
            .WarmupCount(10)
            .MeasurementCount(100)
            .GC()
            .Run();
    }

    [Test, Performance]
    public void ConsoleLogger_Info_FormattedOutput()
    {
        var handler = new TestLogHandler();
        UnityDebugHandler.SetUnityLogHandler(handler);
        var console = new ConsoleLogger(LoggerConfig.Builder.Create().Build());
        Measure.Method(() => console.Info("test"))
            .WarmupCount(5)
            .MeasurementCount(50)
            .GC()
            .Run();
        UnityDebugHandler.SetUnityLogHandler(null); // 恢复
    }

    [Test, Performance]
    public void GameLog_Log_WithNullLogger()
    {
        Debug.Configure(NullLogger.Instance);
        Measure.Method(() =>
        {
            Debug.Info("Test message {0}", 100);
        })
        .WarmupCount(10)
        .MeasurementCount(100)
        .GC()                           // ← 启用 GC 收集
        .Run();
    }

    [Test, Performance]
    public void ConsoleLogger_Info()
    {
        var config = LoggerConfig.Builder.Create().Build();
        var console = new ConsoleLogger(config);
        var originalHandler = UnityEngine.Debug.unityLogger.logHandler;
        UnityEngine.Debug.unityLogger.logHandler = new TestLogHandler();
        Measure.Method(() => console.Info("test"))
            .WarmupCount(5)
            .MeasurementCount(50)
            .GC()                       // ← 启用 GC 收集
            .Run();
        UnityEngine.Debug.unityLogger.logHandler = originalHandler;
    }

    [Test, Performance]
    public void FileLogger_Info_Sync()
    {
        var logConfig = LoggerConfig.Builder.Create().Build();
        var fileConfig = FileLoggerConfig.Builder.Create(logConfig)
            .SetLogDirectory(_tempDir)
            .SetFileNamePrefix("perf")
            .SetUseAsync(false)
            .SetAutoFlush(true)
            .Build();
        using var logger = new FileLogger(fileConfig);
        Measure.Method(() => logger.Info("test message"))
            .WarmupCount(10)
            .MeasurementCount(100)
            .GC()                       // ← 启用 GC 收集
            .Run();
    }

    [Test, Performance]
    public void CompositeLogger_Info_TwoLoggers()
    {
        var config = LoggerConfig.Builder.Create().Build();
        var logger1 = new TestableLogger(config);
        var logger2 = new TestableLogger(config);
        var composite = new CompositeLogger(config, logger1, logger2);
        Measure.Method(() => composite.Info("test"))
            .WarmupCount(10)
            .MeasurementCount(100)
            .GC()                       // ← 启用 GC 收集
            .Run();
    }

    [Test, Performance]
    public void DefaultLogFormatter_Format()
    {
        var formatter = new DefaultLogFormatter();
        Measure.Method(() => formatter.Format(LogLevel.Error, "test message"))
            .WarmupCount(10)
            .MeasurementCount(100)
            .GC()                       // ← 启用 GC 收集
            .Run();
    }
}