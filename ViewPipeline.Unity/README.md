# ViewPipeline - Unity 视图管线框架

> 内容由 AI 根据核心代码生成，已通过人工审核。

![MIT License](https://img.shields.io/badge/License-MIT-green.svg)
![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)
![Dependencies](https://img.shields.io/badge/dependencies-UniTask-orange.svg)
![Tests](https://img.shields.io/badge/tests-NUnit%20%7C%20UnityTestFramework-red.svg)

## 📖 简介

ViewPipeline 是一个为 Unity 游戏引擎设计的高性能、可扩展的视图生命周期管理框架。它采用 **管道-中间件（Pipeline-Middleware）** 架构模式，将视图的打开（Open）和关闭（Close）操作抽象为可编排的执行管道，开发者可以通过中间件机制灵活地插入横切关注点（如权限校验、数据缓存、加载动画、埋点上报等），而无需修改核心视图逻辑。

**核心特性：**
- 🔄 **双向管线**：分别管理 Open 和 Close 两个独立的执行管道
- 🧩 **中间件机制**：支持静态中间件和动态中间件流式供应器
- ✅ **验证器系统**：构建时对中间件配置进行校验，支持 Error/Warning 级别
- 🎯 **执行策略**：可动态决定是否跳过特定中间件
- ♻️ **对象池化**：上下文、会话等高频对象使用对象池，减少 GC 分配
- 📦 **扩展包支持**：通过 `IExtension` 接口实现模块化集成

## 📋 环境要求

| 要求 | 版本/说明 |
|------|-----------|
| Unity | 2020.3 及以上 |
| .NET | 4.x 或 Standard 2.0 |
| UniTask | 2.x 或更高 |
| 测试框架 | NUnit 1.0+, Unity Test Framework 1.1+ |

## 📦 安装方式

### 方式一：源码导入

1. 下载或克隆本仓库
2. 将 `ViewPipeline.Unity.Core` 目录下的所有源码复制到您的 Unity 项目中的任意位置
3. 确保项目中已安装 [UniTask](https://github.com/Cysharp/UniTask)（通过 UPM 或源码导入）

### 方式二：DLL 导入

1. 编译源码生成 `ViewPipeline.Unity.Core.dll`
2. 将 DLL 文件放入 Unity 项目的 `Assets` 目录下
3. 确保项目中已引用 UniTask 的对应 DLL

## 🎯 设计理念

### 管道-中间件模式

```
┌─────────────────────────────────────────────────────┐
│                   OpenViewAsync                      │
├─────────────────────────────────────────────────────┤
│  Middleware 1 → Middleware 2 → ... → View.ShowAsync │
└─────────────────────────────────────────────────────┘
```

框架将视图的生命周期操作拆解为一系列的中间件处理阶段，每个中间件可以：
- 在视图操作前后执行自定义逻辑
- 决定是否继续执行下一个中间件
- 中断整个管线的执行

### 静态与动态中间件

- **静态中间件**：在构建会话时确定，对每个视图都执行
- **动态中间件**：通过供应器（Provider）在运行时根据视图类型、状态等条件动态决定是否添加

### 资源管理

- 所有可释放资源都实现标准的 `IDisposable` 或 `IAsyncDisposable` 接口
- 会话对象支持异步释放，等待所有进行中的操作完成后再释放资源

## 🔧 功能说明

### 1. 视图会话（ViewSession）

`IViewSession` 是框架的核心入口，提供两个主要方法：
- `OpenViewAsync`：打开/激活视图
- `CloseViewAsync`：关闭/隐藏视图

### 2. 构建器（ViewSessionBuilder）

使用流式 API 配置和创建视图会话：

- 自定义视图注册表
- 自定义导航栈策略
- 添加静态中间件（Open/Close）
- 添加动态中间件供应器（Open/Close）
- 设置中间件执行策略
- 集成扩展包

### 3. 中间件（IViewMiddleware）

中间件通过 `InvokeAsync` 方法实现拦截逻辑，并通过 `UIPipelineExecutor.NextAsync` 控制流转：

```csharp
public async UniTask InvokeAsync(IView view, UIPipelineExecutor executor, CancellationToken token)
{
    // 前置逻辑
    await executor.NextAsync(view, token);  // 继续执行
    // 后置逻辑
}
```

### 4. 动态中间件供应器（IDynamicMiddlewareProvider）

根据当前视图和静态中间件集合，动态决定添加哪些中间件：

```csharp
void PopulateMiddlewares(IView view, IReadOnlyList<IViewMiddleware> staticMiddlewares, IDynamicMiddlewareCollection dynamicMiddlewares);
```

### 5. 验证器（IMiddlewareValidator）

在构建会话时对中间件配置进行校验，可抛出错误阻止构建或输出警告日志。

### 6. 执行策略（IMiddlewareExecutionPolicy）

运行时判断是否跳过特定中间件，适用于 A/B 测试、功能开关等场景。

### 7. 扩展包（IExtension）

将中间件、供应器、验证器打包成一个独立模块，通过 `builder.AddExtension(extension)` 一键装配。

### 8. 内置实现

| 类型 | 实现 | 说明 |
|------|------|------|
| 视图注册表 | `DefaultViewRegistry` | 基于 HashSet 的活跃视图管理 |
| 导航栈策略 | `DefaultViewStackPolicy` | 基于 LinkedList + Dictionary 的 O(1) 栈操作 |
| 动态中间件集合 | `DefaultDynamicMiddlewareList` | 支持动态增删的中间件包装器 |
| 上下文集合 | `DefaultPipelineContextCollection` | 带异步释放的上下文对象池 |

## ❓ 常见问题

### Q: 中间件执行顺序是怎样的？

A: 按照添加到构建器的顺序执行。前置逻辑（NextAsync 之前）按添加顺序执行，后置逻辑（NextAsync 之后）按相反顺序执行。

### Q: 如何在中间件中中断管线执行？

A: 调用 `executor.Abort()` 方法，并且不调用 `NextAsync`。此时不会执行后续中间件，也不会执行视图的 ShowAsync/HideAsync。

### Q: 动态供应器中的 staticMiddlewares 参数有什么用？

A: 供应器可以根据静态中间件的配置来决定是否添加特定的动态中间件，例如避免重复添加功能相同的中间件。

### Q: 会话释放后还能操作吗？

A: 不能。调用 `DisposeAsync` 后，再调用 `OpenViewAsync` 或 `CloseViewAsync` 会抛出 `InvalidOperationException`。

### Q: 并发调用 OpenViewAsync 安全吗？

A: 是的。`ViewSession` 内部维护了操作计数器，并在释放时会等待所有进行中的操作完成。

## 📚 其它文档

- [API 文档 (DOCUMENT.md)](DOCUMENT.md) - 公共 API 详细说明
- [测试报告 (TEST_REPORT.md)](TEST_REPORT.md) - 单元测试与性能测试结果

## 📄 许可证

本项目采用 MIT 许可证，详见 LICENSE 文件。