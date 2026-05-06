using System;
using System.Collections;
using System.IO;
using EasyLogger.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = EasyLogger.Unity.Debug;

[TestFixture]
public class FileLoggerTests
{
    private string _tempDir;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Application.temporaryCachePath, "FileLoggerTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch (IOException) { /* 忽略清理错误 */ }
    }

    [UnityTest]
    public IEnumerator WriteLog_WritesToFile()
    {
        var config = CreateFileLoggerConfig(useAsync: false, autoFlush: true);
        var logger = new FileLogger(config);
        logger.Info("Test message");
        logger.Flush();
        logger.Dispose();
        yield return null;

        string[] files = Directory.GetFiles(_tempDir, "test_*.log");
        Assert.AreEqual(1, files.Length);
        string content = File.ReadAllText(files[0]);
        StringAssert.Contains("Test message", content);
    }

    [UnityTest]
    public IEnumerator SyncLog_ShouldWriteImmediately()
    {
        var config = CreateFileLoggerConfig(useAsync: false, autoFlush: true);
        var logger = new FileLogger(config);
        logger.Info("Sync write");
        logger.Flush();
        logger.Dispose();
        yield return null;

        string[] files = Directory.GetFiles(_tempDir, "test_*.log");
        Assert.AreEqual(1, files.Length);
        string content = File.ReadAllText(files[0]);
        StringAssert.Contains("Sync write", content);
    }

    [UnityTest]
    public IEnumerator AsyncLog_BufferFlushOnSize()
    {
        var config = CreateFileLoggerConfig(useAsync: true, maxFlushBufferSizeBytes: 50);
        var logger = new FileLogger(config);
        for (int i = 0; i < 5; i++)
            logger.Info(new string('B', 50));
        logger.Flush();
        logger.Dispose();
        yield return null;

        string[] files = Directory.GetFiles(_tempDir, "test_*.log");
        Assert.AreEqual(1, files.Length);
        string content = File.ReadAllText(files[0]);
        StringAssert.Contains("B", content);
    }

    [UnityTest]
    public IEnumerator Rotation_WhenFileExceedsSize_CreatesBackup()
    {
        var config = CreateFileLoggerConfig(maxFileSizeBytes: 100, maxBackupFiles: 2);
        var logger = new FileLogger(config);
        for (int i = 0; i < 20; i++)
            logger.Info(new string('A', 40));
        logger.Flush();
        logger.Dispose();
        yield return new WaitForSeconds(0.1f);

        var allFiles = Directory.GetFiles(_tempDir, "test_*.log");
        Assert.GreaterOrEqual(allFiles.Length, 2);
    }

    [UnityTest]
    public IEnumerator Dispose_ShouldCloseFile()
    {
        var config = CreateFileLoggerConfig(useAsync: false);
        var logger = new FileLogger(config);
        logger.Info("Dispose test");
        // 完整释放 (主线程 + 资源)
        logger.DisposeOnUnityThread();
        logger.Dispose();
        yield return null;

        Assert.Throws<ObjectDisposedException>(() => logger.Info("after"));
    }

    [UnityTest]
    public IEnumerator DisposeOnUnityThread_ShouldStopCoroutine()
    {
        var config = CreateFileLoggerConfig(useAsync: true);
        var logger = new FileLogger(config);
        yield return new WaitForSeconds(0.2f);
        logger.DisposeOnUnityThread();
        logger.Dispose();
        yield return null;
    }

    private FileLoggerConfig CreateFileLoggerConfig(
        bool useAsync = false,
        long maxFileSizeBytes = 10 * 1024 * 1024,
        long maxFlushBufferSizeBytes = 1024,
        int maxBackupFiles = 10,
        bool autoFlush = true)
    {
        var logConfig = LoggerConfig.Builder.Create().Build();
        return FileLoggerConfig.Builder.Create(logConfig)
            .SetLogDirectory(_tempDir)
            .SetFileNamePrefix("test")
            .SetMaxFileSizeBytes(maxFileSizeBytes)
            .SetMaxFlushBufferSizeBytes(maxFlushBufferSizeBytes)
            .SetMaxBackupFiles(maxBackupFiles)
            .SetAutoFlush(autoFlush)
            .SetUseAsync(useAsync)
            .Build();
    }
}

[TestFixture]
public class GameLogTests
{
    [Test]
    public void Configure_ShouldReturnOldLogger()
    {
        // PlayMode 启动后，GameLog 默认 Logger 是 ConsoleLogger，不是 NullLogger
        var old = Debug.Configure(NullLogger.Instance);
        Assert.IsNotNull(old);
        // old 应该是 ConsoleLogger 实例
        Assert.AreNotSame(NullLogger.Instance, old);
        // 恢复：把旧 logger 设回去
        Debug.Configure(old);
    }

    [Test]
    public void Log_WhenNullLogger_ShouldNotThrow()
    {
        var previous = Debug.Configure(NullLogger.Instance);
        try
        {
            Assert.DoesNotThrow(() => Debug.Info("test"));
            Assert.DoesNotThrow(() => Debug.Warning("test"));
            Assert.DoesNotThrow(() => Debug.Error("test"));
            Assert.DoesNotThrow(() => Debug.Fatal("test"));
            Assert.DoesNotThrow(() => Debug.Trace("test"));
        }
        finally
        {
            // 恢复
            if (previous != null) Debug.Configure(previous);
        }
    }

    [UnityTest, Order(999)]  // 最后执行，避免污染其他测试
    public IEnumerator Flush_ShouldClearQueue()
    {
        var logger = new TestableLogger(LoggerConfig.Builder.Create().SetFormatter(null).Build());
        var previous = Debug.Configure(logger);
        Debug.Info("msg1");
        Debug.Info("msg2");
        Debug.Flush();
        yield return null;
        Assert.AreEqual(2, logger.Logs.Count);
        // 恢复
        if (previous != null) Debug.Configure(previous);
    }

    [Test, Order(1000)]  // 最后执行
    public void DisposeOnUnityThread_ShouldNotThrow()
    {
        // 该测试会破坏 Debug 状态，所以放在最后
        Assert.DoesNotThrow(() => Debug.DisposeOnUnityThread());
    }
}