> 内容由 AI 根据核心代码生成，已通过人工审核。

# EasyLogger.Unity API 文档

本页详细列出了 `EasyLogger.Unity` 库中所有公开的 API，包括接口、枚举、类和它们的成员签名及说明。  
使用示例见文档末尾。

---

## 接口

### ILogger
所有日志记录器必须实现的核心接口。

```csharp
public interface ILogger
{
    void Log(LogLevel level, string message, params object[] args);
    void Trace(string message, params object[] args);
    void Info(string message, params object[] args);
    void Warning(string message, params object[] args);
    void Error(string message, params object[] args);
    void Fatal(string message, params object[] args);
}
```

| 成员 | 说明 |
|------|------|
| `Log(LogLevel, string, params object[])` | 以给定的日志级别和格式化参数记录一条日志。 |
| `Trace(string, params object[])` | 记录 `LogLevel.Trace` 级别的日志，通常用于跟踪程序执行路径。 |
| `Info(string, params object[])` | 记录 `LogLevel.Info` 级别的普通信息日志。 |
| `Warning(string, params object[])` | 记录 `LogLevel.Warning` 级别的警告日志。 |
| `Error(string, params object[])` | 记录 `LogLevel.Error` 级别的错误日志。 |
| `Fatal(string, params object[])` | 记录 `LogLevel.Fatal` 级别的致命错误日志。 |

---

### IUnityThreadLogger
标识一个日志记录器需要在 Unity 主线程上释放资源（例如停止协程）。

```csharp
public interface IUnityThreadLogger
{
    void DisposeOnUnityThread();
}
```

| 成员 | 说明 |
|------|------|
| `DisposeOnUnityThread()` | 在主线程上执行与 Unity API 相关的清理工作。 |

---

### ILogFormatter
用于格式化单条日志信息的接口。

```csharp
public interface ILogFormatter
{
    string Format(LogLevel level, string message);
}
```

| 成员 | 说明 |
|------|------|
| `Format(LogLevel, string)` | 返回格式化后的完整日志行，通常包含时间戳、级别和消息。 |

---

## 枚举

### LogLevel
`[Flags]` 位域枚举，表示日志级别。

```csharp
[Flags]
public enum LogLevel
{
    None = 0,
    Min = Trace,
    Max = Fatal,
    Trace = 1 << 0,
    Info = 1 << 1,
    Warning = 1 << 2,
    Error = 1 << 3,
    Fatal = 1 << 4
}
```

| 值 | 说明 |
|----|------|
| `None` | 空级别，用于位运算判断。 |
| `Min` | 日志级别最小值，等于 `Trace`。 |
| `Max` | 日志级别最大值，等于 `Fatal`。 |
| `Trace` | 跟踪级别，最详细的信息。 |
| `Info` | 普通信息级别。 |
| `Warning` | 警告级别。 |
| `Error` | 错误级别。 |
| `Fatal` | 致命错误级别。 |

位域特性允许通过 `｜` 组合多个级别，或者通过位运算进行范围过滤。

---

## 静态门面：Debug

静态入口类，封装了队列、默认记录器和释放逻辑。

```csharp
public static class Debug
{
    public static void HelloWorld();
    public static ILogger Configure(ILogger logger);
    public static void Flush();
    public static void Info(string message, params object[] args);
    public static void Warning(string message, params object[] args);
    public static void Error(string message, params object[] args);
    public static void Fatal(string message, params object[] args);
    [Conditional("TRACE")]
    public static void Trace(string message, params object[] args);
    public static void DisposeOnUnityThread();
}
```

| 成员 | 说明 |
|------|------|
| `HelloWorld()` | 显式触发静态构造函数，预热日志系统并接管 `UnityEngine.Debug`。 |
| `Configure(ILogger)` | 设置自定义日志记录器，返回旧的记录器，可对旧记录器执行释放操作。 |
| `Flush()` | 立即将队列中的所有日志分派给当前记录器。 |
| `Info(string, params object[])` | 记录 `Info` 级别日志。 |
| `Warning(string, params object[])` | 记录 `Warning` 级别日志。 |
| `Error(string, params object[])` | 记录 `Error` 级别日志。 |
| `Fatal(string, params object[])` | 记录 `Fatal` 级别日志。 |
| `Trace(string, params object[])` | 仅在 `TRACE` 编译符号定义时调用，记录 `Trace` 级别日志。 |
| `DisposeOnUnityThread()` | 释放所有实现了 `IUnityThreadLogger` 的记录器（需在主线程调用）。 |

---

## LoggerBase
抽象基类，提供统一的级别过滤、格式化支持和异常处理。

```csharp
public abstract class LoggerBase : ILogger
{
    protected LoggerConfig Config { get; }
    protected LoggerBase(LoggerConfig config);
    public void Log(LogLevel level, string message, params object[] args);
    public void Trace(string message, params object[] args);
    public void Info(string message, params object[] args);
    public void Warning(string message, params object[] args);
    public void Error(string message, params object[] args);
    public void Fatal(string message, params object[] args);
    protected abstract void DoLog(LogLevel level, string message, params object[] args);
    protected string FormatMessageByFormatProvider(string message, object[] args);
    protected string FormatMessageByFormatter(LogLevel level, string message);
    protected void HandleError(Exception ex);
}
```

| 成员 | 说明 |
|------|------|
| `Config` | 获取当前的 `LoggerConfig`。 |
| `Log`, `Trace`, `Info`, `Warning`, `Error`, `Fatal` | 实现 `ILogger` 接口，其中 `Log` 会先检查 `Levels` 位域，再调用 `DoLog`。 |
| `DoLog(LogLevel, string, params object[])` | 抽象方法，子类必须实现，用于输出日志。接收原始 `message` 和 `args`，子类可自由调用格式化方法。 |
| `FormatMessageByFormatProvider(string, object[])` | 使用 `IFormatProvider` 或 `string.Format` 将参数插入消息；异常时回退为 `[Arguments: ...]` 格式。 |
| `FormatMessageByFormatter(LogLevel, string)` | 如果配置了 `ILogFormatter`，则调用其 `Format` 方法；异常时返回原消息。 |
| `HandleError(Exception)` | 根据 `ThrowOnError` 和 `OnError` 配置处理异常：回调或抛出。 |

---

## LoggerConfig
日志记录器的通用配置，通过内部 `Builder` 创建。

```csharp
public sealed class LoggerConfig
{
    public bool ThrowOnError { get; }
    public Action<Exception> OnError { get; }
    public LogLevel Levels { get; }
    public ILogFormatter Formatter { get; }
    public IFormatProvider FormatProvider { get; }

    public class Builder
    {
        public static Builder Create();
        public Builder SetThrowOnError(bool throwOnError);
        public Builder SetOnError(Action<Exception> onError);
        public Builder SetLevels(LogLevel levels);
        public Builder SetMinLevel(LogLevel minLevel);
        public Builder SetMaxLevel(LogLevel maxLevel);
        public Builder SetMinMaxLevel(LogLevel minLevel, LogLevel maxLevel);
        public Builder SetFormatter(ILogFormatter formatter);
        public Builder SetFormatProvider(IFormatProvider formatProvider);
        public LoggerConfig Build();
    }
}
```

| 属性 | 说明 |
|------|------|
| `ThrowOnError` | 日志系统自身发生异常时是否抛出；默认为 `true`。 |
| `OnError` | 异常回调，无论 `ThrowOnError` 为何值都会触发。 |
| `Levels` | 允许的日志级别位域；默认全级别。 |
| `Formatter` | 日志格式化器，默认为 `DefaultLogFormatter`。 |
| `FormatProvider` | 用于 `string.Format` 的 `IFormatProvider`，默认为 `null`。 |

Builder 的 `SetMinLevel`, `SetMaxLevel`, `SetMinMaxLevel` 会自动计算位域值；`Build()` 返回配置实例，不能重复构建。

---

## FileLoggerConfig
文件日志记录器的专用配置，包含文件存储相关参数。

```csharp
public sealed class FileLoggerConfig
{
    public string LogDirectory { get; }
    public string FileNamePrefix { get; }
    public long MaxFileSizeBytes { get; }
    public long MaxFlushBufferSizeBytes { get; }
    public int MaxBackupFiles { get; }
    public bool AutoFlush { get; }
    public bool UseAsync { get; }
    public int FlushIntervalMilliseconds { get; }
    public LoggerConfig Config { get; }

    public class Builder
    {
        public static Builder Create(LoggerConfig config);
        public Builder SetLogDirectory(string logDirectory);
        public Builder SetFileNamePrefix(string fileNamePrefix);
        public Builder SetMaxFileSizeBytes(long maxFileSizeBytes);
        public Builder SetMaxFlushBufferSizeBytes(long maxFlushBufferSizeBytes);
        public Builder SetMaxBackupFiles(int maxBackupFiles);
        public Builder SetAutoFlush(bool autoFlush);
        public Builder SetUseAsync(bool useAsync);
        public Builder SetFlushIntervalMilliseconds(int flushIntervalMilliseconds);
        public FileLoggerConfig Build();
    }
}
```

| 属性 | 说明 |
|------|------|
| `LogDirectory` | 日志文件存放目录，默认为 `"Logs"`。 |
| `FileNamePrefix` | 日志文件名前缀，默认为 `"log"`。 |
| `MaxFileSizeBytes` | 单个日志文件最大字节数，超过后触发轮转，默认 10 MB。 |
| `MaxFlushBufferSizeBytes` | 异步模式下缓冲区大小阈值，达到后自动刷新，默认 1024 字节。 |
| `MaxBackupFiles` | 保留的最大备份文件数，超出后删除最旧的，默认 10。 |
| `AutoFlush` | 同步模式下是否每次写入后自动刷新，默认为 `true`。 |
| `UseAsync` | 是否启用异步写入，默认为 `false`。 |
| `FlushIntervalMilliseconds` | 异步模式下定时刷新的时间间隔，默认为 5000 ms。 |
| `Config` | 继承的 `LoggerConfig` 通用配置。 |

Builder 的 `SetMaxFileSizeBytes` 和 `SetMaxFlushBufferSizeBytes` 会自动限制参数最小值，`Build()` 不可重复调用。

---

## 记录器实现类

### ConsoleLogger
将日志输出到 Unity 编辑器控制台。

```csharp
public sealed class ConsoleLogger : LoggerBase, IUnityThreadLogger
{
    public ConsoleLogger(LoggerConfig config);
    public void DisposeOnUnityThread();
    // protected override void DoLog(...)
}
```

| 成员 | 说明 |
|------|------|
| 构造函数 | 接受 `LoggerConfig`。 |
| `DisposeOnUnityThread()` | 空实现，因为控制台输出无需主线程释放。 |

### FileLogger
将日志写入文件，支持轮转、备份和异步写入。

```csharp
public sealed class FileLogger : LoggerBase, IDisposable, IUnityThreadLogger
{
    public FileLogger(FileLoggerConfig config);
    public void Flush();
    public void Dispose();
    public void DisposeOnUnityThread();
    // protected override void DoLog(...)
}
```

| 成员 | 说明 |
|------|------|
| `FileLogger(FileLoggerConfig)` | 构造函数，传入文件配置。 |
| `Flush()` | 刷新缓冲区并强制写入文件。 |
| `Dispose()` | 释放文件流等非托管资源。 |
| `DisposeOnUnityThread()` | 停止协程等 Unity 主线程相关资源。 |

### CompositeLogger
组合多个 `ILogger`，实现一份日志多路输出。

```csharp
public sealed class CompositeLogger : LoggerBase, IDisposable, IUnityThreadLogger
{
    public CompositeLogger(LoggerConfig config, params ILogger[] loggers);
    public void Add(ILogger logger);
    public void Remove(ILogger logger);
    public void Clear();
    public void Dispose();
    public void DisposeOnUnityThread();
    // protected override void DoLog(...)
}
```

| 成员 | 说明 |
|------|------|
| `CompositeLogger(LoggerConfig, params ILogger[])` | 构造函数，接收配置和可选的初始记录器列表。 |
| `Add(ILogger)` | 添加一个记录器，自动去重。 |
| `Remove(ILogger)` | 移除一个记录器。 |
| `Clear()` | 清空所有记录器。 |
| `Dispose()` | 释放所有实现了 `IDisposable` 的子记录器（非 Unity 线程依赖）。 |
| `DisposeOnUnityThread()` | 释放所有实现了 `IUnityThreadLogger` 的子记录器（在主线程调用）。 |

---

## DefaultLogFormatter
默认格式化器，输出格式为 `[yyyy-MM-dd HH:mm:ss.fff] [LEVEL] message`。

```csharp
public sealed class DefaultLogFormatter : ILogFormatter
{
    public string Format(LogLevel level, string message);
}
```

---

## 使用示例

### 最简单的控制台输出
```csharp
Debug.HelloWorld();
Debug.Info("游戏启动");
Debug.Warning("配置文件未找到，使用默认值");
Debug.Error("数据库连接失败");
```

### 配置文件日志并异步写入
```csharp
var fileConfig = FileLoggerConfig.Builder.Create(
        LoggerConfig.Builder.Create().SetMinLevel(LogLevel.Warning).Build())
    .SetLogDirectory(Application.persistentDataPath + "/Logs")
    .SetFileNamePrefix("MyGame")
    .SetUseAsync(true)
    .Build();

var logger = new FileLogger(fileConfig);
Debug.Configure(logger);
```

### 组合控制台与文件
```csharp
var console = new ConsoleLogger(LoggerConfig.Builder.Create().Build());
var file = new FileLogger(fileConfig);
var composite = new CompositeLogger(LoggerConfig.Builder.Create().Build(), console, file);
Debug.Configure(composite);
```

### 自定义格式化器
```csharp
class MyFormatter : ILogFormatter
{
    public string Format(LogLevel level, string message)
        => $"[Frame:{Time.frameCount}] [{level.ToString().ToUpper()}] {message}";
}

var config = LoggerConfig.Builder.Create().SetFormatter(new MyFormatter()).Build();
Debug.Configure(new ConsoleLogger(config));
```

### 接管 Unity Debug 日志流
库已自动替换 `Debug.unityLogger.logHandler`，所有 `UnityEngine.Debug.Log` 调用都会经由 `EasyLogger.Unity.Debug` 管线。若需尽早生效，可在游戏启动时调用 `Debug.HelloWorld()`。

### 退出时正确释放资源
```csharp
private void OnDestroy()
{
    Debug.DisposeOnUnityThread();   // 主线程释放
    // 进程退出时会自动调用 Dispose() 关闭文件等
}
```