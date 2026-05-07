> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

# 📋 MonoSingleton 测试报告

## 1. 测试环境

| 项目 | 配置 |
|------|------|
| Unity 版本 | 2020.3.48f1 |
| Unity Test Framework | 1.1.33 |
| 脚本后端 | Mono / IL2CPP |
| C# 语言版本 | 7.0 |
| .NET 标准 | .NET Standard 2.0 |
| 操作系统 | Windows 11 |
| 测试模式 | PlayMode（运行模式） |

## 2. 测试类型

- **单元测试**：覆盖所有单例基类（非持久化、持久化、接口变体）的核心行为，包括实例创建、唯一性校验、销毁清理、持久化生存周期以及接口封装验证。

## 3. 测试用例及说明

| 测试用例 | 说明 |
|----------|------|
| `Instance_IsNull_BeforeCreation` | 验证在未创建任何组件时，静态实例为 null。 |
| `Instance_IsNotNull_AfterAddingComponent` | 验证 `AddComponent` 后实例正确赋值。 |
| `DuplicateComponent_IsDestroyed` | 添加第二个相同组件时，重复组件被销毁，第一个实例保留。 |
| `Instance_SetToNull_OnDestroy` | 销毁组件后，静态实例立即置空。 |
| `Persistant_Object_Survives_SceneLoad` | 持久化单例在等待一帧后仍然存活（模拟场景切换）。 |
| `Persistant_WithInterface_WorksCorrectly` | 持久化接口单例实例不为空，调用接口方法有效。 |
| `InterfaceVariant_HidesConcreteType` | 接口变体返回的实例类型为 `I`，编译时隐藏具体实现。 |
| `BaseAwake_Called_AfterOverride` | 重写 `Awake` 后基类逻辑仍被执行（`initialized` 为 true）。 |

## 4. 测试结果

所有 8 个测试用例**全部通过**。

```
测试运行摘要：
  总测试数: 8
  通过: 8
  失败: 0
  跳过: 0
```

详细断言验证均符合预期：

- `DuplicateComponent_IsDestroyed` 在等待一帧后第二个组件为 null。
- 各静态实例在 `TearDown` 使用 `DestroyImmediate` 后完全重置，测试间无干扰。
- 持久化对象在 `DontDestroyOnLoad` 后保持存活且非空。

## 5. 备注

- 测试代码位于 `Assets/Tests/Scripts/PlayModeTests/MonoSingletonTests.cs`。
- 为确保测试独立性，每个测试用例的清理阶段均使用 `Object.DestroyImmediate` 强制清理所有残留单例对象。
- 所有测试依赖 Unity Test Framework 和 NUnit，运行时可选择 PlayMode 选项卡执行。