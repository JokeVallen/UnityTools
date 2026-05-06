> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

# 测试报告

## 测试环境

| 项目 | 值 |
|------|-----|
| **Unity 版本** | 2020.3.48f1 |
| **C# 版本** | 7.0 |
| **目标框架** | .NET Standard 2.0 |
| **操作系统** | Windows 11 |
| **测试框架** | Unity TestFramework 1.1.33 |
| **测试类型** | 单元测试 |

## 测试类型

### 单元测试

#### 测试用例及说明

##### GetHashCode<T>
| 编号 | 用例 | 说明 |
|------|------|------|
| 1 | `GetHashCode_SameValue_ReturnsSameHash` | 相同输入返回相同哈希 |
| 2 | `GetHashCode_NullString_ReturnsStableHash` | null 引用返回可预期的哈希（种子*31） |
| 3 | `GetHashCode_DefaultValueType_ReturnsStableHash` | 值类型默认值保持稳定 |
| 4 | `GetHashCode_DifferentValues_UnlikelyEqual` | 不同值产生不同哈希 |

##### Combine<T1,T2> ... Combine<T1,...,T5>
| 编号 | 用例 | 说明 |
|------|------|------|
| 5 | `Combine_TwoArgs_SameValues_SameHash` | 相同参数顺序返回相同哈希 |
| 6 | `Combine_TwoArgs_OrderMatters` | 改变顺序产生不同哈希 |
| 7 | `Combine_TwoArgs_AllNull_ReturnsPredictableHash` | 全 null 输入有可计算预期值 |
| 8 | `Combine_ThreeArgs_AllNull_DoesNotThrow` | 多 null 参数不抛异常 |
| 9 | `Combine_FourArgs_MixedTypes_Works` | 混合类型合并稳定 |
| 10 | `Combine_FiveArgs_OrderSensitive` | 五参数顺序敏感 |
| 11 | `Combine_FiveArgs_AllNullRefTypes` | 全 null 引用类型产生确定哈希 |

##### CombineAll<T> (泛型)
| 编号 | 用例 | 说明 |
|------|------|------|
| 12 | `CombineAll_NullArray_ReturnsZero` | null 数组返回 0 |
| 13 | `CombineAll_EmptyArray_ReturnsSeed` | 空数组返回种子 17 |
| 14 | `CombineAll_SingleElement_MatchesGetHashCode` | 单元素与 GetHashCode 一致 |
| 15 | `CombineAll_MultipleElements_OrderMatters` | 多元素顺序敏感 |
| 16 | `CombineAll_ParamsWithValueTypes_Works` | 值类型元素稳定 |
| 17 | `CombineAll_ParamsWithNullElements_HandledGracefully` | 含 null 元素不抛异常 |

##### CombineAll (非泛型)
| 编号 | 用例 | 说明 |
|------|------|------|
| 18 | `CombineAll_NonGeneric_NullArray_ReturnsZero` | null 数组返回 0 |
| 19 | `CombineAll_NonGeneric_EmptyArray_ReturnsSeed` | 空数组返回 17 |
| 20 | `CombineAll_NonGeneric_MixedTypes_Stable` | 混合类型稳定 |
| 21 | `CombineAll_NonGeneric_BoxedValueTypeHashDiffersFromGeneric` | 文档性测试：值类型装箱哈希可能与泛型版不同 |

##### GetOrderDependentHashCode<T> (数组)
| 编号 | 用例 | 说明 |
|------|------|------|
| 22 | `GetOrderDependentHashCode_Array_Null_ReturnsZero` | null 数组返回 0 |
| 23 | `GetOrderDependentHashCode_Array_Empty_ReturnsSeed` | 空数组返回种子 |
| 24 | `GetOrderDependentHashCode_Array_SingleElement_MatchesGetHashCode` | 单元素与 GetHashCode 一致 |
| 25 | `GetOrderDependentHashCode_Array_OrderMatters` | 顺序不同哈希不同 |
| 26 | `GetOrderDependentHashCode_Array_WithNullElements_Handled` | 数组含 null 不抛异常 |

##### GetOrderDependentHashCode<T> (数组 + 自定义比较器)
| 编号 | 用例 | 说明 |
|------|------|------|
| 27 | `GetOrderDependentHashCode_Array_NullComparer_UsesDefault` | comparer 为 null 回退默认 |
| 28 | `GetOrderDependentHashCode_Array_CustomComparer_ChangesHash` | 不区分大小写比较器影响哈希 |
| 29 | `GetOrderDependentHashCode_Array_CustomComparer_OrderStillMatters` | 自定义比较器下顺序仍敏感 |

##### GetOrderDependentHashCode<T> (IEnumerable)
| 编号 | 用例 | 说明 |
|------|------|------|
| 30 | `GetOrderDependentHashCode_IEnumerable_Null_ReturnsZero` | null 序列返回 0 |
| 31 | `GetOrderDependentHashCode_IEnumerable_Empty_ReturnsSeed` | 空序列返回种子 |
| 32 | `GetOrderDependentHashCode_IEnumerable_OrderMatters` | 顺序改变哈希变 |
| 33 | `GetOrderDependentHashCode_IEnumerable_CustomComparer` | 自定义比较器有效 |
| 34 | `GetOrderDependentHashCode_IEnumerable_StableAcrossEnumerations` | 多次遍历结果稳定 |

##### 边界与极端值
| 编号 | 用例 | 说明 |
|------|------|------|
| 35 | `Combine_WithIntMaxValues_NoOverflowException` | int.MaxValue 不抛溢出异常 |
| 36 | `CombineAll_WithLargeParams_NoOverflowException` | 大数组合并不溢出 |
| 37 | `GetOrderDependentHashCode_Array_ExtremeValues` | 极值数组正常计算 |

##### 内部常量验证
| 编号 | 用例 | 说明 |
|------|------|------|
| 38 | `SeedAndMultiplier_ProduceKnownResult` | 验证种子和乘数的数学计算结果 |
| 39 | `Combine_TwoDifferentTypes_IntAndString` | 不同类型参数组合正常 |
| 40 | `Combine_FourArgs_MixedNulls` | 混合 null 和非 null 不崩溃 |

#### 测试结果

**所有测试用例均通过。**