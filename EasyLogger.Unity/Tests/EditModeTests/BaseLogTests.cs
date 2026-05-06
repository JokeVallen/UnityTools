using System;
using System.Globalization;
using EasyLogger.Unity;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class LogLevelTests
{
    [Test]
    public void Values_ShouldBeCorrect()
    {
        Assert.AreEqual(1, (int)LogLevel.Trace);
        Assert.AreEqual(2, (int)LogLevel.Info);
        Assert.AreEqual(4, (int)LogLevel.Warning);
        Assert.AreEqual(8, (int)LogLevel.Error);
        Assert.AreEqual(16, (int)LogLevel.Fatal);
    }

    [Test]
    public void MinMax_ShouldPointToExtremes()
    {
        Assert.AreEqual(LogLevel.Trace, LogLevel.Min);
        Assert.AreEqual(LogLevel.Fatal, LogLevel.Max);
    }

    [Test]
    public void Flags_Combination_Works()
    {
        LogLevel combined = LogLevel.Warning | LogLevel.Error;
        Assert.IsTrue((combined & LogLevel.Warning) != LogLevel.None);
        Assert.IsTrue((combined & LogLevel.Error) != LogLevel.None);
        Assert.IsFalse((combined & LogLevel.Info) != LogLevel.None);
    }
}

[TestFixture]
public class LoggerConfigBuilderTests
{
    [Test]
    public void Defaults_ShouldBeCorrect()
    {
        var config = LoggerConfig.Builder.Create().Build();
        Assert.IsTrue(config.ThrowOnError);
        Assert.IsNull(config.OnError);
        Assert.AreEqual((LogLevel)(((long)LogLevel.Max << 1) - 1), config.Levels);
        Assert.IsNotNull(config.Formatter);
        Assert.IsNull(config.FormatProvider);
    }

    [Test]
    public void SetThrowOnError_ShouldUpdate()
    {
        var config = LoggerConfig.Builder.Create().SetThrowOnError(false).Build();
        Assert.IsFalse(config.ThrowOnError);
    }

    [Test]
    public void SetOnError_ShouldUpdate()
    {
        Action<Exception> action = ex => { };
        var config = LoggerConfig.Builder.Create().SetOnError(action).Build();
        Assert.AreSame(action, config.OnError);
    }

    [Test]
    public void SetLevels_ShouldUpdate()
    {
        var config = LoggerConfig.Builder.Create().SetLevels(LogLevel.Error).Build();
        Assert.AreEqual(LogLevel.Error, config.Levels);
    }

    [Test]
    public void SetMinLevel_ShouldSetRange()
    {
        var config = LoggerConfig.Builder.Create().SetMinLevel(LogLevel.Warning).Build();
        Assert.AreEqual(LogLevel.Warning | LogLevel.Error | LogLevel.Fatal, config.Levels);
    }

    [Test]
    public void SetMaxLevel_ShouldSetRange()
    {
        var config = LoggerConfig.Builder.Create().SetMaxLevel(LogLevel.Info).Build();
        Assert.AreEqual(LogLevel.Trace | LogLevel.Info, config.Levels);
    }

    [Test]
    public void SetMinMaxLevel_ShouldSetRange()
    {
        var config = LoggerConfig.Builder.Create().SetMinMaxLevel(LogLevel.Trace, LogLevel.Warning).Build();
        Assert.AreEqual(LogLevel.Trace | LogLevel.Info | LogLevel.Warning, config.Levels);
    }

    [Test]
    public void SetFormatter_ShouldUpdate()
    {
        var formatter = new DefaultLogFormatter();
        var config = LoggerConfig.Builder.Create().SetFormatter(formatter).Build();
        Assert.AreSame(formatter, config.Formatter);
    }

    [Test]
    public void SetFormatProvider_ShouldUpdate()
    {
        var provider = CultureInfo.InvariantCulture;
        var config = LoggerConfig.Builder.Create().SetFormatProvider(provider).Build();
        Assert.AreSame(provider, config.FormatProvider);
    }

    [Test]
    public void Build_ShouldThrowIfReused()
    {
        var builder = LoggerConfig.Builder.Create();
        builder.Build();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }
}

[TestFixture]
public class DefaultLogFormatterTests
{
    [Test]
    public void Format_ShouldContainTimestampLevelAndMessage()
    {
        var formatter = new DefaultLogFormatter();
        string result = formatter.Format(LogLevel.Error, "test");
        StringAssert.Contains("[ERROR]", result);
        StringAssert.Contains("test", result);
        StringAssert.IsMatch(@"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]", result);
    }
}

[TestFixture]
public class LoggerBaseTests
{
    private LoggerConfig _config;

    [SetUp]
    public void SetUp()
    {
        _config = LoggerConfig.Builder.Create()
            .SetFormatter(null)
            .Build();
    }

    [Test]
    public void Log_BelowLevel_ShouldNotReachDoLog()
    {
        var cfg = LoggerConfig.Builder.Create().SetMinLevel(LogLevel.Error).Build();
        var logger = new TestableLogger(cfg);
        logger.Info("msg");
        Assert.IsEmpty(logger.Logs);
    }

    [Test]
    public void Log_AtOrAboveLevel_ShouldCallDoLog()
    {
        var cfg = LoggerConfig.Builder.Create().SetMinLevel(LogLevel.Warning).Build();
        var logger = new TestableLogger(cfg);
        logger.Warning("warn");
        logger.Error("err");
        Assert.AreEqual(2, logger.Logs.Count);
    }

    [Test]
    public void Log_FormatException_ShouldFallback()
    {
        // 必须关闭 ThrowOnError，否则异常会向上抛出，测试中断
        var cfg = LoggerConfig.Builder.Create()
            .SetFormatter(null)
            .SetThrowOnError(false)
            .Build();
        var logger = new TestableLogger(cfg);
        logger.Error("Bad {0", "arg");
        StringAssert.Contains("[Arguments: arg]", logger.Logs[0].Message);
    }

    [Test]
    public void Log_WithArgs_ShouldFormat()
    {
        // 无 Formatter，消息应该就是 "Hello World"
        var logger = new TestableLogger(_config);
        logger.Info("Hello {0}", "World");
        Assert.AreEqual("Hello World", logger.Logs[0].Message);
    }

    [Test]
    public void Log_WithFormatProvider_ShouldUseIt()
    {
        var provider = CultureInfo.GetCultureInfo("fr-FR");
        var cfg = LoggerConfig.Builder.Create()
            .SetFormatter(null)
            .SetFormatProvider(provider)
            .Build();
        var logger = new TestableLogger(cfg);
        logger.Info("{0}", 1.5);
        Assert.AreEqual("1,5", logger.Logs[0].Message);
    }

    [Test]
    public void Log_NullArgs_ShouldNotFormat()
    {
        // 无 Formatter，消息原样
        var logger = new TestableLogger(_config);
        logger.Info("no format", null);
        Assert.AreEqual("no format", logger.Logs[0].Message);
    }

    [Test]
    public void Log_WithFormatter_ShouldCallIt()
    {
        // 使用带 Formatter 的配置，消息应包含 [INFO]
        var cfg = LoggerConfig.Builder.Create()
            .SetFormatter(new DefaultLogFormatter())
            .Build();
        var logger = new TestableLogger(cfg);
        logger.Info("msg");
        StringAssert.Contains("[INFO]", logger.Logs[0].Message);
    }

    [Test]
    public void HandleError_ThrowOnError_ShouldThrow()
    {
        var cfg = LoggerConfig.Builder.Create().SetThrowOnError(true).Build();
        var logger = new TestableLogger(cfg);
        Assert.Throws<InvalidOperationException>(() => logger.HandleError(new InvalidOperationException()));
    }

    [Test]
    public void HandleError_OnError_ShouldCallCallback()
    {
        Exception received = null;
        var cfg = LoggerConfig.Builder.Create().SetThrowOnError(false).SetOnError(ex => received = ex).Build();
        var logger = new TestableLogger(cfg);
        var testEx = new InvalidOperationException("test");
        logger.HandleError(testEx);
        Assert.AreSame(testEx, received);
    }

    [Test]
    public void Trace_Debug_Info_Warning_Error_Fatal_Shortcuts()
    {
        var logger = new TestableLogger(_config);
        logger.Trace("t");
        logger.Info("i");
        logger.Warning("w");
        logger.Error("e");
        logger.Fatal("f");
        Assert.AreEqual(5, logger.Logs.Count);
    }
}

[TestFixture]
public class ConsoleLoggerTests
{
    private ILogHandler _originalUnityHandler;
    private TestLogHandler _testHandler;

    [SetUp]
    public void SetUp()
    {
        _testHandler = new TestLogHandler();
        // 保存当前 GameLogHandler 内部使用的原始 handler
        _originalUnityHandler = UnityDebugHandler.UnityLogHandler;
        // 替换为测试 handler，ConsoleLogger 会使用它
        UnityDebugHandler.SetUnityLogHandler(_testHandler);
    }

    [TearDown]
    public void TearDown()
    {
        // 恢复原始 handler，避免影响其他测试
        UnityDebugHandler.SetUnityLogHandler(_originalUnityHandler);
    }

    [Test]
    public void Info_ShouldLogWithLogTypeLog()
    {
        var console = new ConsoleLogger(LoggerConfig.Builder.Create().Build());
        console.Info("test");
        Assert.AreEqual(1, _testHandler.Logs.Count);
        Assert.AreEqual(LogType.Log, _testHandler.Logs[0].type);
    }

    [Test]
    public void Warning_ShouldUseLogTypeWarning()
    {
        var console = new ConsoleLogger(LoggerConfig.Builder.Create().Build());
        console.Warning("test");
        Assert.AreEqual(LogType.Warning, _testHandler.Logs[0].type);
    }

    [Test]
    public void Error_ShouldUseLogTypeError()
    {
        var console = new ConsoleLogger(LoggerConfig.Builder.Create().Build());
        console.Error("test");
        Assert.AreEqual(LogType.Error, _testHandler.Logs[0].type);
    }

    [Test]
    public void Fatal_ShouldUseLogTypeError()
    {
        var console = new ConsoleLogger(LoggerConfig.Builder.Create().Build());
        console.Fatal("test");
        Assert.AreEqual(LogType.Error, _testHandler.Logs[0].type);
    }

    [Test]
    public void FormattedMessage_ShouldContainTimestamp()
    {
        var console = new ConsoleLogger(LoggerConfig.Builder.Create().Build());
        console.Info("Hello");
        Assert.IsTrue(_testHandler.Logs.Count > 0, "未收到任何日志");
        string msg = _testHandler.Logs[0].message;
        StringAssert.Contains("Hello", msg);
        StringAssert.IsMatch(@"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[INFO\] Hello", msg);
    }

    [Test]
    public void LevelFiltering_ShouldWork()
    {
        var cfg = LoggerConfig.Builder.Create().SetMinLevel(LogLevel.Error).Build();
        var console = new ConsoleLogger(cfg);
        console.Info("should not appear");
        console.Error("should appear");
        Assert.AreEqual(1, _testHandler.Logs.Count);
        Assert.AreEqual(LogType.Error, _testHandler.Logs[0].type);
    }
}

[TestFixture]
public class CompositeLoggerTests
{
    private LoggerConfig _config;

    [SetUp]
    public void SetUp() => _config = LoggerConfig.Builder.Create().Build();

    [Test]
    public void Constructor_WithLoggers_ShouldAddThem()
    {
        var logger1 = new TestableLogger(_config);
        var logger2 = new TestableLogger(_config);
        var composite = new CompositeLogger(_config, logger1, logger2);
        composite.Info("test");
        Assert.AreEqual(1, logger1.Logs.Count);
        Assert.AreEqual(1, logger2.Logs.Count);
    }

    [Test]
    public void Add_Duplicate_ShouldNotAdd()
    {
        var logger = new TestableLogger(_config);
        var composite = new CompositeLogger(_config, logger);
        composite.Add(logger);
        composite.Info("test");
        Assert.AreEqual(1, logger.Logs.Count);
    }

    [Test]
    public void Add_Null_ShouldBeIgnored()
    {
        var composite = new CompositeLogger(_config);
        composite.Add(null);
        composite.Info("test"); // 不应崩溃
    }

    [Test]
    public void Remove_ShouldStopReceivingLogs()
    {
        var logger = new TestableLogger(_config);
        var composite = new CompositeLogger(_config, logger);
        composite.Remove(logger);
        composite.Info("test");
        Assert.AreEqual(0, logger.Logs.Count);
    }

    [Test]
    public void Clear_ShouldRemoveAll()
    {
        var logger = new TestableLogger(_config);
        var composite = new CompositeLogger(_config, logger);
        composite.Clear();
        composite.Info("test");
        Assert.AreEqual(0, logger.Logs.Count);
    }

    [Test]
    public void OneLoggerThrows_OthersStillCalled()
    {
        var tolerantConfig = LoggerConfig.Builder.Create()
            .SetThrowOnError(false)
            .Build();

        var goodLogger = new TestableLogger(tolerantConfig);
        var badLogger = new ThrowingLogger(tolerantConfig);
        var composite = new CompositeLogger(tolerantConfig, badLogger, goodLogger);

        Assert.DoesNotThrow(() => composite.Info("test"));
        Assert.AreEqual(1, goodLogger.Logs.Count);
    }

    private class ThrowingLogger : LoggerBase
    {
        public ThrowingLogger(LoggerConfig config) : base(config) { }
        protected override void DoLog(LogLevel level, string message, params object[] args)
            => throw new Exception("test exception");
    }

    [Test]
    public void Dispose_ShouldCallDisposeOnChildren()
    {
        var disposableLogger = new DisposableLogger(_config);
        var composite = new CompositeLogger(_config, disposableLogger);
        composite.Dispose();
        Assert.IsTrue(disposableLogger.Disposed);
    }

    private class DisposableLogger : LoggerBase, IDisposable
    {
        public bool Disposed;
        public DisposableLogger(LoggerConfig config) : base(config) { }
        protected override void DoLog(LogLevel level, string message, params object[] args) { }
        public void Dispose() => Disposed = true;
    }

    [Test]
    public void DisposeOnUnityThread_ShouldCallOnlyUnityThreadLoggers()
    {
        var unilLogger = new UnityThreadDisposableLogger(_config);
        var normalDisposable = new DisposableLogger(_config);
        var composite = new CompositeLogger(_config, unilLogger, normalDisposable);
        composite.DisposeOnUnityThread();
        Assert.IsTrue(unilLogger.DisposedOnUnity);
        Assert.IsFalse(normalDisposable.Disposed);
    }

    private class UnityThreadDisposableLogger : LoggerBase, IDisposable, IUnityThreadLogger
    {
        public bool DisposedOnUnity;
        public UnityThreadDisposableLogger(LoggerConfig config) : base(config) { }
        protected override void DoLog(LogLevel level, string message, params object[] args) { }
        public void Dispose() { }
        public void DisposeOnUnityThread() => DisposedOnUnity = true;
    }

    [Test]
    public void DisposedCompositeLogger_ShouldThrowOnAdd()
    {
        var composite = new CompositeLogger(_config);
        composite.Dispose();
        Assert.Throws<ObjectDisposedException>(() => composite.Add(new TestableLogger(_config)));
    }

    [Test]
    public void DisposedCompositeLogger_ShouldThrowOnLog()
    {
        var composite = new CompositeLogger(_config);
        composite.Dispose();
        Assert.Throws<ObjectDisposedException>(() => composite.Info("test"));
    }
}