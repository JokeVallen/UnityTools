# EasyLogger.Unity

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3+-black?logo=unity)](https://unity.com/)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)

一个即拿即用、轻量低耗的 Unity 日志工具库。它统一了控制台输出与文件持久化，提供灵活的级别过滤、格式化扩展以及**日志上下文**能力，同时几乎不产生运行时开销。

---

## 简介

EasyLogger.Unity 为 Unity 项目提供了开箱即用的日志解决方案。它接管 `UnityEngine.Debug` 日志流，自动将所有 `Debug.Log` 调用收归统一管线，支持同时输出到控制台和文件；内置文件轮转、备份清理、异步缓冲等实用功能。

开发者可通过简单的配置定制日志级别、格式、输出目标。通过 `LogContext` 和 `ILoggerWithContext`，可以携带调用位置、堆栈、自定义数据，甚至 Unity 对象引用，获得与原生 `Debug.Log` 一致的调试体验（点击跳转、对象高亮）。

测试覆盖全部公共 API，功能稳定；性能基准显示每次日志调用仅微秒级耗时且内存分配极小，即使在高频场景下也不会引起 GC 卡顿。

---

## 安装环境要求

- Unity 2020.3 或更高版本
- 脚本后端：Mono 或 IL2CPP
- 目标平台：所有 Unity 支持的平台

---

## 安装方式

### 方式一：源码导入

1. 将 `EasyLogger.Unity` 目录拷贝至你的项目 `Assets/` 下。
2. 在你的游戏入口脚本中调用 `LogUtility.Info("初始化完成")` 即可完成预热（静态构造函数会自动初始化）。

### 方式二：DLL 导入

1. 将 `EasyLogger.Unity.dll` 放入项目的 `Assets/Plugins/` 目录。
2. 确保 `.dll` 的编译目标与你的项目一致（通常为 .NET Standard 2.0）。
3. 使用 `LogUtility.Info(...)` 即可开始记录日志。

---

## 设计理念

- **即拿即用**：简单配置即可获得控制台 + 文件的完整日志能力。
- **不够扩展**：核心接口 `ILogger`、`ILogFormatter`、`IUnityThreadLogger` 开放给开发者，可根据需要自定义任何输出目标或日志格式。
- **轻量低耗**：通过 `ConcurrentQueue` 和最小化内存分配，确保高频使用时性能依旧出色（单次日志入队约 8-10 μs，文件写入约 10 μs）。
- **安全可靠**：异常隔离、分阶段释放资源、自动轮转与备份清理，避免日志丢失或文件无限膨胀。

---

## 具体功能

- **控制台输出**：通过 `ConsoleLogger` 输出至 Unity 编辑器控制台，支持 `LogType` 映射与自定义格式化。配合 `LogContext` 可实现点击跳转和对象高亮。
- **文件日志**：通过 `FileLogger` 将日志写入文件，支持文件轮转（按大小）、备份保留数限制、同步/异步写入、缓冲区自动刷新等丰富配置。
- **组合输出**：使用 `CompositeLogger` 将多个记录器组合在一起，一份日志同时发送到多个目标。
- **级别过滤**：基于位域的 `LogLevel` 支持灵活的级别开关，可配置范围、最小/最大级别，甚至任意组合。
- **格式化器**：实现 `ILogFormatter` 接口可自定义日志行样式（如添加帧数、线程ID等），默认格式化器提供标准时间戳与级别前缀。
- **日志上下文**：通过 `LogContext` 携带文件路径、行号、成员名、堆栈和自定义数据。`ConsoleLogger` 支持将 `UserData` 中的 `UnityEngine.Object` 传递给控制台，实现对象高亮。
- **Unity 生命周期安全**：主线程依赖的资源通过 `IUnityThreadLogger` 分阶段释放，协程定时刷新由内部 `Proxy` 驱动，保证退出前数据完整落盘。
- **Debug 接管**：全局替换 `Debug.unityLogger.logHandler`，确保所有 `Debug.Log` 调用都通过 EasyLogger 管线处理。

---

## 快速开始

### 基础用法

```csharp
using EasyLogger.Unity;

// 无需任何配置，直接使用
LogUtility.Info("游戏启动完成");
LogUtility.Warning("血量低于 20%");
LogUtility.Error("加载配置失败");
```

### 带上下文（点击跳转 + 对象高亮）

```csharp
public class PlayerController : MonoBehaviour
{
    private void TakeDamage(int damage, GameObject attacker)
    {
        var ctx = LogContext.CaptureWithUserData(attacker);
        LogUtility.Info($"受到 {damage} 点伤害", ctx);
        // 控制台输出可点击跳转，且 attacker 可高亮
    }
}
```

### 文件日志（异步）

```csharp
var config = LoggerConfig.Builder.Create().Build();

// 需要协程代理（可传入 MonoBehaviour 实例）
var proxy = this as ICoroutineProxy;

var fileConfig = FileLoggerConfig.Builder.Create(config, proxy)
    .SetLogDirectory(Application.persistentDataPath + "/Logs")
    .SetFileNamePrefix("MyGame")
    .SetUseAsync(true)
    .SetFlushIntervalMilliseconds(3000)
    .Build();

var fileLogger = new FileLogger(fileConfig);
LogUtility.Configure(fileLogger);
```

---

## 常见问题

**Q：如何仅记录 Warning 及以上级别的日志？**

A：使用 `LoggerConfig.Builder.Create().SetMinLevel(LogLevel.Warning).Build()` 作为记录器的配置。

**Q：如何自定义日志格式？**

A：实现 `ILogFormatter` 接口，并在配置中通过 `SetFormatter` 注入。

**Q：怎样将日志同时输出到控制台和文件？**

A：利用 `CompositeLogger` 分别添加 `ConsoleLogger` 和 `FileLogger` 实例。

**Q：性能如何？会有 GC 吗？**

A：单次日志入队约 8-10 μs，文件写入约 10 μs，每次分配不到 4 字节（入队阶段），全程无 Gen1/Gen2 回收。

**Q：如何实现点击跳转和对象高亮？**

A：在调用 `LogUtility.Info` 时传入 `LogContext`，其中包含文件路径和行号。`ConsoleLogger` 会自动识别并输出可点击链接；如果 `UserData` 是 `UnityEngine.Object`，控制台会支持高亮。

**Q：能接管现有的 `Debug.Log` 吗？**

A：可以。`UnityDebugHandler` 会自动替换 `Debug.unityLogger.logHandler`，所有 `Debug.Log` 调用都会进入 EasyLogger 管线。

---

## 其它文档

- [API 文档](./documents/1.0.1-beta/DOCUMENT.md)
- [更新日志](./RELEASE.md)
- [测试报告](./tests/1.0.1-beta/TEST_REPORT.md)

---

## 许可证

本项目基于 [MIT 许可证](LICENSE) 发布。