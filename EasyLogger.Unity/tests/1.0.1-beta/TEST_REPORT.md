> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

# EasyLogger.Unity 测试报告

**版本**: 1.0.1-beta  
**报告日期**: 2026-06-16

---

## 一、测试环境

### 1.1 运行环境

| 项目 | 规格 |
| :--- | :--- |
| Unity 版本 | 2020.3.48f1 |
| 脚本后端 | Mono2x |
| 操作系统 | Windows 11 (10.0.26100) 64bit |
| 处理器 | Intel Core i7-14650HX (24 核) |
| 内存 | 16 GB |
| 显卡 | NVIDIA GeForce RTX 4060 Laptop GPU |

### 1.2 测试框架

| 项目 | 版本 |
| :--- | :--- |
| Unity Test Framework | 1.1.33 |
| Unity Performance Testing | 3.0.3 |
| NUnit | 3.5 |

### 1.3 测试类型

| 类型 | 说明 |
| :--- | :--- |
| **单元测试** | 验证各模块功能正确性 |
| **基准测试** | 测量关键路径性能指标 |

---

## 二、单元测试

### 2.1 日志级别测试 (`LogLevelTests`)

| 测试用例 | 说明 | 结果 |
| :--- | :--- | :--- |
| `Values_ShouldBeCorrect` | 验证 `LogLevel` 枚举值正确 | ✅ 通过 |
| `MinMax_ShouldPointToExtremes` | 验证 `Min`/`Max` 指向极值 | ✅ 通过 |
| `Flags_Combination_Works` | 验证位域组合正常工作 | ✅ 通过 |

### 2.2 配置构建器测试 (`LoggerConfigBuilderTests`)

| 测试用例 | 说明 | 结果 |
| :--- | :--- | :--- |
| `Defaults_ShouldBeCorrect` | 验证默认配置值 | ✅ 通过 |
| `SetLevels_ShouldUpdate` | 验证 `SetLevels` 设置正确 | ✅ 通过 |
| `SetMinLevel_ShouldSetRange` | 验证 `SetMinLevel` 范围正确 | ✅ 通过 |
| `SetMaxLevel_ShouldSetRange` | 验证 `SetMaxLevel` 范围正确 | ✅ 通过 |
| `SetMinMaxLevel_ShouldSetRange` | 验证 `SetMinMaxLevel` 范围正确 | ✅ 通过 |
| `SetFormatter_ShouldUpdate` | 验证自定义 Formatter 生效 | ✅ 通过 |
| `SetFormatProvider_ShouldUpdate` | 验证自定义 FormatProvider 生效 | ✅ 通过 |
| `Build_ShouldThrowIfReused` | 验证 Builder 不可复用 | ✅ 通过 |

### 2.3 日志记录器基类测试 (`LoggerBaseTests`)

| 测试用例 | 说明 | 结果 |
| :--- | :--- | :--- |
| `Log_BelowLevel_ShouldNotReachDoLog` | 验证级别过滤（低于阈值） | ✅ 通过 |
| `Log_AtOrAboveLevel_ShouldCallDoLog` | 验证级别过滤（高于阈值） | ✅ 通过 |
| `Log_FormatException_ShouldThrow` | 验证格式异常正确抛出 | ✅ 通过 |
| `Log_WithArgs_ShouldFormat` | 验证参数格式化 | ✅ 通过 |
| `Log_WithFormatProvider_ShouldUseIt` | 验证自定义 FormatProvider | ✅ 通过 |
| `Log_NullArgs_ShouldNotFormat` | 验证空参数不格式化 | ✅ 通过 |
| `Log_WithFormatter_ShouldCallIt` | 验证自定义 Formatter 调用 | ✅ 通过 |
| `Trace_Debug_Info_Warning_Error_Fatal_Shortcuts` | 验证快捷方法 | ✅ 通过 |

### 2.4 控制台日志记录器测试 (`ConsoleLoggerTests`)

| 测试用例 | 说明 | 结果 |
| :--- | :--- | :--- |
| `Info_ShouldLogWithLogTypeLog` | 验证 Info → LogType.Log | ✅ 通过 |
| `Warning_ShouldUseLogTypeWarning` | 验证 Warning → LogType.Warning | ✅ 通过 |
| `Error_ShouldUseLogTypeError` | 验证 Error → LogType.Error | ✅ 通过 |
| `Fatal_ShouldUseLogTypeError` | 验证 Fatal → LogType.Error | ✅ 通过 |
| `FormattedMessage_ShouldContainTimestamp` | 验证格式化包含时间戳 | ✅ 通过 |
| `LevelFiltering_ShouldWork` | 验证级别过滤生效 | ✅ 通过 |

### 2.5 复合日志记录器测试 (`CompositeLoggerTests`)

| 测试用例 | 说明 | 结果 |
| :--- | :--- | :--- |
| `Constructor_WithLoggers_ShouldAddThem` | 验证构造时添加 Logger | ✅ 通过 |
| `Add_Duplicate_ShouldNotAdd` | 验证重复添加被阻止 | ✅ 通过 |
| `Add_Null_ShouldBeIgnored` | 验证空值添加被忽略 | ✅ 通过 |
| `Remove_ShouldStopReceivingLogs` | 验证移除后不再接收日志 | ✅ 通过 |
| `Clear_ShouldRemoveAll` | 验证清空所有 Logger | ✅ 通过 |
| `OneLoggerThrows_OthersStillCalled` | 验证异常隔离 | ✅ 通过 |
| `Dispose_ShouldCallDisposeOnChildren` | 验证 `Dispose` 传播 | ✅ 通过 |
| `DisposeOnUnityThread_ShouldCallOnlyUnityThreadLoggers` | 验证主线程释放 | ✅ 通过 |
| `DisposedCompositeLogger_ShouldThrowOnAdd` | 验证释放后操作抛异常 | ✅ 通过 |
| `DisposedCompositeLogger_ShouldThrowOnLog` | 验证释放后日志抛异常 | ✅ 通过 |

### 2.6 文件日志记录器测试 (`FileLoggerTests`)

| 测试用例 | 说明 | 结果 |
| :--- | :--- | :--- |
| `WriteLog_WritesToFile` | 验证日志写入文件 | ✅ 通过 |
| `SyncLog_ShouldWriteImmediately` | 验证同步写入立即落盘 | ✅ 通过 |
| `AsyncLog_BufferFlushOnSize` | 验证异步缓冲达到阈值刷新 | ✅ 通过 |
| `Rotation_WhenFileExceedsSize_CreatesBackup` | 验证文件轮转创建备份 | ✅ 通过 |
| `Dispose_ShouldCloseFile` | 验证释放后无法继续写入 | ✅ 通过 |
| `DisposeOnUnityThread_ShouldStopCoroutine` | 验证主线程释放停止协程 | ✅ 通过 |

### 2.7 日志工具类测试 (`GameLogTests`)

| 测试用例 | 说明 | 结果 |
| :--- | :--- | :--- |
| `Log_WhenNullLogger_ShouldNotThrow` | 验证空 Logger 抛异常 | ✅ 通过 |
| `Flush_ShouldClearQueue` | 验证 Flush 清空队列 | ✅ 通过 |

**单元测试汇总**：共 **36** 个测试用例，全部 **通过**。

---

## 三、基准测试

### 3.1 测试方法

每个测试用例执行流程：
1. **预热**：执行指定次数（`WarmupCount`）使 JIT 完成编译
2. **测量**：执行指定次数（`MeasureCount`）记录耗时和 GC 分配
3. **GC 统计**：使用 `Unity.PerformanceTesting` 框架自动收集

### 3.2 日志入队性能 (`LogUtilityEnqueuePerformanceTests`)

测试单条日志入队（无 Logger 输出）的耗时和 GC 分配。

| 测试用例 | 平均耗时 (ms) | GC 分配 (B) | 单条耗时 (μs) |
| :--- | :--- | :--- | :--- |
| `Measure_Info_Enqueue_Performance` | 0.81 | 385 | 8.1 |
| `Measure_Warning_Enqueue_Performance` | 0.86 | 395 | 8.6 |
| `Measure_Error_Enqueue_Performance` | 0.93 | 396 | 9.3 |
| `Measure_Trace_Enqueue_Performance` | 0.92 | 395 | 9.2 |
| `Measure_Mixed_Levels_Enqueue_Performance` | 0.91 | 395 | 9.1 |

**结论**：
- 单次入队约 **8-10 μs**
- 每次迭代分配约 **4 字节**
- 不同日志级别性能差异可忽略

### 3.3 日志刷新性能 (`LogUtilityFlushPerformanceTests`)

#### 3.3.1 空队列刷新

| 测试用例 | 平均耗时 (ms) | GC 分配 (B) |
| :--- | :--- | :--- |
| `Measure_Flush_Empty_Queue` | 0.0012 | 0 |

**结论**：空队列刷新 **1.2 μs**，零分配。

#### 3.3.2 队列消息刷新

预先入队 N 条消息，测量 `Flush()` 耗时。

| 消息数 | 平均耗时 (ms) | GC 分配 (B) |
| :--- | :--- | :--- |
| 10 | 0.0010 | 0 |
| 100 | 0.0012 | 0 |
| 1000 | 0.0012 | 0 |

**结论**：`Flush()` 耗时与队列长度无关，始终约 **1-1.2 μs**，零 GC。

#### 3.3.3 批量入队 + 刷新

| 消息数 | 平均耗时 (ms) | GC 分配 (B) | 单条耗时 (μs) |
| :--- | :--- | :--- | :--- |
| 10 | 0.009 | 20 | 0.9 |
| 100 | 0.104 | 338 | 1.0 |
| 1000 | 5.62 | 3938 | 5.6 |

**结论**：批量操作时单条消息耗时降低，1000 条消息约 5.6 ms。

### 3.4 复合日志记录器性能 (`CompositeLoggerPerformanceTests`)

| 测试用例 | 平均耗时 (ms) | GC 分配 (B) |
| :--- | :--- | :--- |
| `Measure_CompositeLogger_Dynamic_Add_Remove_Performance` | 0.027 | 201 |
| `Measure_CompositeLogger_Many_Loggers_Performance(1)` | 0.134 | 802 |
| `Measure_CompositeLogger_Many_Loggers_Performance(3)` | 0.339 | 2002 |
| `Measure_CompositeLogger_Many_Loggers_Performance(5)` | 0.567 | 3204 |
| `Measure_CompositeLogger_Two_Loggers_Performance` | 33.79 | 4916 |

**结论**：
- 纯内存 Logger：耗时与 Logger 数量成正比
- 含 `ConsoleLogger` 时耗时约 33.8 ms（控制台输出是主要瓶颈）

### 3.5 控制台日志记录器性能 (`ConsoleLoggerPerformanceTests`)

| 测试用例 | 平均耗时 (ms) | GC 分配 (B) | 单条耗时 (μs) |
| :--- | :--- | :--- | :--- |
| `Measure_ConsoleLogger_Info_Performance` | 17.77 | 4303 | 177.7 |
| `Measure_ConsoleLogger_With_Formatter_Performance` | 16.95 | 1902 | 169.5 |

**结论**：
- 单条控制台日志约 **170-180 μs**
- 控制台输出是日志链路的主要瓶颈（Unity 内部处理开销）

### 3.6 文件日志记录器性能 (`FileLoggerPerformanceTests`)

#### 3.6.1 同步写入

| 消息数 | 平均耗时 (ms) | GC 分配 (B) | 单条耗时 (μs) |
| :--- | :--- | :--- | :--- |
| 10 | 0.114 | 369 | 11.4 |
| 100 | 1.037 | 3699 | 10.4 |
| 500 | 4.97 | 18499 | 9.9 |

#### 3.6.2 异步写入

| 消息数 | 平均耗时 (ms) | GC 分配 (B) | 单条耗时 (μs) |
| :--- | :--- | :--- | :--- |
| 10 | 0.090 | 371 | 9.0 |
| 100 | 0.713 | 3706 | 7.1 |
| 500 | 4.68 | 18529 | 9.4 |

#### 3.6.3 文件轮转

| 测试用例 | 平均耗时 (ms) | GC 分配 (B) |
| :--- | :--- | :--- |
| `Measure_FileLogger_Rotation_Performance` | 90.15 | 34756 |

**结论**：
- 同步单条写入约 **10-12 μs**
- 异步单条写入约 **7-10 μs**，略快于同步
- 文件轮转约 **90 ms**，可接受

### 3.7 压力测试 (`StressTests`)

| 测试用例 | 说明 | 总耗时 (ms) | 总消息量 | 单条耗时 (μs) |
| :--- | :--- | :--- | :--- | :--- |
| `Measure_High_Frequency_Logging_Stress` | 10000 条，批量 Flush | 3042 | 10000 | 304 |
| `Measure_MultiThread_Logging_Stress` | 4 线程 × 2500 条 | 15.7 | 10000 | 1.6 |
| `Measure_Queue_Overflow_Stress` | 5000 条，容量无限 | 1.35 | 5000 | 0.27 |
| `Measure_AutoFlush_Stress` | 5000 条，0.1s 自动刷新 | 1.55 | 5000 | 0.31 |

**结论**：
- 多线程入队性能极优，10000 条仅 15.7 ms
- 队列无容量限制，5000 条入队仅 1.35 ms
- 自动刷新机制正常工作，开销可控

---

## 四、基准测试结果汇总

| 测试类别 | 关键指标 | 结果 |
| :--- | :--- | :--- |
| 日志入队 | 单条耗时 | **8-10 μs** |
| 日志入队 | 单条 GC | **~4 字节** |
| 空队列刷新 | 耗时 | **1.2 μs** |
| 空队列刷新 | GC | **0 字节** |
| 控制台输出 | 单条耗时 | **170-180 μs** |
| 文件同步写入 | 单条耗时 | **10-12 μs** |
| 文件异步写入 | 单条耗时 | **7-10 μs** |
| 复合 Logger (1个) | 总耗时 | **0.13 ms** |
| 复合 Logger (5个) | 总耗时 | **0.57 ms** |
| 高频压力 | 10000 条 | **3042 ms** |
| 多线程压力 | 10000 条 | **15.7 ms** |

---

## 五、性能评级

| 组件 | 等级 | 说明 |
| :--- | :--- | :--- |
| 日志入队 | ⭐⭐⭐⭐⭐ | 极轻量，适合高频调用 |
| 文件输出 | ⭐⭐⭐⭐ | 10 μs/条，优秀 |
| 控制台输出 | ⭐⭐⭐ | 180 μs/条，可接受 |
| 复合记录器 | ⭐⭐⭐⭐ | 与 Logger 数量成正比 |
| 多线程安全 | ⭐⭐⭐⭐⭐ | 并发入队无锁竞争 |
| 压力耐受 | ⭐⭐⭐⭐ | 队列无容量限制，入队极快 |

---

**报告生成时间**: 2026-06-16  
**测试版本**: EasyLogger.Unity 1.0.1-beta