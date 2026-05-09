> 内容由 AI 根据核心代码生成，已通过人工审核。

# Orchestrator

![MIT License](https://img.shields.io/badge/license-MIT-green)
![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue)
![.NET Version](https://img.shields.io/badge/.NET_Standard-2.0%20-blueviolet)
![UniTask](https://img.shields.io/badge/UniTask-2.3.1%2B-orange)
![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-0.15.8-passing)

> 一个轻量级、高性能的异步工作流编排引擎，支持 Task、ValueTask、UniTask 三种异步基元。

## 简介

**Orchestrator** 是一个 .NET 异步工作流编排库，可将复杂的多步骤异步操作定义为**有向无环图 (DAG)**，通过 Builder 模式配置依赖关系、中断策略、并发限制和行为管道，最终自动完成拓扑排序、并发调度和结果审计。

这套库提供三个版本，共享同一套核心逻辑，仅异步基元不同：
- **Task 版本** (`Orchestrator.Tasks`) — 标准 .NET `Task`，生态兼容性最广。
- **ValueTask 版本** (`Orchestrator.ValueTasks`) — `ValueTask` 实现，内存分配更低。
- **UniTask 版本** (`Orchestrator.UniTasks`) — 专为 Unity 设计，GC 压力极低。

## 安装环境要求

- 目标框架: .NET Standard 2.0（兼容 .NET Core 2.0+、.NET Framework 4.6.1+、Unity 2019.4+）  
Unity 版本: 2020.3+ (UniTask 版本)  

- 依赖:
UniTask 版本：需安装 UniTask 包 (v2.3.1+)  
Task / ValueTask 版本：无额外依赖  

- 测试框架:  
Unity 单元测试及性能测试：Unity Test Framework + Performance Testing API  
.NET 单元测试：xUnit  
基准测试：BenchmarkDotNet 0.15.8 (Task / ValueTask 版本)

## 安装方式

### 方式一：源码导入

将整个 `Orchestrator` 文件夹复制到项目的 `Assets/` 或源码目录下，所有类型即可直接使用。

### 方式二：DLL 文件导入

1. 将源码编译为 DLL。
2. 将编译后的 `Orchestrator.dll` 放入：
   - Unity: `Assets/Plugins/` 目录
   - .NET 项目: 通过引用添加至项目

## 设计理念

传统异步流程控制通常使用 `async/await` 链式调用或手动管理依赖，当步骤增多、依赖复杂时，代码耦合度急剧增加，且难以处理中断、并行限制、日志审计等横切关注点。

Orchestrator 将工作流视为**有向无环图 (DAG)**，每个步骤只声明“我依赖谁、我做什么”，所有调度逻辑交给引擎。架构上遵循：

- **关注点分离**：步骤定义 (IStep/IXxxStep) 与执行引擎 (XxxOrchestrator) 完全解耦。
- **装饰器模式**：行为管道 (IXxxBehavior) 可在不修改步骤的情况下注入日志、重试等横切逻辑。
- **构建期校验**：所有依赖分析、循环检测、拓扑排序均在 `Build()` 时完成，运行时零开销。
- **零分配倾向**：所有结果类型 (`StepResult<T>`, `StepExecutionResult`, `ExecutionResult<T>`) 均为 `readonly struct`，避免堆分配。

## 具体功能

### 1. 步骤定义与依赖声明

通过实现 `IXxxStep<TIn, TOut>` 接口定义步骤。步骤通过 `Dependencies` 属性声明其依赖，引擎自动解析依赖图：

```csharp
public class ValidateStep : ITaskStep<string, string>
{
    public string Name => "Validate";
    public IReadOnlyCollection<IStep> Dependencies => Array.Empty<IStep>();

    public Task<StepResult<string>> ExecuteAsync(string input, CancellationToken token)
    {
        // 业务逻辑
        return Task.FromResult(StepResult<string>.Continue(input));
    }
}
```

### 2. 有向无环图调度

引擎将所有步骤构建为 DAG，使用 **Kahn 算法**进行拓扑排序和循环检测。步骤按依赖关系自动并行执行（无依赖的多步骤同时启动），有依赖的步骤等待前驱完成后执行。

### 3. 中断策略

提供三种中断策略，通过 `UsePolicy()` 配置：

| 策略 | 行为 |
|------|------|
| `Strict` | 任一步骤中断，全局所有未开始步骤停止 |
| `DependencyBased` (默认) | 仅阻断依赖该中断步骤的分支，其他分支继续 |
| `Ignore` | 忽略中断，尝试执行所有步骤 |

### 4. 行为中间件（行为链）

通过 `IXxxBehavior<TIn, TOut>` 定义可复用的横切关注点，按添加顺序形成洋葱管道包裹每个步骤：

```csharp
// 日志行为：前置记录“开始”，后置记录“结束”
public class LoggingBehavior<TIn, TOut> : ITaskBehavior<TIn, TOut>
{
    public async Task<StepResult<TOut>> HandleAsync(TIn input, Func<Task<StepResult<TOut>>> next, CancellationToken token)
    {
        Console.WriteLine("Before");
        var result = await next();
        Console.WriteLine("After");
        return result;
    }
}
```

### 5. 并发限制

通过 `WithMaxConcurrency(n)` 使用信号量限制同时执行的步骤数，适用于保护有限资源（如网络连接数）的场景。

### 6. 输入映射

`MapInput()` 允许步骤从全局输入和先前步骤的输出缓存中提取所需数据，支持步骤输入类型与全局类型不同的场景。

### 7. 执行审计

每次执行返回 `ExecutionResult<T>`，包含整体成功状态、最终输出、每个步骤的详细信息（名称、状态、耗时、异常）和总耗时。

### 8. 取消令牌

所有步骤均接收 `CancellationToken`，支持外部取消。配合 Unity 的 `GetCancellationTokenOnDestroy()` 可实现对象销毁时自动取消。

### 9. 异构步骤支持 (ContextStep)

三个版本均提供 `IXxxContextStep<TContext>` 接口，允许所有步骤共享并修改同一个上下文对象，适用于步骤输入输出类型不统一的场景。

## 版本选择指南

| 使用场景 | 推荐版本 |
|----------|----------|
| Unity 游戏客户端，GC 要求苛刻 | UniTask |
| .NET 服务器/桌面应用，追求生态兼容 | Task |
| .NET 服务器/桌面应用，追求低内存分配 | ValueTask |

## 其它文档

- [API 文档](./DOCUMENT.md) — 全部公开 API 的详细说明
- [测试报告](./TEST_REPORT.md) — 单元测试与基准测试的详细结果

## 许可证

本项目使用 MIT 许可证。详见 [LICENSE](./LICENSE) 文件。