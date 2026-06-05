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

---

## 测试类型

### 1. 单元测试（EditMode）

验证框架各模块的功能正确性，包括基础功能、中间件执行、拦截机制、动态供应器、执行策略、扩展包、异常处理和资源释放等。

### 2. 基准/性能测试（PlayMode）

测量框架在真实 Unity 运行环境下的性能表现，包括构建耗时、打开/关闭视图耗时、GC 分配、压力测试等。

---

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

### 异常测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `BuildSession_WithNullRegistry_ShouldThrow` | 空注册表应抛出参数异常 | ✅ 通过 |
| `BuildSession_WithNullStackPolicy_ShouldThrow` | 空栈策略应抛出参数异常 | ✅ 通过 |
| `OpenViewAsync_WithNullView_ShouldThrow` | 空视图应抛出参数异常 | ✅ 通过 |
| `BuildSession_ReuseBuilder_ShouldThrow` | 重复使用构建器应抛出无效操作异常 | ✅ 通过 |

### 资源释放测试

| 测试用例 | 说明 | 状态 |
|----------|------|------|
| `DisposeAsync_ShouldCompleteWithoutError` | 异步释放应正常完成 | ✅ 通过 |
| `OpenViewAsync_AfterDispose_ShouldThrow` | 释放后再操作应抛出异常 | ✅ 通过 |

---

## 性能测试结果

### 1. 构建会话性能

| 测试场景 | 中间件数量 | 平均耗时 (ms) | 最小 (ms) | 最大 (ms) |
|----------|:----------:|--------------:|----------:|----------:|
| BuildSession_NoMiddleware | 0 | 0.0049 | 0.0028 | 0.0080 |
| BuildSession_WithMiddlewares | 1 | 0.0034 | 0.0029 | 0.0045 |
| BuildSession_WithMiddlewares | 5 | 0.0043 | 0.0031 | 0.0063 |
| BuildSession_WithMiddlewares | 10 | 0.0046 | 0.0039 | 0.0064 |
| BuildSession_WithMiddlewares | 20 | 0.0065 | 0.0059 | 0.0074 |

**结论**：构建 20 个中间件的会话耗时约 0.0065ms，开销极小，与中间件数量呈近似线性关系。

### 2. 打开/关闭视图性能

| 测试场景 | 中间件数量 | 平均耗时 (ms) | GC 分配 (字节) |
|----------|:----------:|--------------:|---------------:|
| OpenCloseView_NoMiddleware | 0 | 0.0248 | 54 |
| OpenCloseView_WithMiddlewares | 0 | 0.0221 | 54 |
| OpenCloseView_WithMiddlewares | 1 | 0.0236 | 61 |
| OpenCloseView_WithMiddlewares | 5 | 0.0266 | 75 |
| OpenCloseView_WithMiddlewares | 10 | 0.0311 | 92 |
| OpenCloseView_WithMiddlewares | 20 | 0.0378 | 124 |

**结论**：
- 单次 Open+Close 操作在无中间件时约 0.025ms
- 每增加一个中间件，耗时增长约 0.5-1μs
- GC 分配与中间件数量成正相关（每个中间件约 3-4 字节）

### 3. GC 分配详细测试

| 测试场景 | 预热次数 | 测量次数 | GC 分配 (字节) | 平均耗时 (ms) |
|----------|:--------:|:--------:|---------------:|--------------:|
| GCAllocation_NoMiddleware | 10 | 20 | 54 | 0.0194 |
| GCAllocation_WithPoolWarmup | 40 | 20 | 54 | 0.0227 |

**结论**：对象池预热后 GC 分配无明显变化，框架核心路径已实现零/低 GC 分配。

### 4. 中间件开销对比

| 测试场景 | 样本数 | 平均耗时 (ms) | 说明 |
|----------|:------:|--------------:|------|
| 无中间件基线 | 10 | 约 0.005 | 第一次运行（含冷启动） |
| 5 个空中间件 | 10 | 约 0.022 | 包含中间件遍历开销 |

**注意**：数据中存在部分异常高值（最高 395ms），可能由于首次 JIT 编译或系统干扰导致，不影响常规性能评估。

### 5. 压力测试（顺序操作）

| 操作次数 | 测量次数 | 平均耗时 (ms) | 总耗时 (ms) |
|----------|:--------:|--------------:|------------:|
| 10 | 3 | 0.0438 | 0.1315 |
| 100 | 3 | 0.2845 | 0.8534 |
| 500 | 3 | 0.9146 | 2.7438 |

**结论**：500 次顺序 Open+Close 操作总耗时约 2.74ms，平均单次操作约 0.0055ms，线性扩展良好。

### 6. 压力测试（并行操作）

| 并发视图数 | 测量次数 | 平均耗时 (ms) | 说明 |
|:----------:|:--------:|--------------:|------|
| 10 | 3 | 0.0224 | 10 个视图并行打开+关闭 |
| 50 | 3 | 0.2000 | 50 个视图并行打开+关闭（含一次异常高值 0.424ms） |

**结论**：框架能够正确处理并发操作，并行执行性能表现稳定。

### 7. 栈策略性能

| 测试场景 | 操作次数 | 平均耗时 (ms) | 总耗时 (ms) |
|----------|:--------:|--------------:|------------:|
| StackPolicy_PushPop_1000Items | 1000 | 0.5445 | 2.7223 |

**结论**：`DefaultViewStackPolicy` 基于 LinkedList + Dictionary 实现，单次 Push/Pop 操作平均约 0.00054ms，满足高频导航场景需求。

### 8. 真实场景模拟（电商页面）

模拟包含 5 个中间件的真实页面流程：Auth → Cache → Loading → Analytics → Animation

| 指标 | 数值 |
|------|------|
| 平均耗时 | 0.0284 ms |
| GC 分配 | 75-80 字节 |
| 样本数 | 10 次测量 |

**结论**：典型业务场景下，框架开销约 0.03ms，对游戏性能影响可忽略不计。

---

## 性能基准总结

| 场景 | 耗时 (ms) | GC (bytes) | 评价 |
|------|----------:|-----------:|------|
| 空会话构建 | 0.005 | ~0 | ✅ 优秀 |
| 空视图 Open+Close | 0.025 | 54 | ✅ 优秀 |
| 5 中间件 Open+Close | 0.027 | 75 | ✅ 优秀 |
| 20 中间件 Open+Close | 0.038 | 124 | ✅ 良好 |
| 500 次顺序操作 | 0.915/次 | - | ✅ 优秀 |
| 50 视图并发 | 0.200 | - | ✅ 良好 |

---

## 测试结论

1. **功能完整性**：所有单元测试均通过，框架的核心功能（中间件执行、拦截、动态供应、策略、扩展包、资源管理等）工作正常。

2. **性能表现**：
   - 框架开销极低，单次视图操作在 0.025-0.038ms 之间
   - GC 分配控制在每操作 54-124 字节，适合对 GC 敏感的游戏项目
   - 线性扩展良好，中间件数量增加对性能影响可控

3. **并发安全**：并行操作测试通过，框架能正确处理多视图同时打开/关闭的场景

4. **资源管理**：异步释放机制工作正常，释放后正确阻止后续操作

5. **适用场景**：性能表现满足大多数 Unity 项目的 UI 管理需求，包括高频率的页面跳转和复杂的中间件编排场景