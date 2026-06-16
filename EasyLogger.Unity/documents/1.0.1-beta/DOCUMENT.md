> 内容由 AI 根据核心代码生成，已通过人工审核。

# EasyLogger.Unity API 文档

**版本**：1.0.1-beta  
**命名空间**：`EasyLogger.Unity`  

---

## 一、核心接口

### 1. ILogger

日志记录器核心接口，定义了日志输出的基本契约。

```csharp
public interface ILogger
{
    void Log(LogLevel level, string message, params object[] args);
}
```

所有自定义 Logger 都应实现此接口。推荐继承 `LoggerBase` 以减少重复代码。

---

### 2. ILoggerWithContext

支持日志上下文的记录器接口，继承自 `ILogger`。

```csharp
public interface ILoggerWithContext : ILogger
{
    void Log(LogLevel level, string message, object[] args, LogContext context);
}
```

实现此接口的 Logger 能够接收调用方传入的 `LogContext`，从而获得文件路径、行号、堆栈、自定义数据等额外信息。

---

### 3. ILogFormatter

日志格式化接口，负责将日志消息格式化为最终输出字符串。

```csharp
public interface ILogFormatter
{
    string Format(LogLevel level, string message);
}
```

默认实现为 `DefaultLogFormatter`，输出格式为 `[yyyy-MM-dd HH:mm:ss.fff] [LEVEL] message`。

---

### 4. IUnityThreadLogger

标记接口，表示该 Logger 内部使用了 Unity API，需要在主线程释放资源。

```csharp
public interface IUnityThreadLogger
{
    void DisposeOnUnityThread();
}
```

实现此接口的 Logger 会在 `LogUtility` 退出时先调用 `DisposeOnUnityThread()`，再调用 `IDisposable.Dispose()`。

---

### 5. ICoroutineProxy

协程代理接口，用于在非 MonoBehaviour 环境中启动和停止协程。

```csharp
public interface ICoroutineProxy
{
    Coroutine StartCoroutine(IEnumerator enumerator);
    void StopCoroutine(Coroutine coroutine);
}
```

`FileLogger` 依赖此接口实现异步定时刷新。使用者可传入 `MonoBehaviour` 实例作为代理。

---

## 二、核心类型

### 1. LogLevel

日志级别（位域枚举）。

```csharp
[Flags]
public enum LogLevel
{
    None    = 0,
    Trace   = 1 << 0,
    Info    = 1 << 1,
    Warning = 1 << 2,
    Error   = 1 << 3,
    Fatal   = 1 << 4,
    Min     = Trace,
    Max     = Fatal
}
```

---

### 2. LogContext

日志上下文，用于传递调用位置和自定义数据。

```csharp
public readonly struct LogContext
{
    public string StackTrace { get; }
    public string FilePath { get; }
    public string MemberName { get; }
    public int LineNumber { get; }
    public object UserData { get; }
}
```

**工厂方法**：

| 方法 | 说明 |
| :--- | :--- |
| `Capture()` | 捕获文件路径、成员名、行号 |
| `CaptureWithStackTrace()` | 捕获文件路径、成员名、行号、堆栈 |
| `CaptureWithUserData(object userData)` | 捕获文件路径、成员名、行号、自定义数据 |
| `CaptureWithStackTraceAndUserData(object userData)` | 捕获全部信息 |

所有工厂方法均使用 `[Caller*]` 属性，自动捕获调用点信息。

---

### 3. LoggerConfig

日志记录器配置类。

```csharp
public sealed class LoggerConfig
{
    public LogLevel Levels { get; }
    public ILogFormatter Formatter { get; }
    public IFormatProvider FormatProvider { get; }
}
```

**Builder**：

```csharp
public class Builder
{
    public static Builder Create();
    public Builder SetLevels(LogLevel levels);
    public Builder SetMinLevel(LogLevel minLevel);
    public Builder SetMaxLevel(LogLevel maxLevel);
    public Builder SetMinMaxLevel(LogLevel minLevel, LogLevel maxLevel);
    public Builder SetFormatter(ILogFormatter formatter);
    public Builder SetFormatProvider(IFormatProvider formatProvider);
    public LoggerConfig Build();
}
```

---

### 4. FileLoggerConfig

文件日志记录器配置类。

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
    public ICoroutineProxy CoroutineProxy { get; }
}
```

**Builder**：

```csharp
public class Builder
{
    public static Builder Create(LoggerConfig config, ICoroutineProxy coroutineProxy);
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
```

**默认值**：

| 参数 | 默认值 |
| :--- | :--- |
| `LogDirectory` | `"Logs"` |
| `FileNamePrefix` | `"log"` |
| `MaxFileSizeBytes` | `10 * 1024 * 1024` (10MB) |
| `MaxFlushBufferSizeBytes` | `1024` |
| `MaxBackupFiles` | `10` |
| `AutoFlush` | `true` |
| `UseAsync` | `false` |
| `FlushIntervalMilliseconds` | `5000` |

---

## 三、默认实现

### 1. ConsoleLogger

控制台日志记录器，输出到 Unity 控制台。

```csharp
public sealed class ConsoleLogger : LoggerBase, ILoggerWithContext
{
    public ConsoleLogger(LoggerConfig config);
}
```

- 支持 `ILoggerWithContext`，可接收 `LogContext`
- 有上下文时输出可点击跳转的文件路径和行号
- `UserData` 为 `UnityEngine.Object` 时支持对象高亮

---

### 2. FileLogger

文件日志记录器，支持同步/异步写入、文件轮转、备份清理。

```csharp
public sealed class FileLogger : LoggerBase, IDisposable, IUnityThreadLogger
{
    public FileLogger(FileLoggerConfig config);
    public void Flush();
    public void Dispose();
    public void DisposeOnUnityThread();
}
```

**特性**：
- 支持同步/异步两种写入模式
- 异步模式使用 `StringBuilder` 缓冲，定时或达阈值后刷新
- 文件轮转：按大小自动分割，生成备份文件
- 备份清理：保留最近 N 个备份文件
- 双阶段释放：`DisposeOnUnityThread` + `Dispose`

---

### 3. CompositeLogger

复合日志记录器，将日志同时输出到多个 Logger。

```csharp
public sealed class CompositeLogger : LoggerBase, IDisposable, IUnityThreadLogger
{
    public CompositeLogger(LoggerConfig config, params ILogger[] loggers);
    public void Add(ILogger logger);
    public void Remove(ILogger logger);
    public void Clear();
    public void Dispose();
    public void DisposeOnUnityThread();
}
```

- 自动查重，防止添加自身
- 支持运行时动态添加/移除子 Logger
- 释放时倒序遍历子 Logger，正确处理 `IDisposable` 和 `IUnityThreadLogger`

---

### 4. DefaultLogFormatter

默认日志格式化器。

```csharp
public sealed class DefaultLogFormatter : ILogFormatter
{
    public string Format(LogLevel level, string message);
}
```

输出格式：`[yyyy-MM-dd HH:mm:ss.fff] [LEVEL] message`

---

## 四、静态入口：LogUtility

```csharp
public static class LogUtility
```

### 配置

| 方法 | 说明 |
| :--- | :--- |
| `Configure(ILogger logger)` | 替换当前的日志记录器 |
| `EnableAutoFlush(float interval)` | 启用自动刷新（协程定时调用 `Flush()`） |
| `DisableAutoFlush()` | 禁用自动刷新 |

### 日志输出

| 方法 | 说明 |
| :--- | :--- |
| `Trace(object message, params object[] args)` | 跟踪日志（仅 `TRACE` 宏定义时生效） |
| `Trace(object message, LogContext context, params object[] args)` | 跟踪日志（带上下文） |
| `Info(object message, params object[] args)` | 信息日志 |
| `Info(object message, LogContext context, params object[] args)` | 信息日志（带上下文） |
| `Warning(object message, params object[] args)` | 警告日志 |
| `Warning(object message, LogContext context, params object[] args)` | 警告日志（带上下文） |
| `Error(object message, params object[] args)` | 错误日志 |
| `Error(object message, LogContext context, params object[] args)` | 错误日志（带上下文） |
| `Fatal(object message, params object[] args)` | 严重错误日志 |
| `Fatal(object message, LogContext context, params object[] args)` | 严重错误日志（带上下文） |

### 刷新与队列

| 方法 | 说明 |
| :--- | :--- |
| `Flush()` | 立即将队列中所有日志刷新到 Logger |

---

## 五、使用示例

### 示例 1：基础配置

```csharp
using EasyLogger.Unity;

// 创建配置
var config = LoggerConfig.Builder.Create()
    .SetMinLevel(LogLevel.Info)
    .SetFormatter(new DefaultLogFormatter())
    .Build();

// 使用控制台输出
var logger = new ConsoleLogger(config);
LogUtility.Configure(logger);

// 记录日志
LogUtility.Info("游戏启动完成");
LogUtility.Warning("血量低于 20%");
LogUtility.Error("加载配置失败");
```

---

### 示例 2：文件日志（同步）

```csharp
// 创建协程代理（需要 MonoBehaviour）
public class CoroutineProxy : MonoBehaviour, ICoroutineProxy { }

var proxy = gameObject.AddComponent<CoroutineProxy>();

// 创建日志配置
var config = LoggerConfig.Builder.Create()
    .SetMinLevel(LogLevel.Trace)
    .Build();

// 创建文件配置
var fileConfig = FileLoggerConfig.Builder.Create(config, proxy)
    .SetLogDirectory(Application.persistentDataPath + "/Logs")
    .SetFileNamePrefix("MyGame")
    .SetMaxFileSizeBytes(5 * 1024 * 1024) // 5MB
    .SetMaxBackupFiles(5)
    .SetUseAsync(false)
    .SetAutoFlush(true)
    .Build();

var fileLogger = new FileLogger(fileConfig);
LogUtility.Configure(fileLogger);

LogUtility.Info("日志已写入文件");
```

---

### 示例 3：文件日志（异步）

```csharp
var fileConfig = FileLoggerConfig.Builder.Create(config, proxy)
    .SetLogDirectory(Application.persistentDataPath + "/Logs")
    .SetFileNamePrefix("MyGame")
    .SetUseAsync(true)
    .SetAutoFlush(false)
    .SetFlushIntervalMilliseconds(3000) // 3秒刷新一次
    .Build();

var fileLogger = new FileLogger(fileConfig);
LogUtility.Configure(fileLogger);
LogUtility.EnableAutoFlush(3f); // 配合自动刷新
```

---

### 示例 4：复合日志（控制台 + 文件）

```csharp
var config = LoggerConfig.Builder.Create()
    .SetMinLevel(LogLevel.Trace)
    .Build();

var consoleLogger = new ConsoleLogger(config);
var fileLogger = new FileLogger(fileConfig);

var composite = new CompositeLogger(config, consoleLogger, fileLogger);
LogUtility.Configure(composite);

// 日志同时输出到控制台和文件
LogUtility.Info("这条日志会出现在两个地方");
```

---

### 示例 5：带上下文日志（文件跳转 + 对象高亮）

```csharp
using EasyLogger.Unity;

public class PlayerController : MonoBehaviour
{
    private void TakeDamage(int damage, GameObject attacker)
    {
        // 捕获调用位置 + 传递对象
        var ctx = LogContext.CaptureWithUserData(attacker);
        LogUtility.Info($"受到 {damage} 点伤害", ctx);
        // 控制台输出可点击跳转，且 attacker 可高亮
    }
}
```

---

### 示例 6：堆栈跟踪

```csharp
try
{
    // 可能异常的代码
}
catch (Exception ex)
{
    // 捕获堆栈 + 异常信息
    var ctx = LogContext.CaptureWithStackTraceAndUserData(ex);
    LogUtility.Error("操作失败", ctx);
}
```

---

### 示例 7：手动刷新与自动刷新

```csharp
// 手动刷新（在关键节点调用）
LogUtility.Info("关键操作完成");
LogUtility.Flush(); // 立即写入文件

// 自动刷新（启动协程定时刷新）
LogUtility.EnableAutoFlush(1f); // 每秒刷新一次
// ... 游戏运行 ...
LogUtility.DisableAutoFlush(); // 退出前禁用
LogUtility.Flush(); // 最后一次刷新
```

---

## 六、注意事项

| 注意点 | 说明 |
| :--- | :--- |
| **`Trace` 方法** | 仅在 `TRACE` 宏定义时生效，适合开发调试 |
| **`LogUtility.Configure`** | 替换 Logger 后，旧 Logger 若实现 `IDisposable` 会被释放 |
| **`LogContext.UserData`** | 使用 `object` 类型，可传入任意数据（字符串、对象、Dictionary） |
| **`ConsoleLogger` 上下文** | 传入 `UnityEngine.Object` 时会在控制台支持对象高亮 |
| **异步文件写入** | 需要 `ICoroutineProxy` 支持协程 |
| **退出时刷新** | `LogUtility` 会在编辑器退出/应用退出时自动刷新并释放资源 |
| **线程安全** | `LogUtility.Info` 等方法是线程安全的，可在多线程调用 |

---

## 七、扩展指南

### 自定义 Logger

```csharp
public class MyCustomLogger : LoggerBase
{
    public MyCustomLogger(LoggerConfig config) : base(config) { }

    protected override void DoLog(LogLevel level, string message, params object[] args)
    {
        // 自定义输出逻辑
        string formatted = FormatMessageByFormatProvider(message, args);
        formatted = FormatMessageByFormatter(level, formatted);
        // 输出到你的目标
    }
}
```

### 自定义 Formatter

```csharp
public class MyFormatter : ILogFormatter
{
    public string Format(LogLevel level, string message)
    {
        return $"[{DateTime.Now:HH:mm:ss}] {message}";
    }
}
```

---

**文档版本**：1.0.1-beta  
**最后更新**：2026-06-16