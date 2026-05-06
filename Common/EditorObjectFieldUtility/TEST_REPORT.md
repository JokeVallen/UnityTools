> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

# EditorObjectFieldUtility 测试报告

## 测试环境

- **Unity 版本**：2020.3 及以上（兼容 NUnit 框架的 Editor Test）
- **测试框架**：Unity Test Framework（基于 NUnit）
- **测试运行模式**：Editor Mode

## 测试类型

本库的测试均为 **单元测试**，包含以下两类：
1. **参数校验测试**：验证方法在接收到非法参数（`null`类型、非 `UnityEngine.Object` 类型）时能正确抛出异常。
2. **类型过滤测试**：验证当传入的实际对象类型与要求的类型不匹配时，返回值会被清空为 `null`。

## 测试用例及说明

### 1. 参数校验

| 测试方法 | 测试说明 | 预期结果 |
|----------|----------|----------|
| `NoPickerObjectField_WithNullType_ThrowsArgumentNullException` | 调用 `NoPickerObjectField`（非泛型版本）并传入 `Type = null` | 抛出 `ArgumentNullException` |
| `NoPickerObjectField_WithNonUnityObjectType_ThrowsArgumentException` | 调用 `NoPickerObjectField` 并传入非 `UnityEngine.Object` 类型（例如 `typeof(string)`） | 抛出 `ArgumentException` |
| `ObjectField_WithNullType_ThrowsArgumentNullException` | 调用 `ObjectField`（非泛型版本）并传入 `Type = null` | 抛出 `ArgumentNullException` |
| `ObjectField_WithNonUnityObjectType_ThrowsArgumentException` | 调用 `ObjectField` 并传入非 `UnityEngine.Object` 类型（例如 `typeof(int)`） | 抛出 `ArgumentException` |

### 2. 类型过滤

| 测试方法 | 测试说明 | 预期结果 |
|----------|----------|----------|
| `NoPickerObjectField_MismatchedType_ReturnsNull` | 传入一个 `GameObject` 实例，但要求类型为 `Texture2D` | 返回值为 `null`，且不会引发异常 |
| `ObjectField_MismatchedType_ReturnsNull` | 传入一个自定义 `ScriptableObject` 实例，但要求类型为 `Texture2D` | 返回值为 `null`，且不会引发异常 |
| `NoPickerObjectField_Generic_WithCorrectType_DoesNotThrow` | 泛型版本，传入正确类型的值（`GameObject`） | 正常返回原值，不抛出异常 |
| `ObjectField_Generic_WithCorrectType_DoesNotThrow` | 泛型版本，传入正确类型的值（`Texture2D.whiteTexture`） | 正常返回原值，不抛出异常 |

## 测试结果

所有列出的测试用例均已通过。

- **参数校验**：在非法参数情况下均正确抛出指定异常。
- **类型过滤**：类型不匹配时安全返回 `null`，无异常；类型匹配时返回原对象。