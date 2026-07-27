> 内容由 AI 根据核心代码生成，已通过人工审核。

# CodeGenerator Framework

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET_Standard-2.0-blueviolet)]()
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)

一个轻量、灵活、完全解耦的 C# 代码生成框架，适用于任何需要模板驱动代码生成的 .NET 项目。通过可插拔的 **模板提供者**、**生成器** 和 **写入器** 抽象，配合 **中介者模式** 进行集中管理，让批量生成代码变得简洁可靠，同时完整支持同步与异步执行。

## 安装环境要求

- .NET Framework 4.x / .NET Standard 2.0 或兼容版本
- C# 7.0 或更高版本

## 安装方式

### 通过源码导入
1. 将本仓库的 `CodeGenerator` 命名空间下的所有 `.cs` 文件复制到您的项目中。
2. 确保项目脚本编译完成即可使用。

### 通过 DLL 导入
1. 在 Releases 页面下载预编译的 `CodeGenerator.dll`。
2. 将 DLL 放入您自己的插件目录中。
3. 重新编译项目即可调用相关 API。

## 设计理念

框架将代码生成流程抽象为三个独立环节：

- **模板读取**（`ITemplateProvider`） – 从任意来源获取模板内容。
- **内容生成**（`IGenerator`） – 依据模板生成最终输出。
- **结果写入**（`IWriter`） – 将生成内容持久化到指定路径。

每一环节都通过泛型接口定义，支持不同的数据类型；同步与异步版本完全分离，满足不同 IO 场景的性能需求。所有生成器由 **中介者**（`IGeneratorMediator`）统一扫描、托管和执行，只需在生成器类上标记 `[GeneratorConfig]` 特性即可自动注册。

## 具体功能说明

### 1. 模板提供者
`ITemplateProvider<TTemplate>` 及其异步版本负责从模板路径加载原始模板。您可以实现为文件读取、内嵌资源读取，或从网络获取。

### 2. 代码生成器
`IGenerator<TTemplate, TContent>` 是核心转换接口，接收模板并生成最终内容。异步版本 `IAsyncGenerator<TTemplate, TContent>` 支持取消令牌，适合耗时操作。所有生成器只需要实现泛型接口并在类上添加 `[GeneratorConfig]` 特性，框架即可自动发现。

### 3. 内容写入器
`IWriter<TContent>` 定义写入行为，负责将生成结果保存到 `OutputPath`。异步版本 `IAsyncWriter<TContent>` 可避免主线程阻塞。

### 4. 中介者管理
`IGeneratorMediator<TGenerator>` 提供扫描（`Rescan`）、清理（`Clear`）、运行全部（`RunAll`）和运行指定生成器（`Run<T>`）的能力。异步中介者接口 `IAsyncGeneratorMediator<TGenerator>` 功能类似，所有方法均返回 `Task` 并支持取消令牌。基类 `BaseGeneratorMediator<TGenerator>` 已实现字典式访问（通过 `Type` 获取元数据）及资源释放逻辑，您只需继承并实现扫描策略即可快速构建具体的管理器。

### 5. 声明式配置
`GeneratorConfigAttribute` 允许直接在生成器类上指定模板路径和输出路径，免去额外配置文件，遵循“约定优于配置”原则。

## 常见问题

**问：我的生成器需要依赖其他服务怎么办？**  
答：框架只定义了抽象，您可以结合依赖注入容器（如 Zenject、VContainer）在生成器构造函数中注入所需服务，并在中介者的 `Rescan` 阶段完成实例化。

**问：异步生成器与同步接口能否混用？**  
答：框架刻意将同步与异步接口分离，不建议在同一工作流中混用。通常选择统一使用异步接口以获得更好的可维护性和性能。

## 其它文档导航

- [API 详细说明](./source/1.0.1-beta/DOCUMENT.md)  
  包含所有公共接口、类、成员的签名与作用。
- [测试报告](./tests/1.0.1-beta/TEST_REPORT.md)   
  单元测试和基准测试。

## 许可证

本项目采用 [MIT](../LICENSE) 许可证。