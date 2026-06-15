# Orchestrator

![MIT License](https://img.shields.io/badge/license-MIT-green)
![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue)
![.NET Version](https://img.shields.io/badge/.NET_Standard-2.0-blueviolet)
![UniTask](https://img.shields.io/badge/UniTask-2.3.1%2B-orange)
![Unit Tests](https://img.shields.io/badge/Unit%20Tests-passing-brightgreen)

> 一个轻量级、高性能的异步工作流编排引擎，支持 Task、ValueTask、UniTask 三种异步基元。

## 简介

**Orchestrator** 是一个 .NET 异步工作流编排库，可将复杂的多步骤异步操作定义为**有向无环图 (DAG)**，通过 Builder 模式配置依赖关系、中断策略、并发限制和行为管道，最终自动完成拓扑排序、并发调度和结果审计。

这套库提供三个版本，共享同一套核心逻辑，仅异步基元不同：
- **Task 版本** (`Orchestrator.Tasks`) — 标准 .NET `Task`，生态兼容性最广。
- **ValueTask 版本** (`Orchestrator.ValueTasks`) — `ValueTask` 实现，内存分配更低。
- **UniTask 版本** (`Orchestrator.UniTasks`) — 专为 Unity 设计，GC 压力极低。

## 安装环境要求

| 项目 | 要求 |
|------|------|
| 目标框架 | .NET Standard 2.0+ |
| Unity 版本 | 2020.3+ (UniTask 版本) |
| 依赖 (Task/ValueTask) | 无 |
| 依赖 (UniTask) | UniTask 2.3.1+ |

## 安装方式

### 方式一：源码导入

将整个 `Orchestrator` 文件夹复制到项目的 `Assets/` 或源码目录下，所有类型即可直接使用。

### 方式二：DLL 文件导入

1. 将源码编译为 DLL。
2. 将编译后的 `Orchestrator.dll` 和任一异步版本的 `Orchestrator.Xxx.dll` 放入：
   - Unity: `Assets/Plugins/` 目录
   - .NET 项目: 通过引用添加至项目

## 设计理念

传统异步流程控制通常使用 `async/await` 链式调用或手动管理依赖，当步骤增多、依赖复杂时，代码耦合度急剧增加，且难以处理中断、并行限制、日志审计等横切关注点。

Orchestrator 将工作流视为**有向无环图 (DAG)**，每个步骤只声明"我依赖谁、我做什么"，所有调度逻辑交给引擎。架构上遵循：

- **关注点分离**：步骤定义 (`IStep<TKey>` / `IXxxStep<TKey>`) 与执行引擎 (`XxxOrchestrator<TKey>`) 完全解耦。
- **装饰器模式**：行为管道 (`IXxxBehavior<TKey>`) 可在不修改步骤的情况下注入日志、重试等横切逻辑。
- **构建期校验**：所有依赖分析、循环检测、拓扑排序均在 `Build()` 时完成，运行时零开销。
- **零分配倾向**：`StepResult`、`StepExecutionResult`、`ExecutionResult` 均为 `readonly struct`，运行时通过对象池进一步减少分配。

## 核心概念

### 1. 步骤标识 (TKey)

每个步骤通过泛型 `TKey` 类型唯一标识，支持 `string`、`int`、`enum` 等任意类型。

```csharp
public interface IStep<TKey>
{
    TKey Key { get; }
    IReadOnlyCollection<IStep<TKey>> Dependencies { get; }
}
```

### 2. 步骤定义

通过实现 `IXxxStep<TKey>` 接口定义步骤，返回 `StepResult` 控制执行流向。

```csharp
public class LoadUserStep : ITaskStep<string>
{
    public string Key => "LoadUser";
    public IReadOnlyCollection<IStep<string>> Dependencies { get; } = Array.Empty<IStep<string>>();

    public async Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        var userId = context.Get<string, int>("userId").Value;
        var user = await LoadUserAsync(userId, token);
        context.Set("user", user);
        return StepResult.Continue();
    }
}
```

### 3. 共享上下文 (ITypedPipelineContext)

类型安全的键值存储，用于步骤间数据传递。

```csharp
// 写入
context.Set("userId", 123);
context.Set("user", user);

// 读取（返回 Optional<T>，安全处理缺失）
var userId = context.Get<string, int>("userId").Value;
var user = context.Get<string, User>("user");
if (user.HasValue) { ... }
```

### 4. 依赖声明

步骤通过 `Dependencies` 属性声明依赖关系，引擎自动拓扑排序。

```csharp
public class ProcessOrderStep : ITaskStep<string>
{
    public string Key => "ProcessOrder";
    public IReadOnlyCollection<IStep<string>> Dependencies { get; }
        = new[] { new LoadUserStep() };  // 依赖 LoadUserStep
}
```

### 5. 行为中间件

通过 `IXxxBehavior<TKey>` 定义可复用的横切关注点，按添加顺序形成洋葱管道包裹每个步骤。

```csharp
public class LoggingBehavior : ITaskBehavior<string>
{
    public async Task<StepResult> HandleAsync(
        ITypedPipelineContext context,
        TaskBehaviorStepper<string> stepper,
        CancellationToken token)
    {
        Debug.Log("Before");
        var result = await stepper.NextAsync(token);
        Debug.Log("After");
        return result;
    }
}
```

### 6. 中断策略

通过 `UsePolicy()` 配置三种中断策略：

| 策略 | 行为 |
|------|------|
| `Strict` | 任一步骤中断，全局所有未开始步骤停止 |
| `DependencyBased` (默认) | 仅阻断依赖该中断步骤的分支，其他分支继续 |
| `Ignore` | 忽略中断，尝试执行所有步骤 |

### 7. 并发限制

通过 `WithMaxConcurrency(n)` 限制同时执行的步骤数，适用于保护有限资源。

### 8. 取消令牌

所有步骤均接收 `CancellationToken`，支持外部取消。

## 使用示例

### 快速开始（串行执行）

```csharp
using Orchestrator;
using Orchestrator.Tasks;

// 1. 创建共享上下文
var context = new TypedPipelineContext();
context.Set("userId", 100);

// 2. 构建编排器
var orchestrator = TaskOrchestrator<string>.Builder.Create()
    .AddStep(new LoadUserStep())
    .AddStep(new ProcessOrderStep())
    .AddStep(new SaveResultStep())
    .AddBehavior<LoadUserStep>(new LoggingBehavior())
    .AddBehaviorForAll(new MetricsBehavior())
    .Build();

// 3. 串行执行
var result = await orchestrator.ExecuteAsyncSequentially(context);

// 4. 读取结果
var orderId = context.Get<string, int>("orderId").Value;
Console.WriteLine($"Order {orderId} completed");
```

### 并行执行

```csharp
// 对于无依赖关系的步骤，可以使用并行执行提高吞吐量
var result = await orchestrator.ExecuteAsyncInParallel(context);
```

### 步骤流转控制

```csharp
// 正常继续
return StepResult.Continue();

// 业务中断（不视为错误）
return StepResult.Break();

// 执行失败
return StepResult.Fail(new Exception("Something went wrong"));
```

### 构建器配置详解

```csharp
var orchestrator = TaskOrchestrator<StepType>.Builder
    .Create()
    .AddStep(step1)
    .AddStep(step2)
    .AddStep(step3)
    .UsePolicy(InterruptionPolicy.DependencyBased)  // 设置中断策略
    .WithMaxConcurrency(4)                          // 限制最大并发数
    .AddBehavior<StepA>(new RetryBehavior(3))       // 为特定步骤添加行为
    .AddBehavior(typeof(StepB), typeof(StepC), new MetricsBehavior())  // 批量添加
    .AddBehaviorForAll(new LoggingBehavior())       // 为所有步骤添加行为
    .Build();
```

## 版本选择指南

| 使用场景 | 推荐版本 |
|----------|----------|
| Unity 游戏客户端，GC 要求苛刻 | UniTask |
| .NET 服务器/桌面应用，追求生态兼容 | Task |
| .NET 服务器/桌面应用，追求低内存分配 | ValueTask |

## 版本差异说明

| 特性 | Task 版本 | ValueTask 版本 | UniTask 版本 |
|------|-----------|----------------|--------------|
| 串行执行 | ✅ | ✅ | ✅ |
| 并行执行 | ✅ | ❌ | ✅ |
| 行为管道 | ✅ | ✅ | ✅ |
| 中断策略 | ✅ | ✅ | ✅ |
| 并发限制 | ✅ | ✅ | ✅ |

> ⚠️ **注意**：`ValueTaskOrchestrator` 当前仅支持串行执行，并行执行支持将在后续版本中添加。

## 性能特性

- **零 GC 分配**：运行时所有临时数组通过 `ArrayPool` 复用
- **结构体步进器**：行为管道零委托分配
- **对象池体系**：`ListPool`、`DictionaryPool`、`ArrayPool`
- **微秒级调度**：100 步串行约 0.17 ms

## 其它文档

- [API 文档](./documents/1.0.1-beta/DOCUMENT.md) — 全部公开 API 的详细说明
- [测试报告](./tests/1.0.1-beta/Orchestrator%20Performance%20Evaluation%20Report.md) — 单元测试与性能测试的详细结果

## 许可证

本项目使用 MIT 许可证。详见 [LICENSE](/LICENSE) 文件。