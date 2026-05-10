> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

# 测试报告

## 测试环境

| 项目 | 说明 |
|------|------|
| **Unity 版本** | 2020.3.48f1 (b805b124c6b7) |
| **运行模式** | EditMode |
| **脚本后端** | Mono2x |
| **渲染线程** | 多线程 |
| **色彩空间** | Gamma |
| **图形 API** | Direct3D11 |
| **操作系统** | Windows 11 (10.0.26100) 64bit |
| **CPU** | Intel(R) Core(TM) i7-14650HX @ 24 核心 |
| **GPU** | NVIDIA GeForce RTX 4060 Laptop GPU |
| **内存** | 16087 MB |
| **测试框架** | Unity TestFramework 1.1.33, Performance Testing API 3.0.3 |

---

## 测试类型

### 一、单元测试

测试覆盖 `GeneratorConfigAttribute`、`BaseGeneratorMediator` 以及核心接口的基本契约。

#### 1.1 测试用例及说明

| 测试用例 | 说明 |
|----------|------|
| `Constructor_Sets_TemplatePath_And_OutputPath` | 验证 `GeneratorConfigAttribute` 构造函数正确保存模板路径和输出路径 |
| `AddGenerator_And_Retrieve_By_Indexer` | 验证 `BaseGeneratorMediator` 字典添加后可通过类型索引获取元数据 |
| `ContainsKey_Returns_True_For_Existing_Type` | 测试 `ContainsKey` 方法对已注册和未注册类型的判断 |
| `Keys_And_Values_Reflect_Current_Dictionary` | 确保 `Keys`、`Values`、`Count` 与实际字典内容一致 |
| `Clear_Disposes_IDisposable_Generators` | 验证 `Clear` 时会释放实现了 `IDisposable` 的生成器，并清空字典 |
| `Clear_Does_Not_Throw_On_Non_Disposable_Generators` | 验证对于未实现 `IDisposable` 的生成器，`Clear` 不会抛出异常 |
| `TryGetValue_Returns_False_For_Missing_Type` | 验证对不存在的类型调用 `TryGetValue` 返回 `false` |
| `DisposeInstance_Disposes_IDisposable` | 验证静态 `DisposeInstance` 能够正确释放 `IDisposable` 对象 |
| `SyncGenerator_Generate_Transforms_Template` | 验证同步生成器接口的正确实现类能正常转换模板 |
| `AsyncGenerator_Returns_Task` | 验证异步生成器返回的 Task 结果正确 |
| `TemplateProvider_Returns_Content` | 验证模板提供者接口能按路径返回内容 |
| `Writer_Writes_Content` | 验证写入器接口能正确接收路径和内容参数 |

#### 1.2 测试结果

**全部 12 个单元测试用例均通过**，无失败，无错误，框架抽象行为符合设计预期。

---

### 二、基准测试（性能测试）

通过 `Measure.Method` 和 `GC()` 统计各核心环节的耗时（单位：毫秒）与内存分配（单位：KB）。样本已包含预热、多次测量，结果稳定可靠。

#### 2.1 同步生成器性能

| 测试用例 | 说明 | 平均耗时 (ms) | 平均 GC 分配 (KB) | 备注 |
|----------|------|---------------|-------------------|------|
| `Generate_SmallTemplate_Once` | 短模板 (5 字符) 单次生成 | 0.00437 | 0 | 基准参考，几乎无开销 |
| `Generate_LargeTemplate_Repeat10` | 大模板 (1000 字符) ×10 拼接 | 1.301 | 9 | 字符串 `+` 导致时间与 GC 显著上升 |
| `Generate_StressTest_Repeat100` | 短模板 ×100 拼接，高强度 | 0.153 | 99 | 大量临时字符串分配，是主要性能瓶颈 |

**分析**：`result += template` 循环产生 O(n²) 的临时对象，重复次数越多，GC 压力越大。将拼接逻辑替换为 `StringBuilder` 可消除大部分分配，使耗时降低至微秒级。

#### 2.2 异步生成器性能

| 测试用例 | 说明 | 平均耗时 (ms) | 平均 GC 分配 (KB) |
|----------|------|---------------|-------------------|
| `GenerateAsync_WithoutCancellation` | 异步生成 x10，包含 `Task.Yield` | 0.0349 | 8 |

**分析**：异步包装引入了每个任务约 8KB 的状态机分配。若生成逻辑本质为 CPU 计算，建议优先使用同步接口以避免不必要的分配。

#### 2.3 模板提供者性能

| 测试用例 | 说明 | 平均耗时 (ms) | 平均 GC 分配 (KB) |
|----------|------|---------------|-------------------|
| `GetTemplate_SmallSize` | 提供 100 字符模板 | 0.0261 | 1 |
| `GetTemplate_LargeSize` | 提供 100,000 字符模板 | 0.4585 | 1 |

**分析**：字符串分配成本随长度线性增长。对于大模板，可考虑流式加载或缓存来分担压力。

#### 2.4 写入器性能

| 测试用例 | 说明 | 平均耗时 (ms) | 平均 GC 分配 (KB) |
|----------|------|---------------|-------------------|
| `Write_LargeContent` | 写入 10,000 字符内容 (空操作) | 0.00117 | ≈0 |

**分析**：抽象写入器本身无性能负担，真实性能取决于具体实现（如文件 I/O）。实际应用中应针对具体写入器补充测试。

#### 2.5 中介者批量执行性能

| 测试用例 | 说明 | 平均耗时 (ms) | 平均 GC 分配 (KB) |
|----------|------|---------------|-------------------|
| `RunAll_FiveGenerators` | 同时运行 5 个生成器（每个重复3次） | 0.0114 | 3 |
| `RunAll_TwentyGenerators_Stress` | 同时运行 20 个生成器（每个重复2次） | 0.0071 | 2 |

**说明**：20 个生成器的测试因迭代次数少且单次工作量较小，出现了比 5 个更快的结果。在实际均匀负载下，执行时间应与生成器数量成线性关系。建议统一每个生成器的工作量并增加测量迭代次数以得到更一致的对比。

---

## 结论

- **功能正确性**：所有单元测试通过，框架接口和基类行为符合设计预期。
- **性能表现**：核心抽象层调用成本极低（微秒级）。潜在的性能瓶颈源于 **具体实现中的字符串拼接** 等算法细节，通过标准优化方法（如 `StringBuilder`）可轻松解决。
- **内存分配**：除故意使用低效拼接外，框架自身不会引起显著的 GC 压力。异步接口合理使用可控制额外的状态机分配。
- **建议**：在生产实现中为模板提供和写入环节补充实际 IO 的性能测试，并对关键生成代码应用常规 C# 性能最佳实践。
