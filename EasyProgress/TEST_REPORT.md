> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

## 测试环境

- **操作系统**：Windows 11 (10.0.26100.1742/24H2/2024Update/HudsonValley)
- **处理器**：Intel Core i7-14650HX 2.20GHz, 24 logical cores
- **.NET SDK**：9.0.304
- **运行时**：.NET 7.0.20
- **测试框架**：xUnit 2.4.2
- **基准测试框架**：BenchmarkDotNet 0.15.8

## 测试类型

### 1. 单元测试

使用 xUnit 对核心功能进行验证，覆盖正常路径、边界条件和并发场景。

#### 测试用例及说明

| 测试类 | 测试方法 | 说明 |
|--------|----------|------|
| `DefaultLeafTests` | `Report_ShouldUpdateProgressAndTriggerEvent` | 验证报告进度更新值和触发事件 |
| | `Report_ShouldClampValueBetween0And1` | 验证进度值自动限制在 [0,1] |
| | `Report_ShouldIgnoreIdenticalValueWithinTolerance` | 验证相同进度值（容差内）不重复触发 |
| | `Complete_ShouldSetProgressToOne` | 验证 `Complete` 将进度设为 1 |
| | `Reset_ShouldClearProgressAndEventHandlers` | 验证重置状态 |
| | `ConcurrentReport_ShouldBeThreadSafe` | 多线程并发报告安全性 |
| `RealtimeCompositeTests` | `AddChild_ShouldRecalcProgress` | 添加子节点后立即重算总进度 |
| | `RemoveChild_ShouldRecalcProgress` | 移除子节点后重算 |
| | `ChildProgressChange_ShouldTriggerRecalcAndEvent` | 子节点进度变化触发重算和事件 |
| | `SetRule_ShouldRecalcWithNewRule` | 更换规则后重算 |
| | `Progress_ShouldReturnLatestCachedValue` | 缓存值正确 |
| | `MultipleChildren_ShouldUseEqualWeight` | 无权重时等权平均 |
| | `EmptyComposite_ProgressShouldBeZero` | 空组合节点进度为 0 |
| | `Reset_ShouldClearChildrenAndSetDefaultRule` | 重置节点 |
| `ManualCompositeTests` | `AddChild_ShouldMarkDirtyButNotRecalc` | 手动刷新模式下不自动重算 |
| | `Refresh_ShouldRecalcAndTriggerEventOnce` | 手动刷新触发重算和事件 |
| `WeightedRealtimeCompositeTests` | `AddChild_WithWeight_ShouldApplyWeightInAverage` | 带权重添加正确影响加权平均 |
| | `SetWeight_ShouldRecalc` | 修改权重后重算 |
| | `WeightedAverage_ShouldCalculateCorrectly` | 加权平均计算正确 |
| | `ZeroWeightChild_ShouldBeIgnored` | 权重为 0 的子节点被忽略 |
| `CompositionRuleTests` | `WeightedAverageRule_ShouldComputeAverage` | 加权平均规则正确 |
| | `SequentialRule_ShouldAccumulateSequentially` | 顺序规则正确累积 |
| | `MaxRule_ShouldReturnMaxProgress` | 最大值规则正确 |
| | `MinRule_ShouldReturnMinProgress` | 最小值规则正确 |
| `PooledNodeManagerTests` | `Acquire_WhenEmpty_CreatesNewNode` | 空池时创建新节点 |
| | `Acquire_AfterRelease_ReturnsReusedNode` | 释放后复用节点 |
| | `Release_CallsReset` | 归还时调用重置 |
| | `ConcurrentAcquireRelease_NoCorruption` | 并发获取/归还线程安全 |
| `ExtensionMethodTests` | `ReleaseLeafChildren_ReleasesDirectLeafChildrenAndRemovesThem` | 释放一级叶子节点并移除 |
| | `ReleaseTree_ReleasesAllDescendantsAndRemovesThem` | 递归释放整个子树 |
| | `BeginProgress_AddsLeafAndAutomaticallyRemovesOnDispose` | 作用域自动添加和清理叶子节点 |
| | `BeginComposite_AddsCompositeAndOnDisposeReleasesWholeSubtree` | 作用域自动管理组合节点及其子树 |
| | `RunWithProgress_ExecutesWorkAndCleansUp` | 委托模式自动管理临时节点 |
| | `AddChildren_AddsMultipleNodes` | 批量添加子节点 |
| `ConcurrentTests` | `MultipleThreads_ReportOnSameLeaf_ShouldNotCorrupt` | 多线程报告同一叶子节点不损坏 |
| | `MultipleThreads_AddRemoveChildren_ShouldMaintainConsistency` | 多线程动态增删子节点一致性 |
| | `PooledNodeManager_MultiThreaded_AcquireRelease` | 多线程池操作安全 |

#### 测试结果

所有单元测试均已通过（在容差 `1e-7` 下浮点比较通过）。无异常或失败。

### 2. 基准测试

使用 BenchmarkDotNet 对核心操作进行性能测量，包括延迟、内存分配和并发竞争。

#### 测试用例及说明

| 测试类 | 方法 | 说明 |
|--------|------|------|
| `LeafBenchmark` | `Report`, `Complete` | 叶子节点报告和完成操作 |
| `RealtimeCompositeBenchmark` | `UpdateOneChild`, `UpdateAllChildren` | 实时组合节点单子节点和全部子节点更新 |
| `WeightedRealtimeCompositeBenchmark` | `UpdateOneChild` | 加权实时组合节点单子节点更新 |
| `ManualCompositeBenchmark` | `UpdateOneChild_NoRefresh`, `UpdateOneChild_ThenRefresh` | 手动刷新节点仅标记脏和标记后刷新 |
| `DeepNestedCompositeBenchmark` | `UpdateDeepestLeaf` | 深度嵌套树更新最深叶子节点 |
| `ConcurrentUpdateBenchmark` | `ConcurrentUpdates` | 多线程同时更新组合节点 |
| `ExtremeRealtimeCompositeBenchmark` | `UpdateOneLeaf`, `UpdateAllLeaves` | 1000/10000 子节点下更新单个/全部叶子 |
| `ExtremeWeightedCompositeBenchmark` | `UpdateHeavyLeaf`, `UpdateAllLightLeaves` | 极端权重下更新重权重和轻叶子 |
| `PoolBenchmark` | `AcquireRelease` | 节点池的获取与释放操作 |
| `PoolContentionBenchmark` | `AcquireReleaseList`, `AcquireReleaseDict` | 多线程下列表池和字典池竞争 |
| `ProgressManagerBenchmark` | `AcquireLeaf`, `AcquireComposite` | 管理器获取叶子/组合节点的开销 |
| `ExtensionMethodBenchmarks` | `ReleaseLeafChildren`, `ReleaseTree`, `BeginProgress_Using`, `BeginComposite_Using`, `RunWithProgress`, `RunWithProgressAsync`, `AddChildren_Plain`, `AddChildren_Weighted` | 扩展方法的性能（池化版本） |
| `StandardUsageBenchmarks` | `LeafPool_AcquireReportRelease`, `LeafScope_OnPooledComposite`, `CompositeScope_OnPooledParent`, `ReleaseTree_FromPooledRoot`, `ReusedComposite_LeafScope` | 规范使用场景下的性能 |

#### 测试结果（摘要）

| 操作 | 平均耗时 | 内存分配 | 备注 |
|------|----------|----------|------|
| `DefaultLeaf.Report` | ~13 ns | 0 B | 稳定后零分配 |
| `RealtimeComposite` 更新单个子节点 | ~13 ns | 0 B | 与子节点数量无关 |
| `RealtimeComposite` 更新全部 1000 子节点 | ~13.8 µs | 0 B | 线性扩展，每子节点 ~13.8 ns |
| `WeightedRealtimeComposite` 更新单个子节点 | ~13 ns | 0 B | |
| `ManualComposite` 仅标记脏 | ~13 ns | 0 B | |
| `ManualComposite` 标记脏+刷新 | ~26 ns | 0 B | |
| 深度嵌套树（深度20）更新最深叶子 | ~13 ns | 0 B | 与深度无关 |
| 并发更新（16线程，5000子节点） | ~6.7 µs | ~2.9 KB | 锁竞争极低 |
| 叶子池化操作（获取→报告→释放） | 1.24 µs | 56 B | |
| `BeginProgress` 作用域 | 9.6 µs | 656 B | |
| `BeginComposite` 作用域 | 13.4 µs | 1.4 KB | |
| `ReleaseTree` 释放深度2的树 | 10.5 µs | 1.2 KB | |