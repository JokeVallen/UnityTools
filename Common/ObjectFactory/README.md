> 内容由 AI 根据核心代码生成，已通过人工审核。

# Unity Object Factory

![MIT License](https://img.shields.io/badge/license-MIT-green)
![Unity](https://img.shields.io/badge/Unity-2020.3%2B-blue)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)

一个轻量级、可扩展的 Unity 对象工厂框架，为 `GameObject` 和 `Component` 的创建提供统一入口，并支持在运行时无缝替换底层实现（如对象池、测试替身等）。

## 简介

在 Unity 项目开发中，直接使用 `new GameObject()` 或 `AddComponent<T>()` 会很快导致创建逻辑分散、难以统一维护和测试。本工具库通过简洁的工厂接口与全局注册中心，将所有“创建”动作收敛起来，让您在不修改业务代码的前提下切换创建策略，同时内置了安全的错误处理与自动资源清理机制。

## 安装环境要求

- Unity 2020.3 或更高版本
- .NET Standard 2.0 兼容环境（C# 7.0）

## 安装方式

### 1. 源码导入
将 `Plugins/ObjectFactory` 文件夹下的所有 `.cs` 文件拷贝到项目的 `Assets` 目录中。

### 2. DLL 导入
将编译好的 `ObjectFactory.dll` 放入 `Assets/Plugins` 文件夹，并确保项目引用。

## 设计理念

- **统一入口**：所有对象创建都通过 `ObjectFactory` 获取工厂实例，杜绝散落的 `new` 调用。
- **开放封闭**：通过 `RegisterCreator<T>` 注入自定义工厂，扩展时无需修改现有业务代码。
- **安全第一**：初始化失败自动销毁已创建资源，提供 `ThrowOnError` 灵活控制异常策略。
- **最小依赖**：零第三方库，仅依赖 Unity 内置 API。

## 功能说明

- **游戏对象创建**：通过 `IGameObjectFactory` 可创建空对象、指定名称对象，或同时添加多个组件。
- **组件创建**：`IComponentFactory` 支持泛型与非泛型两种方式，在已有 `GameObject` 上添加组件并执行初始化回调。
- **全局工厂注册**：`ObjectFactory.RegisterCreator<T>()` 允许您注册任何自定义工厂，后续通过 `GetFactory<T>()` 或 `TryGetFactory<T>()` 获取实例。
- **内置默认实现**：对 `IGameObjectFactory` 和 `IComponentFactory` 提供开箱即用的默认工厂，无需注册。
- **错误处理与清理**：通过 `ThrowOnError` 属性控制回调异常行为；异常时自动销毁残留对象/组件，避免污染场景。
- **测试友好**：提供 `ClearCreators()` 方法，便于单元测试中重置状态。

## 常见问题

**问：我需要为每个场景注册一次工厂吗？**  
答：不需要，`ObjectFactory` 是静态全局的，一次注册全局生效。建议在游戏启动时（如 `RuntimeInitializeOnLoadMethod`）完成注册。

**问：如果注册了多个同接口工厂会怎样？**  
答：最后注册的会覆盖之前的，符合幂等注册习惯。

**问：默认实现能满足哪些需求？**  
答：默认的 `GameObjectFactory` 和 `ComponentFactory` 涵盖了绝大多数标准创建场景，包括参数校验和错误清理。您可以直接使用，无需额外配置。

## 其它文档导航

- [API 文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)

## 许可证

本项目基于 [MIT License](https://opensource.org/licenses/MIT) 开放使用。