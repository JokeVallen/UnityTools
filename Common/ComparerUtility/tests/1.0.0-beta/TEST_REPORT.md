> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

# 测试报告 - ComparerUtility

## 测试环境

| 项目 | 说明 |
|---|---|
| **测试框架** | Unity TestFramework 1.1.33 (NUnit) |
| **Unity 版本** | 2020.3.48f1 |
| **脚本运行时** | .NET 4.x Equivalent |
| **C# 版本** | 7.0 |
| **测试模式** | EditMode |
| **测试平台** | Windows / macOS |

## 测试类型

- **单元测试**：对 `ComparerUtility` 的所有公开 API 进行功能、异常、线程安全及边界验证。

---

### 单元测试

#### EqualityComparer 测试

| 测试用例 | 说明 | 结果 |
|:---|:---|:---:|
| `GetEqualityComparer_NoCustom_ReturnsDefault` | 无自定义设置时，泛型获取返回 `EqualityComparer<T>.Default` | √ |
| `GetEqualityComparer_NonGeneric_NoCustom_ReturnsDefault` | 无自定义设置时，非泛型获取返回默认相等性比较器 | √ |
| `SetEqualityComparer_Generic_AndGet_ReturnsSameInstance` | 泛型设置后，泛型获取应返回同一实例 | √ |
| `SetEqualityComparer_Generic_OnlyNonGeneric_AndGet_ReturnsSameInstance` | 设置仅实现非泛型接口的比较器后，非泛型获取返回原对象，泛型获取返回有效适配器且行为正确 | √ |
| `SetEqualityComparer_Generic_OnlyGenericInterface_AndGet_ReturnsOriginal` | 设置仅实现泛型接口的比较器后，泛型获取返回原始实例 | √ |
| `GetEqualityComparer_NonGeneric_AfterGenericSet_ReturnsOriginalNonGeneric` | 设置同时实现两个接口的比较器后，非泛型获取返回原对象 | √ |
| `TryRemoveEqualityComparer_RemovesCustomAndDefaultCache` | 移除存在自定义缓存的类型后返回 true；再次移除返回 false | √ |
| `TryRemoveEqualityComparer_RemovesDefaultCacheIfExists` | 先触发默认缓存，移除成功，再次获取会重新创建默认缓存 | √ |
| `ClearEqualityComparers_ClearsAll` | 清空后，获取回到默认行为，重新创建默认比较器 | √ |
| `GetEqualityComparer_UnsupportedType_ThrowsInvalidOperationException` | 对指针等不支持类型抛出 `InvalidOperationException` | √ |
| `SetEqualityComparer_NullArguments_ThrowsArgumentNullException` | 泛型和非泛型 Set 传入 null 时抛出 `ArgumentNullException` | √ |
| `GetEqualityComparer_NullType_ThrowsArgumentNullException` | 非泛型 Get 或 TryRemove 传入 null Type 时抛出 `ArgumentNullException` | √ |
| `EqualityComparer_Adapter_PreservesCorrectness` | 仅实现非泛型接口的比较器，经泛型 Get 适配后比较逻辑正确 | √ |

#### Comparer 测试

| 测试用例 | 说明 | 结果 |
|:---|:---|:---:|
| `GetComparer_NoCustom_ReturnsDefault` | 无自定义设置时，泛型获取返回 `Comparer<T>.Default` | √ |
| `SetComparer_Generic_AndGet_ReturnsSameInstance` | 泛型设置后，泛型获取返回同一实例 | √ |
| `SetComparer_NonGeneric_GenericGet_Works` | 设置非泛型比较器后，泛型获取返回可用适配器，比较结果正确 | √ |
| `TryRemoveComparer_ClearsCache` | 移除成功后返回 true，二次移除返回 false | √ |
| `ClearComparers_ClearsAll` | 清空后恢复为默认比较器 | √ |
| `SetComparer_NullArguments_ThrowsArgumentNullException` | 设置时传入 null 抛出 `ArgumentNullException` | √ |
| `GetComparer_NullType_ThrowsArgumentNullException` | 获取或移除时传入 null Type 抛出 `ArgumentNullException` | √ |
| `GetComparer_UnsupportedType_ThrowsInvalidOperationException` | 不支持类型抛出 `InvalidOperationException` | √ |

#### 线程安全测试

| 测试用例 | 说明 | 结果 |
|:---|:---|:---:|
| `ConcurrentAccess_EqualityComparer_DoesNotThrow` | 4 个任务并发获取、设置、移除相等性比较器，无异常抛出 | √ |
| `ConcurrentAccess_Comparer_DoesNotThrow` | 4 个任务并发获取、设置、移除比较器，无异常抛出 | √ |

> 所有测试均已通过。