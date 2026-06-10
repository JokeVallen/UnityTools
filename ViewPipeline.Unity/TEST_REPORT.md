# ViewPipeline - 测试报告

> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

## 测试环境

### 硬件环境

| 配置项 | 详细信息 |
|--------|----------|
| 操作系统 | Windows 11 (10.0.26100) 64bit |
| 设备型号 | OMEN Gaming Laptop 16-ae0xxx (HP) |
| 处理器 | Intel(R) Core(TM) i7-14650HX (24 核) |
| 显卡 | NVIDIA GeForce RTX 4060 Laptop GPU |
| 系统内存 | 16087 MB |

### 软件环境

| 配置项 | 详细信息 |
|--------|----------|
| Unity 版本 | 2020.3.48f1 |
| 脚本后端 | Mono 2x |
| 图形 API | Direct3D11 |
| 颜色空间 | Gamma |
| 渲染线程模式 | MultiThreaded |

### 依赖库

| 依赖 | 版本 |
|------|------|
| UniTask | - |
| Unity Test Framework | 1.1.33 |
| Unity Performance Testing | 3.0.3 |
| NUnit | 1.0+ |


## 测试类型

### 1. 单元测试（EditMode）

验证框架各模块的功能正确性，包括基础功能、中间件执行、拦截机制、动态供应器、执行策略、扩展包、强类型上下文、资源释放等。

### 2. 基准/性能测试（PlayMode）

测量框架在真实 Unity 运行环境下的性能表现，包括构建耗时、打开/关闭视图耗时、GC 分配、压力测试等。


## 单元测试用例及结果

### 基础功能测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `BuildSession_ShouldNotThrow` | 构建默认会话不应抛出异常 | ✅ 通过 |
| `OpenViewAsync_WithoutMiddleware_ShouldShowView` | 无中间件时打开视图应正常显示 | ✅ 通过 |
| `CloseViewAsync_WithoutMiddleware_ShouldHideView` | 无中间件时关闭视图应正常隐藏 | ✅ 通过 |
| `OpenViewAsync_WithStaticMiddleware_ShouldExecuteMiddleware` | 静态中间件应正常执行 | ✅ 通过 |
| `OpenViewAsync_WithMultipleMiddlewares_ShouldExecuteInOrder` | 多个中间件应按顺序执行 | ✅ 通过 |

**说明**：中间件执行顺序验证为 A-Before → B-Before → B-After → A-After，符合洋葱模型预期。

### 拦截测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `OpenViewAsync_WithInterceptedMiddleware_ShouldNotShowView` | 中间件拦截后视图不应显示 | ✅ 通过 |
| `OpenViewAsync_WithInterceptedMiddleware_ShouldAllowReopenAfterRollback` | 拦截后回滚状态应允许重新打开 | ✅ 通过 |

### 动态中间件测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `OpenViewAsync_WithDynamicProvider_ConditionTrue_ShouldAddMiddleware` | 条件满足时应添加动态中间件 | ✅ 通过 |
| `OpenViewAsync_WithDynamicProvider_ConditionFalse_ShouldNotAddMiddleware` | 条件不满足时不应添加动态中间件 | ✅ 通过 |

### 执行策略测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `OpenViewAsync_WithExecutionPolicy_ShouldSkipSpecifiedMiddleware` | 执行策略应能跳过指定中间件 | ✅ 通过 |

### 扩展包测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `AddExtension_ShouldAddAllMiddlewares` | 扩展包应能批量添加中间件 | ✅ 通过 |
| `ExtensionWithTypedContextRequirement_WithoutWithTypedContext_ShouldNotBuild` | 扩展包验证器检查失败时不添加 | ✅ 通过 |
| `ExtensionWithTypedContextRequirement_WithWithTypedContext_ShouldBuild` | 扩展包验证器检查通过时正常添加 | ✅ 通过 |

### 强类型上下文测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `WithTypedContext_MultipleMiddlewares_ShouldShareData` | 强类型上下文多中间件数据共享 | ✅ 通过 |
| `WithoutTypedContext_ContextShouldNotSupportTypedOperations` | 未启用时强类型操作不可用 | ✅ 通过 |

### 异常测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `OpenViewAsync_WithNullView_ShouldThrow` | 空视图应抛出参数异常 | ✅ 通过 |
| `BuildSession_ReuseBuilder_ShouldThrow` | 重复使用构建器应抛出无效操作异常 | ✅ 通过 |

### 资源释放测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `DisposeAsync_ShouldCompleteWithoutError` | 异步释放应正常完成 | ✅ 通过 |
| `OpenViewAsync_AfterDispose_ShouldThrow` | 释放后再操作应抛出异常 | ✅ 通过 |


## 性能测试结果

### 1. 构建会话性能

| 测试场景 | 中间件数量 | 平均耗时 (ms) | 最小 (ms) | 最大 (ms) |
|----------|:----------:|--------------:|----------:|----------:|
| BuildSession_NoMiddleware | 0 | 0.0058 | 0.0042 | 0.0085 |
| BuildSession_WithMiddlewares | 1 | 0.0042 | 0.0029 | 0.0063 |
| BuildSession_WithMiddlewares | 5 | 0.0050 | 0.0031 | 0.0063 |
| BuildSession_WithMiddlewares | 10 | 0.0066 | 0.0039 | 0.0079 |
| BuildSession_WithMiddlewares | 20 | 0.0095 | 0.0059 | 0.0123 |
| BuildSession_WithTypedContext | - | 0.0053 | 0.0044 | 0.0066 |

**结论**：构建 20 个中间件的会话耗时约 0.01ms，开销极小，与中间件数量呈近似线性关系。

### 2. 打开/关闭视图性能

| 测试场景 | 中间件数量 | 平均耗时 (ms) | GC 分配 (字节) |
|----------|:----------:|--------------:|---------------:|
| OpenCloseView_NoMiddleware | 0 | 0.0206 | 71 |
| OpenCloseView_WithMiddlewares | 1 | 0.0236 | 77 |
| OpenCloseView_WithMiddlewares | 5 | 0.0266 | 91 |
| OpenCloseView_WithMiddlewares | 10 | 0.0311 | 108 |
| OpenCloseView_WithMiddlewares | 20 | 0.0378 | 140 |
| OpenCloseView_WithCloseMiddlewares | 5 | 0.0255 | 91 |
| OpenCloseView_WithTypedContext | - | 0.0239 | 74 |

**结论**：
- 单次 Open+Close 操作在无中间件时约 0.021ms
- 每增加一个中间件，耗时增长约 0.001ms
- GC 分配与中间件数量成正相关（每个中间件约 3-4 字节）

### 3. GC 分配详细测试

| 测试场景 | 预热次数 | 测量次数 | GC 分配 (字节) | 平均耗时 (ms) |
|----------|:--------:|:--------:|---------------:|--------------:|
| GCAllocation_NoMiddleware | 10 | 20 | 71 | 0.0228 |
| GCAllocation_WithTypedContext | 10 | 20 | 74-77 | 0.0234 |

**结论**：框架核心路径已实现低 GC 分配，每次操作约 70-80 字节，TypedContext 增加约 3-6 字节。

### 4. 中间件开销对比

| 测试场景 | 样本数 | 平均耗时 (ms) | 说明 |
|----------|:------:|--------------:|------|
| 无中间件基线 | 10 | 约 0.011 | 第一次运行（含冷启动） |
| 5 个空中间件 | 10 | 约 0.022 | 包含中间件遍历开销 |
| 10 个空中间件 | 10 | 约 0.022 | 开销增长不明显 |

**注意**：数据中存在部分异常高值（最高 0.272ms），可能由于首次 JIT 编译或系统干扰导致，不影响常规性能评估。

### 5. 压力测试（顺序操作）

| 操作次数 | 测量次数 | 平均耗时 (ms/次) | 总耗时 (ms) |
|----------|:--------:|-----------------:|------------:|
| 10 | 3 | 0.028 | 0.28 |
| 100 | 3 | 0.0056 | 0.56 |
| 500 | 3 | 0.0043 | 2.15 |

**结论**：500 次顺序 Open+Close 操作总耗时约 2.15ms，平均单次操作约 0.004ms，线性扩展良好。

### 6. 压力测试（并行操作）

| 并发视图数 | 测量次数 | 平均耗时 (ms) | 说明 |
|:----------:|:--------:|--------------:|------|
| 10 | 3 | 0.041 | 10 个视图并行打开+关闭 |
| 50 | 3 | 0.200 | 50 个视图并行打开+关闭 |

**结论**：框架能够正确处理并发操作，并行执行性能表现稳定。

### 7. 强类型上下文性能

| 测试场景 | 平均耗时 (ms/次) | 1000 次操作总耗时 (ms) | GC 分配 (字节) |
|----------|-----------------:|----------------------:|---------------:|
| TypedContext_ReadWrite_Overhead | 0.030 | - | 85 |
| TypedContext_ReadWrite_1000Operations | 0.0055 | 6.5 | - |

**结论**：TypedContext 单次读写约 0.03ms，1000 次操作约 6.5ms，GC 分配约 85 字节。

### 8. 真实场景模拟（电商页面）

模拟包含 5 个中间件的真实页面流程：Auth → Cache → Loading → Analytics → Animation

| 指标 | 数值 |
|------|------|
| 平均耗时 | 0.028 - 0.033 ms |
| GC 分配 | 91-100 字节 |
| 样本数 | 10 次测量 |

**结论**：典型业务场景下，框架开销约 0.03ms，对游戏性能影响可忽略不计。


## 性能基准总结

| 场景 | 耗时 (ms) | GC (bytes) | 评价 |
|------|----------:|-----------:|------|
| 构建会话（20中间件） | 0.010 | ~0 | ✅ 优秀 |
| 空视图 Open+Close | 0.021 | 71 | ✅ 优秀 |
| 5 中间件 Open+Close | 0.027 | 91 | ✅ 优秀 |
| 20 中间件 Open+Close | 0.038 | 140 | ✅ 良好 |
| TypedContext 单次读写 | 0.030 | 85 | ✅ 优秀 |
| 500 次顺序操作 | 2.15 | - | ✅ 优秀 |
| 50 视图并发 | 0.200 | - | ✅ 良好 |


## 测试结论

1. **功能完整性**：所有单元测试均通过，框架的核心功能（中间件执行、拦截、动态供应、策略、扩展包、强类型上下文、资源管理等）工作正常。

2. **性能表现**：
   - 框架开销极低，单次视图操作在 0.02-0.04ms 之间
   - GC 分配控制在每操作 70-140 字节，适合对 GC 敏感的游戏项目
   - 线性扩展良好，中间件数量增加对性能影响可控

3. **强类型上下文**：`ITypedPipelineContext` 提供零装箱的键值存储，单次读写约 0.03ms，GC 分配约 85 字节，满足高性能场景需求。

4. **扩展包验证**：通过 `IValidatable` 接口，扩展包可在构建时进行前置条件检查，验证失败时阻止构建并提供清晰的错误信息。

5. **并发安全**：并行操作测试通过，框架能正确处理多视图同时打开/关闭的场景

6. **资源管理**：异步释放机制工作正常，释放后正确阻止后续操作


## 版本信息

当前版本：1.0.1-beta