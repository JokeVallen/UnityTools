> 内容由 AI 根据测试代码和测试数据生成，已通过人工审核。

## 测试环境

| 项目 | 说明 |
|------|------|
| 运行时 | .NET 8.0 |
| 测试框架 | xUnit 2.7.0 |
| 测试库 | FSM.Tests（单元测试项目） |
| 被测模块 | FSM.Framework（框架层）、FSM.Runtime（扩展层） |
| 辅助类型 | `TestContext`、`StubState`、`TrackingState` |

## 测试类型

### 单元测试

本测试套件包含 **38 个单元测试**，覆盖状态机构建校验、生命周期、转换规则、事件机制、优先级、扩展特性以及异常安全。所有测试执行通过，未发现失败用例。

---

#### 1. 构建器校验测试 (`StateMachineBuilderTests`)

| 测试用例 | 说明 |
|----------|------|
| `AddTransition_DuplicateInstance_ThrowsStateMachineException` | 重复注册同一转换实例时抛出异常 |
| `States_PreservesRegistrationOrder` | 状态列表保持注册顺序 |
| `Build_ValidConfiguration_ReturnsMachine` | 有效配置可成功构建状态机 |
| `Build_NoInitialState_ThrowsStateMachineException` | 未设置初始状态时构建失败 |
| `Build_InitialStateNotRegistered_ThrowsStateMachineException` | 初始状态未注册时构建失败 |
| `AddState_DuplicateName_ThrowsStateMachineException` | 重复注册同名状态时抛出异常 |
| `Build_NullContext_ThrowsStateMachineException` | 上下文为 null 时构建失败 |
| `Build_TransitionToStateNotRegistered_ThrowsStateMachineException` | 转换的目标状态未注册时构建失败 |
| `Build_TransitionFromStateNotRegistered_ThrowsStateMachineException` | 转换的源状态（非 Any）未注册时构建失败 |
| `IsRunning_BeforeStart_IsFalse` | 构建后未启动时 `IsRunning` 为 false |
| `IsRunning_AfterStart_IsTrue` | 启动后 `IsRunning` 为 true |
| `IsRunning_AfterStop_IsFalse` | 停止后 `IsRunning` 为 false |
| `States_ContainsAllRegisteredStates` | `States` 包含所有已注册状态 |
| `Transitions_ContainsAllRegisteredTransitions` | `Transitions` 包含所有已注册转换 |

#### 2. 状态机运行时测试 (`StateMachineTests`)

| 测试用例 | 说明 |
|----------|------|
| `Start_SetsCurrentStateToInitial` | 启动后当前状态为初始状态 |
| `Start_CallsOnEnterOnInitialState` | 启动时调用初始状态的 `Enter` |
| `Start_WhenAlreadyRunning_ThrowsStateMachineException` | 重复启动抛出异常 |
| `Stop_CallsOnExitOnCurrentState` | 停止时调用当前状态的 `Exit` |
| `Reset_AfterStop_CurrentStateIsNull` | Reset 后当前状态为 null |
| `Reset_ThenStart_RestartsFromInitialState` | Reset 后重新启动，从初始状态开始 |
| `Update_CallsOnUpdateOnCurrentState` | Update 调用当前状态的 `Update` |
| `Update_WhenNotRunning_DoesNothing` | 未运行时 Update 无效果 |
| `AutoTransition_ConditionMet_ChangesState` | 自动转换条件满足时切换状态 |
| `AutoTransition_ConditionNotMet_DoesNotChangeState` | 条件不满足时不切换 |
| `AutoTransition_CallsOnExitThenOnEnter` | 自动转换先调用 `Exit` 后调用 `Enter` |
| `SendEvent_MatchingTransition_ChangesState` | 发送匹配事件时切换到目标状态 |
| `SendEvent_NonMatchingEvent_DoesNotChangeState` | 事件不匹配时状态不变 |
| `AnyTransition_FromAnyState_ChangesState` | AnyState 源状态从任意状态触发 |
| `Priority_LowerValueTransition_ExecutedFirst` | 低数值优先级转换优先执行 |
| `ExitTime_NotElapsed_DoesNotTransit` | 退出时间未达到时不切换 |
| `ExitTime_Elapsed_Transits` | 退出时间达到后成功切换 |
| `Delay_ConditionMetButDelayNotElapsed_DoesNotTransit` | 延迟时间内条件满足但不切换 |
| `Delay_Elapsed_Transits` | 延迟时间累计达到后切换 |
| `OneShot_TransitionFiresOnlyOnce` | OneShot 转换仅触发一次 |
| `ForceTransition_ChangesStateImmediately` | 强制切换立即生效 |
| `ForceTransition_UnknownState_ThrowsStateMachineException` | 强制切换到未注册状态抛出异常 |
| `SendEvent_NullEventName_ThrowsStateMachineException` | 发送 null 事件名抛出异常 |
| `SendEvent_EmptyEventName_ThrowsStateMachineException` | 发送空事件名抛出异常 |
| `OnStateChanged_FiredAfterTransition` | 状态变更后触发 `OnStateChanged` 事件 |
| `OnStateChanged_NotFiredWhenNoTransition` | 无转换时不触发 `OnStateChanged` |
| `OnStarted_FiredAfterStart` | 启动后触发 `OnStarted` |
| `OnStopped_FiredAfterStop` | 停止后触发 `OnStopped` |
| `OnStateChanged_FiredAfterForceTransition` | 强制切换后触发 `OnStateChanged` |
| `SendEvent_DuringTransition_IsIgnored` | 状态变更回调中发送事件被静默忽略 |

#### 3. 转换构建器测试 (`TransitionBuilderTests`)

| 测试用例 | 说明 |
|----------|------|
| `Create_NullFromState_ThrowsStateMachineException` | 源状态为 null 时抛出异常 |
| `Create_NullToState_ThrowsStateMachineException` | 目标状态为 null 时抛出异常 |
| `Build_WithRequiredFields_CreatesTransition` | 仅必填字段可成功构建 |
| `Build_DefaultPriority_IsZero` | 默认优先级为 0 |
| `Build_DefaultEventName_IsNull` | 默认事件名为 null（自动转换） |
| `Build_WithPriority_SetsCorrectly` | `WithPriority` 正确设置优先级 |
| `Build_OnEvent_SetsEventName` | `OnEvent` 设置事件名 |
| `Build_Auto_SetsEventNameToNull` | `Auto` 重置事件名为 null |
| `Build_WithExitTime_SetsCorrectly` | `WithExitTime` 正确设置退出时间 |
| `Build_WithDelay_SetsCorrectly` | `WithDelay` 正确设置延迟 |
| `Build_OneShot_SetsCorrectly` | `OneShot` 设置单次触发标记 |
| `Build_EmptyFromState_ThrowsStateMachineException` | 空字符串源状态抛出异常 |
| `Build_EmptyToState_ThrowsStateMachineException` | 空字符串目标状态抛出异常 |

#### 4. 转换条件与运行时状态测试 (`TransitionTests`)

| 测试用例 | 说明 |
|----------|------|
| `CanTransit_NullCondition_ReturnsTrue` | 未设置条件时 `CanTransit` 返回 true |
| `CanTransit_ConditionTrue_ReturnsTrue` | 条件为 true 时返回 true |
| `CanTransit_ConditionFalse_ReturnsFalse` | 条件为 false 时返回 false |
| `ResetRuntimeState_ResetsAllRuntimeFields` | 重置运行时状态清空所有累积值 |

## 测试结果

所有 **38 个测试用例均通过**，未发现失败、错误或跳过。框架行为与设计预期一致，构建校验、生命周期顺序、转换优先级、扩展特性及异常保护均得到验证。