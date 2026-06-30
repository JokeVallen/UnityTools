> 内容由 AI 根据核心代码生成，已通过人工审核。

# EasyAttributes

![License](https://img.shields.io/badge/license-MIT-green) ![.NET](https://img.shields.io/badge/.NET_Standard-2.0-blue) ![Test Framework](https://img.shields.io/badge/Unit_Test-xUnit%202.4.2-blue) ![Benchmark](https://img.shields.io/badge/benchmark-BenchmarkDotNet%200.15.8-orange)

轻量级、高性能的 .NET Attribute 驱动 AOP（面向切面编程）内核，提供缓存、重试、事务、日志等横切关注点的声明式处理。

## 简介

EasyAttributes 让你通过自定义 Attribute 和独立的处理器类，将横切逻辑与业务代码彻底分离。框架不绑定任何拦截技术，可灵活集成到 WPF、WinForms、控制台、Unity 等各种 .NET 应用中。

## 环境要求

- .NET Standard 2.0 或更高
- 支持 C# 7.3 及以上

## 安装

### 源码方式
1. 克隆仓库至本地。
2. 将 `EasyAttributes` 和 `EasyAttributes.Core` 项目添加到你的解决方案中。
3. 在你的主项目中引用这两个项目。

### DLL 导入
1. 从 Release 页面下载 `EasyAttributes.dll` 和 `EasyAttributes.Core.dll`。
2. 在你的项目中添加引用。

## 设计理念

框架层仅定义极简的核心契约（上下文、处理器、执行器），不包含任何场景假设。扩展层提供 9 种强类型上下文接口和对应的处理器基类，使开发者无需任何 `object` 转换即可编写类型安全的拦截逻辑。读写权限严格隔离（写入接口内部化），全局功能（如日志、缓存）通过构建器注入，处理器只读。整个库零依赖，完全自主。

## 功能说明

- **声明式切面**：继承 `EasyAttribute` 定义你自己的横切属性。
- **强类型处理器**：继承 `MethodProcessor<T>`、`PropertyProcessor<T>` 等基类，直接获取 `IMethodContext`、`IPropertyContext` 等场景上下文。
- **管道编排**：支持多处理器按优先级排序，可在任一处理器中止执行、替换返回值、跳过后续回调。
- **同步/异步统一**：处理器同步和异步版本均可混用。
- **全局功能注入**：通过构建器注入 `ILogger`、`ICache` 等基础设施服务，所有处理器可通过 `GetFeature<T>()` 获取。
- **状态共享**：处理器间可通过 `Items` 字典读写临时数据（如事务对象）。
- **异常处理**：可配置全局异常处理器，记录或吞没框架产生的异常。
- **程序集扫描**：支持自动发现和注册处理器。
- **高性能**：每次拦截仅约 142 ns，分配 528 B，每秒可处理超 700 万次拦截。

## 常见问题

**Q：框架是否会自动拦截所有标注的方法？**  
A：不会。EasyAttributes 是 AOP 内核，你需要配合拦截器（如基于 `DispatchProxy` 或手动包装）来触发管道。

**Q：如何处理一个 Attribute 同时标注在方法和属性上？**  
A：为方法和属性分别编写对应的处理器（`MethodProcessor<T>` 和 `PropertyProcessor<T>`），并将它们都注册上。运行时根据上下文类型自动匹配。

**Q：Features 和 Items 有何区别？**  
A：`Features` 用于注入全局、稳定的基础设施服务（如日志器），在构建执行器时配置，处理器只读。`Items` 用于处理器间传递临时数据（如事务），可在 `Before`/`Process`/`After` 中读写。

## 文档导航

- [API 文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)

## 许可证

本项目采用 [MIT](../LICENSE) 许可证。