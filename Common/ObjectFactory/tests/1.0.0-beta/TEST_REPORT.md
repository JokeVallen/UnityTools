> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

# 测试报告

## 测试环境

| 项目 | 详情 |
|------|------|
| **Unity 版本** | 2020.3.48f1 |
| **C# 语言版本** | 7.0 |
| **目标框架** | .NET Standard 2.0 |
| **操作系统** | Windows 11 |
| **测试框架** | Unity TestFramework 1.1.33 |

## 测试类型：单元测试

所有测试均以 **EditMode** 方式运行，无需启动 PlayMode。

### 测试覆盖说明

测试用例覆盖了 `ObjectFactory`（注册与解析）、`GameObjectFactory`（创建与错误处理）、`ComponentFactory`（创建与异常清理）的全部公开行为及边界情况。

### 测试用例及说明

#### 1. ObjectFactoryTests
| 用例名称 | 说明 |
|----------|------|
| RegisterCreator_NullCreator_ShouldLogErrorAndNotThrow | 传入 `null` 委托，验证输出错误日志但不抛出异常 |
| RegisterCreator_ValidCreator_ShouldRegisterSuccessfully | 注册有效工厂，验证获取的实例确为注册的实例 |
| RegisterCreator_OverwriteExistingRegistration_ShouldUseLatest | 重复注册同一接口，验证最后注册的生效 |
| GetFactory_NotRegistered_ReturnsDefaultGameObjectFactory | 未注册时获取 `IGameObjectFactory`，应返回默认 `GameObjectFactory` 实例 |
| GetFactory_NotRegistered_ReturnsDefaultComponentFactory | 未注册时获取 `IComponentFactory`，应返回默认 `ComponentFactory` 实例 |
| GetFactory_CustomInterfaceWithoutDefault_ReturnsNull | 获取未注册且无默认实现的自定义接口，返回 `null` |
| GetFactory_TypeParameter_ReturnsSameAsGeneric | 非泛型 `GetFactory(Type)` 返回结果与泛型版本一致 |
| TryGetFactory_Registered_ReturnsTrueAndFactory | 已注册时 `TryGetFactory` 返回 `true` 并提供实例 |
| TryGetFactory_NoRegistrationOrDefault_ReturnsFalse | 无注册且无默认时返回 `false`，`factory` 为 `null` |
| TryGetFactory_DefaultAvailable_ReturnsTrue | 有默认实现时返回 `true` |
| TryGetFactory_NonGeneric_WorksCorrectly | 非泛型 `TryGetFactory(Type, out)` 正常工作 |
| ClearCreators_AfterRegister_ShouldFallBackToDefault | 清除注册后，获取工厂回到默认实现 |

#### 2. GameObjectFactoryTests
| 用例名称 | 说明 |
|----------|------|
| Create_Default_ReturnsNotNull | 创建默认对象不为空 |
| Create_WithName_ReturnsObjectWithCorrectName | 创建指定名称的对象，名称正确 |
| Create_WithComponents_AddsAllComponents | 创建时添加多个组件，均成功挂载 |
| Create_WithInitializeCallback_ExecutesCallback | 初始化回调被正确执行 |
| Create_InvalidComponentType_ReturnsNullAndLogsError | 传入非法组件类型（如 `string`），返回 `null` 并记录错误 |
| Create_NullComponentInArray_ReturnsNullAndLogsError | 组件数组包含 `null`，返回 `null` 并记录错误 |
| Create_ThrowOnErrorFalse_InitializeThrows_ReturnsNullAndDestroysObject | 回调异常且 `ThrowOnError = false`，对象被销毁，方法返回 `null` |
| Create_ThrowOnErrorTrue_InitializeThrows_RethrowsException | 回调异常且 `ThrowOnError = true`，异常被重新抛出 |
| Create_ThrowOnErrorFalse_LogsErrorOnFailure | 验证错误日志输出 |
| Create_NameAndComponentsOnly_Works | 提供名称和组件类型，初始化回调为 `null`，创建成功 |
| Create_EmptyName_StillCreatesObject | 空字符串名称仍创建对象，且有默认名称 |

#### 3. ComponentFactoryTests
| 用例名称 | 说明 |
|----------|------|
| CreateT_ValidType_ReturnsComponent | 泛型创建 `Rigidbody` 成功 |
| CreateT_WithInitialize_SetsValues | 泛型创建并通过回调设置质量成功 |
| CreateT_NullGameObject_ReturnsNullAndLogsError | `gameObject` 为 `null`，返回 `null` 并记录错误 |
| Create_ValidType_ReturnsComponent | 非泛型创建组件成功 |
| Create_InvalidTypeNotComponent_ReturnsNullAndLogsError | 传入非 `Component` 类型返回 `null` |
| Create_NullType_ReturnsNullAndLogsError | `type` 为 `null` 返回 `null` |
| Create_NullGameObject_ReturnsNullAndLogsError | `gameObject` 为 `null` 返回 `null` |
| Create_ThrowOnErrorFalse_InitializeFails_ReturnsNullAndDestroysComponent | 泛型创建，回调异常，组件被移除，返回 `null` |
| Create_ThrowOnErrorTrue_InitializeFails_Rethrows | 泛型创建，回调异常且 `ThrowOnError = true`，异常重新抛出 |
| Create_NonGeneric_ThrowOnErrorFalse_CleansUp | 非泛型创建，回调异常，组件被移除 |
| Create_ThrowOnErrorFalse_LogsError | 验证错误日志输出 |

### 测试结果

所有测试用例均**通过**，无失败项。

- **ObjectFactoryTests**: 12/12 通过
- **GameObjectFactoryTests**: 11/11 通过
- **ComponentFactoryTests**: 11/11 通过

> 测试过程中通过 `LogAssert.Expect` 正确验证了预期的错误日志输出，未产生非预期异常。测试覆盖了正常流程、边界值及异常恢复，当前版本稳定性良好。