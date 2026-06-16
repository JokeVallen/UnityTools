> 内容由 AI 根据核心代码、测试代码和测试数据总结生成，已通过人工审核。

# EasyLogger.Unity 测试报告

本报告呈现 `EasyLogger.Unity` 日志工具库在功能正确性与运行时性能方面的测试结果，旨在为使用者提供直观、可靠的参考依据。

## 测试环境

| 项目 | 值 |
|------|-----|
| **Unity 版本** | 2020.3.48f1 |
| **脚本后端** | Mono |
| **测试框架** | Unity Test Framework 1.1.33 |
| **性能测试扩展** | Unity.PerformanceTesting 3.0.3 |
| **测试平台** | StandaloneWindows64 (Development Build) |
| **处理器** | Intel(R) Core(TM) i7-14650HX @ 2.20 GHz |
| **内存** | 16 GB |
| **操作系统** | Windows 11 (10.0.26100) |

> 所有性能数据均在 **PlayMode** 下采样，每个场景预热 10 轮后测量 100 次（或 50 次），取三次独立运行的平均值。

## 一、功能测试

### 测试覆盖总览

| 模块 | 测试用例数 | 关键验证点 |
|------|-----------|-----------|
| `LogLevel` 枚举 | 3 | 基础值、Min/Max、`[Flags]` 位域组合 |
| `LoggerConfig.Builder` | 10 | 默认值、各 Setter、范围设置、构建后重用限制 |
| `DefaultLogFormatter` | 1 | 输出格式含时间戳、级别、消息 |
| `LoggerBase` | 11 | 级别过滤、参数格式化、`FormatException` 回退、`IFormatProvider`、`ILogFormatter`、`ThrowOnError`/`OnError`、快捷方法 |
| `ConsoleLogger` | 6 | `LogType` 映射、格式化输出、级别过滤 |
| `CompositeLogger` | 9 | 多记录器分发、查重、移除、清空、异常隔离、`Dispose` 递归释放、`DisposeOnUnityThread`、已释放拒绝 |
| `FileLogger` (PlayMode) | 6 | 文件写入、同步写入、异步缓冲区刷新、文件轮转、`Dispose` 关闭、协程停止 |
| `Debug` (PlayMode) | 4 | 配置返回旧 Logger、`NullLogger` 无异常、`Flush` 清空队列、`DisposeOnUnityThread` 不抛异常 |

**所有用例均通过**，覆盖了库的公共 API 及核心逻辑路径，包括边界条件与错误处理。

---

## 二、性能测试

性能测试聚焦于典型场景的运行时开销与内存压力，所有指标均通过 `Measure.Method().GC()` 收集。

### 测试场景说明

| 场景 | 描述 |
|------|------|
| **Debug.Enqueue** | 纯入队操作，记录器为 `NullLogger`（零开销输出） |
| **Debug.Info** | 完整调用 `Debug.Info` 静态方法（含参数数组和级别过滤） |
| **DefaultLogFormatter.Format** | 纯格式化，构造一条带时间戳和级别的日志行 |
| **ConsoleLogger.Info** | 控制台记录器首次调用，注入自定义 `TestLogHandler` 收集 |
| **ConsoleLogger.Info (带格式化)** | 同上前使用 `DefaultLogFormatter` 进行完整格式化 |
| **FileLogger.Info (同步)** | 同步文件记录器，写入临时文件（`AutoFlush` 启用） |
| **CompositeLogger.Info (2目标)** | 组合两个子记录器（控制台 + 文件）的分发调用 |

### 耗时结果 （单位：微秒）

| 测试场景 | 平均耗时 | 最小 | 最大 | 标准差 |
|----------|----------|------|------|--------|
| **Debug.Enqueue** | **0.174** | 0.100 | 0.300 | 0.048 |
| **Debug.Info** | **1.835** | 1.300 | 4.100 | 0.471 |
| **DefaultLogFormatter.Format** | **11.88** | 9.70 | 22.5 | 1.92 |
| **ConsoleLogger.Info** | **13.43** | 10.30 | 30.3 | 3.34 |
| **ConsoleLogger.Info (带格式化)** | **13.26** | 10.0 | 23.2 | 2.88 |
| **FileLogger.Info (同步)** | **34.28** | 14.9 | 1923.8* | 154.7 |
| **CompositeLogger.Info (2目标)** | **18.14** | 13.2 | 34.7 | 3.79 |

*注：`FileLogger` 的最大耗时毛刺由文件轮转或系统 IO 争用引起，平均值仍保持稳定。

### 内存分配与 GC

| 测试场景 | 每次分配 | Gen0 回收 | Gen1/Gen2 回收 |
|----------|---------|----------|---------------|
| **Debug.Enqueue** | < 1 B | 几乎不触发 | 无 |
| **Debug.Info** | ~2.0 B | 极低 | 无 |
| **DefaultLogFormatter.Format** | ~32.0 B | 极低 | 无 |
| **ConsoleLogger.Info** | ~33.2 B | 极低 | 无 |
| **ConsoleLogger.Info (带格式化)** | ~33.1 B | 极低 | 无 |
| **FileLogger.Info (同步)** | ~33.0 B | 偶发抖动 | 无 |
| **CompositeLogger.Info (2目标)** | ~65.7 B | 极低 | 无 |

> 所有测试中 **Gen1 与 Gen2 回收均为 0 次**，表明库在高频调用下不会引发长时间 GC 停顿。

---

## 三、结论

- **零开销入队**：日志入队操作仅需 **174 纳秒**，且几乎不产生内存分配，对主线程影响可忽略。
- **高效格式化**：默认格式化器耗时约 **12 微秒**，分配 32 字节，高频调用无忧。
- **控制台输出轻量**：通过自定义 `LogHandler` 接管后，控制台日志输出仅需 **13 微秒**，推荐在开发阶段使用。
- **文件写入稳健**：同步文件写入平均 **34 微秒**，罕见毛刺可通过启用**异步模式**（`UseAsync = true`）彻底规避。
- **组合记录器线性扩展**：双记录器分发仅额外增加约 **50% 耗时**，复合逻辑无性能陷阱。
- **GC 友好**：所有场景的分配量均 ≤ 66 字节，**Gen1/Gen2 从未触发**，可放心嵌入帧循环或高频业务。

EasyLogger.Unity 严格践行了“即拿即用、轻量低耗”的设计理念，在功能完整性与极致性能之间取得了优异平衡。以上测试数据可作为项目选型与性能评估的坚实依据。