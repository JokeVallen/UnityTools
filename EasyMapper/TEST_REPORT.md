> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

# EasyMapper 测试报告

## 测试环境

| 项目 | 详情 |
|------|------|
| **测试引擎** | Unity 2020.3.48f1 (Editor Mode) |
| **脚本后端** | Mono2x |
| **目标平台** | StandaloneWindows64 |
| **测试框架** | Unity Test Framework 1.1.33, Performance Testing API 3.0.3 |
| **硬件** | Intel i7-14650HX, 16GB RAM, NVIDIA RTX 4060 Laptop GPU |
| **操作系统** | Windows 11 64bit |
| **测试模式** | EditMode |

---

## 单元测试 (EditMode)

### 测试用例及说明

单元测试覆盖了框架所有默认组件、装饰器与组合逻辑，共 **40+** 个用例，全部通过。

| 模块 | 测试要点 | 状态 |
|------|----------|------|
| **LongToken / GuidToken** | 相等性、隐式转换、哈希一致性 | ✅ |
| **Char10PackingBlueprint** | null/空串返回0、最长字符串往返、大小写折叠、非法字符终止、超长截断、下划线和连字符编码 | ✅ |
| **InterningBlueprint** | 相同字符串返回相同Token、不同字符串不重复、Restore抛出异常、IsTraceable=false | ✅ |
| **SmartDistributor** | 短合法字符串走快速路径(bit63=0)、长字符串/非法字符走回退(bit63=1)、null返回0 | ✅ |
| **ObjectNamingBlueprint** | 基于对象名称生成Token、Restore抛出异常 | ✅ |
| **StandardPipeline** | 可溯源蓝图正确往返、非可溯源蓝图正确查字典、重复导入返回相同Token、Cleanup清空 | ✅ |
| **UnityWeakPipeline** | 活对象导入导出正常、销毁对象导出返回null、Cleanup移除死引用、null对象导入返回默认Token | ✅ |
| **ThreadSafePipeline** | 导入导出正确委托、Cleanup有效 | ✅ |
| **CacheFirstPipeline** | 缓存命中直接返回Token、导出委托内部流水线 | ✅ |
| **CappedPipeline** | 容量限制正确淘汰、LRU访问顺序准确性 | ✅ |
| **IdempotentPipeline** | 保证幂等性、Cleanup清除映射 | ✅ |
| **GuardedPipeline** | null源抛出异常、默认Token导出抛出异常 | ✅ |
| **DiagnosticPipeline** | 计数器正确递增、事件触发、重置计数器 | ✅ |
| **BinaryIdentityPackage / GuidBinaryPackage** | 序列化往返正确、输入不足返回默认值 | ✅ |
| **IDMap 静态 API** | 字符串分配和查找往返、对象分配和查找、Pack/Unpack、Contains、Current替换 | ✅ |

> *注：部分测试用例在早期版本中因 CappedPipeline 实现逻辑问题失败，已修复为独立存储模式后全部通过。*

---

## 基准测试 (EditMode, Performance)

### 测试用例及说明

基准测试测量核心路径在 1 万次 / 10 万次迭代下的耗时 (ms) 与 GC 分配 (byte)，使用 `Measure.Method` 与 `Performance` 特性，Warmup 3-5 次，Measurement 5-10 次。  
以下表格展示 **三次独立运行的中位数平均值**，反映典型性能。

| 测试方法 | 迭代量 | 中位耗时 (ms) | 单次耗时 (ns) | GC 分配（每次） | 说明 |
|----------|--------|---------------|--------------|----------------|------|
| `Char10Packing_Refine_ShortString` | 1万 | 3.56 | 356 | 0 (框架开销) | 短字符串编码 |
| `Char10Packing_Refine_MaxLenString` | 1万 | 4.89 | 489 | 0 | 最长10字符编码 |
| `Char10Packing_Restore` | 1万 | 3.93 | 393 | 每次构造 StringBuilder + string | 字符串还原，有自然分配 |
| `Interning_Refine_SameLongString` | 1万 | 0.98 | 98 | 0 | 字典命中的长字符串查找 |
| `Interning_Refine_UniqueStrings` | 1万 | 0.75 | 75 | 字典节点分配（极少量） | 全新字符串首次分配 |
| `SmartDistributor_FastPath` | 1万 | 4.40 | 440 | 0 | 自动路由到快速编码 |
| `SmartDistributor_Fallback` | 1万 | 1.28 | 128 | 0 | 自动路由到回退驻留 |
| `StandardPipeline_Import_ExistingString` | 10万 | 61.58 | 616 | 0 | 流水线重复导入已存在键 |
| `StandardPipeline_Import_UniqueStrings` | 10万 | 17.70 | 177 | 字典扩容分配 | 流水线批量导入新键 |
| `UnityWeakPipeline_Import_DestroyedObjects` | 1千 | 7.43 | 7,430 | ~8 KB 总量 | 包含 GameObject 创建销毁 |
| `BinaryPackage_WrapUnwrap` | 1万 | 1.01 | 101 | 每次 8 字节数组 | 序列化/反序列化组合 |
| `GuardedPipeline_NullCheck` | 1万 | 5.03 | 503 | 0 | null 检查开销 |
| `CappedPipeline_Overhead_FullCache` | 1万 | 10.88 | 1,088 | LRU 链表节点操作 | 5千容量满缓存 Import |

**关键结论**：
- 核心映射操作在 **100 ns ~ 1 μs** 之间，远低于帧预算要求。
- 快速路径和字典命中路径 **零额外堆分配**，对 GC 友好。
- 字符串还原 (`Restore`) 会产生标准字符串分配，符合预期。
- 弱引用对象管理开销合理，1千次创建销毁约 7.4 ms。

*以上数据均在 EditMode 下采集，避免 PlayMode 引擎开销干扰，真实运行时纯算法耗时与此一致。*