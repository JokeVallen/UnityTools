> 内容由 AI 根据核心代码生成，已通过人工审核。

# FSM 状态机框架

![License](https://img.shields.io/badge/license-MIT-blue)
![.NET](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)

一个基于 .NET Standard 2.0 的通用有限状态机框架，采用接口与实现分离的架构设计，提供 Builder 模式的流式配置体验。框架聚焦最小正交原语，将高级特性（如分层状态机、装饰器）留给组合模式实现，兼具类型安全、高性能与可扩展性。适用于游戏 AI、UI 流程、业务逻辑等多种场景。

## 安装环境要求

- .NET Standard 2.0 兼容的运行环境（.NET Framework 4.6.1+、.NET Core 2.0+、.NET 5+、Unity 2018.3+ 等）
- 无第三方依赖，仅使用 `System` 基础库

## 安装方式

### 源码导入
将 `FSM.Framework` 和 `FSM.Runtime` 项目文件夹复制到你的解决方案中，添加项目引用即可。

### DLL 文件导入
编译两个项目生成 `FSM.Framework.dll` 与 `FSM.Runtime.dll`，在目标项目中添加引用。

## 设计理念

- **接口与实现分离**：`FSM.Framework` 仅包含核心接口 `IState`、`ITransition`、`IStateMachine`，不含任何实现逻辑，便于单元测试、自定义实现或依赖注入。
- **最小原语，组合至上**：框架不内置分层状态机、日志装饰器等“重型”功能，而是通过提供 `TKey` 泛型标识、`ForceTransition` 后门、事件回调等基础工具，让使用者通过组合模式（如将子状态机包装为状态、实现接口装饰器）自由构建所需能力。
- **类型安全标识**：状态标识支持泛型 `TKey`，可使用 `enum`、`int`、`string` 等任意类型，避免字符串拼写错误，同时为性能优化（如使用整型 Key 减少字典哈希开销）提供可能。
- **安全导向的 Builder**：转换与状态机的构建均通过内部 Builder 完成，在构建阶段进行完整性校验，并在非法配置时抛出统一的 `StateMachineException`。

## 具体功能说明

### 状态与生命周期
每个状态实现 `IState<TKey, TContext>` 接口（可继承 `StateBase` 或 `StateBehaviour`），拥有明确的 `Enter` / `Update` / `Exit` 三个生命周期阶段。状态机通过手动调用 `Update(deltaTime)` 驱动，顺序为：当前状态 `Update` → 评估自动转换。

### 转换规则配置
转换由 `Transition<TContext>.Builder` 流式创建，支持两种触发模式：
- **自动转换**：每帧自动评估条件，满足即切换。
- **事件转换**：仅当调用 `SendEvent(eventName)` 时才评估。

多条转换同时满足时，按 `Priority` 数值升序（越小越优先）执行，同优先级按注册顺序。

### 高级时间控制
- **退出时间（ExitTime）**：状态必须运行满指定时长后，该转换才被评估，常用于动作前摇/后摇保护。
- **转换延迟（Delay）**：条件满足后需稳定持续一段时间才执行切换，防止短暂条件变化引起的抖动。
- **单次触发（OneShot）**：整个生命周期内转换只允许触发一次，`Reset` 后重置。

### 全局转换与强制跳转
- 通过 `InnerStates.AnyState` 作为源状态，可定义从任意状态触发的转换（如死亡、暂停），并可通过优先级与其他转换协调。
- `ForceTransition` 方法可无视任何条件直接切换到指定状态，适用于剧情打断、外部控制等需求。

### 分层与装饰器
- **分层状态机**：将子状态机包装为一个 `IState`，父状态机对此透明。中断转发和上下文共享可通过事件监听与引用类型 `Context` 实现，无需框架内建层级。
- **装饰器**：实现 `IState` 接口包装原有状态，即可在不修改原代码的情况下添加日志、性能监控等横切关注点。

### 泛型状态标识
框架接口 `IState<TKey, TContext>`、`ITransition<TKey, TContext>`、`IStateMachine<TKey, TContext>` 均支持泛型标识 `TKey`。默认实现 `StateMachine<TContext>` 使用 `string` 作为 Key，但你完全可以基于接口实现基于 `enum` 或 `int` 的强类型状态机。

## 常见问题

**问：事件驱动转换的 `Delay` 特性是否生效？**  
当前默认实现（`StateMachine<TContext>`）的事件转换评估中未加入延迟逻辑，`Delay` 仅对自动转换有效。若需事件转换延迟，可自定义状态机实现或触发后立即进入一个中间等待状态。

**问：如何在 `OnStateChanged` 回调中安全地发送事件？**  
框架会静默忽略在转换回调期间发送的 `SendEvent` 调用，避免重入。你可以在回调外使用标志位，在下一帧或异步任务中发送。

**问：能否在多个线程中使用状态机？**  
框架未提供线程安全保证，所有状态操作（`Start`、`Update`、`SendEvent` 等）应在同一线程调用，多线程场景需自行加锁。

## 其他文档

- [API 文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)

## 许可证

本项目采用 [MIT 许可证](LICENSE)。