# ViewPipeline - Unity 视图管线框架

> 内容由 AI 根据核心代码生成，已通过人工审核。

![MIT License](https://img.shields.io/badge/License-MIT-green.svg)
![Unity Version](https://img.shields.io/badge/Unity-2018.4%2B-blue.svg)
![Dependencies](https://img.shields.io/badge/dependencies-UniTask-orange.svg)
![Tests](https://img.shields.io/badge/tests-NUnit%20%7C%20UnityTestFramework-red.svg)
![Version](https://img.shields.io/badge/version-1.0.1--beta-yellow.svg)

## 📖 简介

ViewPipeline 是一个为 Unity 游戏引擎设计的高性能、可扩展的视图生命周期管理框架。它采用 **管道-中间件（Pipeline-Middleware）** 架构模式，将视图的打开（Open）和关闭（Close）操作抽象为可编排的执行管道，开发者可以通过中间件机制灵活地插入横切关注点（如权限校验、数据缓存、加载动画、埋点上报等），而无需修改核心视图逻辑。

**核心特性：**
- 🔄 **双向管线**：分别管理 Open 和 Close 两个独立的执行管道
- 🧩 **中间件机制**：支持静态中间件和动态中间件流式供应器
- 📦 **扩展包系统**：通过 `IExtension` + `IValidatable` 实现模块化集成和前置验证
- 🎯 **流程控制**：支持跳过（`ISkippable*`）、终止（`ITerminable`）、外部策略（`IExecutionPolicy`）
- 💾 **强类型上下文**：`ITypedPipelineContext` 提供零装箱的键值存储
- 📸 **快照系统**：`SnapshotCache` 支持运行时状态检查、调试和扩展包自检
- ♻️ **对象池化**：上下文、会话、数组等高频对象使用池化，减少 GC 分配

## 📋 环境要求

| 要求 | 版本/说明 |
|------|-----------|
| Unity | 2018.4 及以上（推荐 2020.3+） |
| .NET | 3.5 Equivalent 或更高 |
| UniTask | 2.x 或更高 |

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
│                   OpenViewAsync                     │
├─────────────────────────────────────────────────────┤
│  Middleware 1 → Middleware 2 → ... → View.ShowAsync │
└─────────────────────────────────────────────────────┘
```

框架将视图的生命周期操作拆解为一系列的中间件处理阶段，每个中间件可以：
- 在视图操作前后执行自定义逻辑
- 决定是否继续执行下一个中间件
- 中断整个管线的执行

### 洋葱模型执行顺序

```
Middleware 1 (Before)
  ├─ Middleware 2 (Before)
  │    ├─ View.ShowAsync()
  │    └─ Middleware 2 (After)
  └─ Middleware 1 (After)
```

### 静态与动态中间件

- **静态中间件**：在构建会话时确定，默认对每个视图都执行。
- **动态中间件**：通过供应器（Provider）在运行时根据视图类型、状态等条件动态决定是否添加。

## 🔧 快速开始

### 最简示例

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using ViewPipeline.Unity;
using ViewPipeline.Unity.Core;

public class MyView : IView
{
    public async UniTask ShowAsync(CancellationToken cancellationToken)
    {
        gameObject.SetActive(true);
    }
    
    public async UniTask HideAsync(CancellationToken cancellationToken)
    {
        gameObject.SetActive(false);
    }
}

// 创建并打开视图
var session = ViewSessionBuilder.Create().Build();
await session.OpenViewAsync(new MyView(), CancellationToken.None);
```

### 添加中间件

```csharp
public class LoggingMiddleware : IViewMiddleware
{
    public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
    {
        Debug.Log($"[Before] {view.GetType().Name}");
        await executor.NextAsync(view, token);
        Debug.Log($"[After] {view.GetType().Name}");
    }
}

var session = ViewSessionBuilder.Create()
    .AddOpenMiddleware(new LoggingMiddleware())
    .Build();
```

### 使用强类型上下文

```csharp
var session = ViewSessionBuilder.Create()
    .WithTypedContext()
    .Build();

// 在中间件中
public class DataMiddleware : IViewMiddleware
{
    public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
    {
        executor.SetData("userId", 12345);
        var userId = executor.GetData<int>("userId");
        await executor.NextAsync(view, token);
    }
}
```

## ❓ 常见问题

### Q: 中间件执行顺序是怎样的？

A: 按照添加到构建器的顺序执行。前置逻辑（NextAsync 之前）按添加顺序执行，后置逻辑（NextAsync 之后）按相反顺序执行。

### Q: 如何在中间件中中断管线执行？

A: 调用 `executor.Abort()` 方法，并且不调用 `NextAsync`。此时不会执行后续中间件，也不会执行视图的 ShowAsync/HideAsync。

### Q: 强类型上下文和普通上下文有什么区别？

A: `ITypedPipelineContext` 提供类型安全的键值存储，零装箱零反射，适合在中间件之间传递数据。普通 `IPipelineContext` 是空标记接口，由用户自行实现。

### Q: 扩展包的验证器什么时候执行？

A: 在 `AddExtension` 时自动执行。如果验证返回 `Error`，扩展包不会被添加；如果返回 `Warning`，会记录日志但继续添加。

### Q: 会话释放后还能操作吗？

A: 不能。调用 `DisposeAsync` 后，再调用 `OpenViewAsync` 或 `CloseViewAsync` 会抛出 `InvalidOperationException`。

### Q: 并发调用 OpenViewAsync 安全吗？

A: 是的。`ViewSession` 内部维护了操作计数器，并在释放时会等待所有进行中的操作完成。

### Q: 性能如何？

A: 框架开销极低（~0.02-0.04ms/次），GC 分配可控（~70-140 字节/次），适合对性能敏感的游戏项目。详见 [TEST_REPORT.md](./TEST_REPORT.md)。

## 📚 其它文档

概念文档请参阅[CONCEPT.md](./CONCEPT.md)
详细 API 文档请参阅 [DOCUMENT.md](./DOCUMENT.md)
详细测试报告请参阅 [TEST_REPORT.md](./TEST_REPORT.md)
版本历史请参阅 [RELEASE.md](./RELEASE.md)

## 📄 许可证

本项目采用 MIT 许可证，详见 LICENSE 文件。