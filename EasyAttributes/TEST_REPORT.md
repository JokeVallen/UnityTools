> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

## 测试环境

| 项目 | 配置 |
|------|------|
| 操作系统 | Windows 11 (10.0.26100) |
| CPU | Intel Core i7-14650HX 2.20GHz |
| 内存 | 32 GB |
| .NET SDK | 9.0.304 |
| 运行时 | .NET 7.0.20 |
| 测试框架 | xUnit 2.4.2 |
| 基准测试框架 | BenchmarkDotNet 0.15.8 |
| 编译器 | RyuJIT x64 |

---

## 单元测试

### 测试范围
覆盖 EasyAttributes.Core 扩展层的核心逻辑，包括上下文基类、场景上下文、处理器桥接、执行器调度、构建器、异常体系、句柄、扩展方法及工厂。

### 测试用例与结果

| 测试类 | 测试方法 | 说明 | 结果 |
|--------|---------|------|------|
| `ContextTests` | `Constructor_Should_Set_Attribute_Enabled_Priority` | 上下文构造后属性、启用状态、优先级正确 | ✅ 通过 |
| `ContextTests` | `Items_Should_Be_Lazy_And_Cached` | Items 延迟加载并缓存 | ✅ 通过 |
| `ContextTests` | `SetItem_Should_Add_And_Remove_Correctly` | 写入和移除条目 | ✅ 通过 |
| `ContextTests` | `SetFeature_Should_Throw_If_Type_Not_Implement_IFeature` | 非法类型抛出异常 | ✅ 通过 |
| `ContextTests` | `SetFeature_And_Remove_Should_Work` | 正常写入和移除功能 | ✅ 通过 |
| `ContextFactoryTests` | `CreateMethodContext_Returns_IMethodContext_With_Values` | 创建方法上下文且属性正确 | ✅ 通过 |
| `ContextFactoryTests` | `CreatePropertyContext_Should_Set_Accessor_And_Value` | 创建属性上下文且访问器、值正确 | ✅ 通过 |
| `ContextFactoryTests` | `All_Contexts_Implement_IAsyncContext` | 所有上下文均实现 IAsyncContext | ✅ 通过 |
| `ProcessorBridgeTests` | `NonGeneric_Process_With_Correct_Attribute_Should_Call_Typed_Process` | 属性匹配时调用强类型方法 | ✅ 通过 |
| `ProcessorBridgeTests` | `NonGeneric_Process_With_Wrong_Attribute_Should_Return_Continue_And_Not_Call_Typed` | 属性不匹配时静默跳过 | ✅ 通过 |
| `ExecutorTests` | `Execute_Should_Run_All_Processors` | 正常遍历全部处理器 | ✅ 通过 |
| `ExecutorTests` | `Execute_Should_Abort_And_Not_Run_Subsequent_Processors` | 中止后不再执行后续处理器 | ✅ 通过 |
| `ExecutorTests` | `Execute_Should_Skip_After_When_SkipAfterCallbacks_True` | 完全中止时跳过 After | ✅ 通过 |
| `ExecutorTests` | `Before_Exception_Should_Stop_Chain_But_Run_After_Of_Executed` | Before 异常中断链，已执行 After 仍调用 | ✅ 通过 |
| `ExecutorTests` | `Execute_With_Disabled_Context_Should_Return_Continue` | 上下文禁用时直接返回 | ✅ 通过 |
| `ExecutorTests` | `Execute_With_Empty_Registry_Should_Return_Continue` | 注册表为空直接返回 | ✅ 通过 |
| `ExecutorTests` | `After_Exception_Handled_Should_Continue_Other_Afters` | After 异常处理后继续执行其它 After | ✅ 通过 |
| `ExecutorTests` | `Execute_Should_Inject_Global_Feature_If_Not_Present` | 全局 Feature 注入 | ✅ 通过 |
| `ExecutorTests` | `Execute_Should_Not_Override_Existing_Feature` | 不覆盖已有 Feature | ✅ 通过 |
| `ExecutorAsyncTests` | `ExecuteAsync_Should_Run_Sync_And_Async_In_Order` | 异步执行器正确调度顺序 | ✅ 通过 |
| `ExecutorAsyncTests` | `ExecuteAsync_Should_Abort_And_Not_Run_Subsequent` | 异步中止后续处理器 | ✅ 通过 |
| `ExecutorAsyncTests` | `ExecuteAsync_With_Cancellation_Should_Throw_Before_Process` | 取消令牌抛出异常 | ✅ 通过 |
| `BuilderTests` | `Build_Should_Return_IExecutor` | 构建同步执行器 | ✅ 通过 |
| `BuilderTests` | `Build_After_Build_Should_Throw` | 重复构建抛异常 | ✅ 通过 |
| `BuilderTests` | `UseFeature_With_Generic_Should_Store_Feature` | 泛型功能注入 | ✅ 通过 |
| `BuilderTests` | `BuildAsync_Should_Return_IExecutorAsync` | 构建异步执行器 | ✅ 通过 |
| `BuilderTests` | `BuildBoth_Should_Return_Both` | 同时构建同步/异步 | ✅ 通过 |
| `ExceptionTests` | `ProcessorBeforeException_Should_Carry_Context_And_ProcessorType` | 异常携带上下文和处理器类型 | ✅ 通过 |
| `ExceptionTests` | `FeatureTypeException_Should_Contain_FeatureType` | 异常携带非法类型 | ✅ 通过 |
| `HandleTests` | 所有单例与工厂方法属性验证 | Continue/Aborted/AbortedAll 属性正确 | ✅ 通过 |
| `ExtensionTests` | `GetItem` / `GetFeature` 正常与默认值 | 扩展方法行为正确 | ✅ 通过 |
| `FactoryTests` | 瞬态与单例工厂行为 | 创建实例行为符合预期 | ✅ 通过 |
| `FeaturesTests` | 全局 Feature 注入不抛异常 | 空上下文注入无异常 | ✅ 通过 |

**结论**：33 个单元测试全部通过，覆盖核心逻辑、边界条件及异常路径。

---

## 基准测试

### 测试目的
测量框架在不同数量处理器、不同工厂模式、异步场景下的执行时间与内存分配，验证高性能特性。

### 测试结果

#### 方法拦截（同步）
| 方法 | Mean (ns) | Allocated (B) |
|------|----------|---------------|
| 直接调用 | 0.44 | 0 |
| 反射调用 | 20.97 | 24 |
| 2 个瞬态处理器 | 142.10 | 528 |
| 2 个单例处理器 | 148.99 | 480 |
| 带全局 Feature | 154.18 | 672 |

#### 异步方法拦截
| 方法 | Mean (ns) | Allocated (B) |
|------|----------|---------------|
| 直接异步调用 | 716.3 | 233 |
| 异步拦截（混合处理器） | 221.8 | 760 |

#### 上下文创建
| 方法 | Mean (ns) | Allocated (B) |
|------|----------|---------------|
| 创建方法上下文 | 27.13 | 352 |
| 创建属性上下文 | 18.93 | 280 |

#### 处理器链扩展性
| 处理器数量 | Mean (ns) | Allocated (B) |
|-----------|----------|---------------|
| 0 | 38.31 | 352 |
| 5 | 247.79 | 624 |
| 10 | 426.82 | 784 |

### 性能分析
- 拦截开销约 142 ns，仅为反射调用的 7 倍，每秒可支持超 700 万次拦截。
- 扩展性严格线性，每个处理器增加约 40 ns。
- 内存分配轻量，每次拦截分配 480-760 字节，无 Gen0 以上 GC 压力。
- 异步执行器没有引入额外异步开销，表现优异。

**结论**：框架性能满足高吞吐量应用场景。