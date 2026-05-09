> 内容由 AI 根据核心代码生成，已通过人工审核。

# Orchestrator API 文档

## 公共 API

Orchestrator 库提供了三个命名空间下的编排器实现，共享同一套核心类型。

---

### 核心公共类型（所有版本共享）

#### 枚举

| API 签名 | 作用 |
|----------|------|
| `enum StepFlow { Continue, Break, Fail }` | 步骤流转状态：`Continue` 正常继续；`Break` 业务中断（不视为错误）；`Fail` 失败并携带异常。 |
| `enum InterruptionPolicy { Strict, DependencyBased, Ignore }` | 中断策略：`Strict` 全局中断；`DependencyBased` 仅阻断依赖分支；`Ignore` 忽略中断继续执行。 |

#### 步骤结果

| API 签名 | 作用 |
|----------|------|
| `readonly struct StepResult<T>` | 单步执行结果，包含 `Flow`（流转状态）、`Output`（产出数据）、`Exception`（异常）。通过静态工厂方法创建。 |
| `static StepResult<T> Continue(T output)` | 创建成功继续的结果。 |
| `static StepResult<T> Break(T output = default)` | 创建业务中断的结果。 |
| `static StepResult<T> Fail(Exception ex)` | 创建失败结果，携带异常。 |

#### 执行审计

| API 签名 | 作用 |
|----------|------|
| `readonly struct StepExecutionResult : IStepExecutionResult` | 单步执行审计信息：`StepName`、`Success`、`Flow`、`Output`（`object`）、`Exception`、`Duration`。由引擎内部创建。 |
| `readonly struct ExecutionResult<T> : IExecutionResult<T, StepExecutionResult>` | 工作流最终结果：`Success`（整体是否成功）、`Output`（最终产出）、`StepResults`（所有步骤审计列表）、`Duration`（总耗时）。 |

#### 核心接口

| API 签名 | 作用 |
|----------|------|
| `interface IStep { string Name; IReadOnlyCollection<IStep> Dependencies; }` | 步骤基类型，定义名称和依赖集合。所有步骤接口均继承自此接口。 |
| `interface IStepExecutionResult` | 单步执行结果快照的只读接口。 |
| `interface IExecutionResult<out T, TStepResult> where TStepResult : IStepExecutionResult` | 工作流最终结果的只读接口，支持协变。 |

#### 工具类

| API 签名 | 作用 |
|----------|------|
| `static class OrchestratorUtility` | 图算法工具类。 |
| `static bool ValidateNoCycles(IEnumerable<IStep> steps, out IEnumerable<string> cycleSteps)` | Kahn 算法循环检测，返回是否无环，`cycleSteps` 输出参与循环的步骤名称。 |
| `static List<IStep> TopologicalSort(IEnumerable<IStep> steps)` | Kahn 算法拓扑排序，返回按依赖顺序排列的步骤列表。调用前应确保无环。 |

---

### Task 版本 API (`Orchestrator.Tasks`)

#### 步骤接口

| API 签名 | 作用 |
|----------|------|
| `interface ITaskStep<TIn, TOut> : IStep` | 标准异步步骤，返回 `Task<StepResult<TOut>>`。 |
| `interface ITaskContextStep<TContext> : IStep` | 共享上下文步骤，直接操作并修改 `TContext`，返回 `Task<StepFlow>`。 |

#### 行为接口

| API 签名 | 作用 |
|----------|------|
| `interface ITaskBehavior<TIn, TOut>` | 横切行为，包裹步骤执行形成中间件管道。实现 `HandleAsync(TIn input, Func<Task<StepResult<TOut>>> next, CancellationToken token)`。 |

#### 编排器

| API 签名 | 作用 |
|----------|------|
| `sealed class TaskOrchestrator<TIn, TOut>` | Task 版本编排器，通过内部 `Builder` 创建实例，调用 `ExecuteAsync` 执行工作流。 |
| `Task<ExecutionResult<TOut>> ExecuteAsync(TIn input, CancellationToken token = default)` | 执行编排并返回最终结果。 |

#### 编排器构建器

| API 签名 | 作用 |
|----------|------|
| `static Builder Create()` | 创建构建器实例。 |
| `Builder AddStep(ITaskStep<TIn, TOut> step)` | 添加步骤，添加顺序不代表执行顺序。 |
| `Builder AddBehavior(ITaskBehavior<TIn, TOut> behavior)` | 添加全局行为，按添加顺序形成管道。 |
| `Builder UsePolicy(InterruptionPolicy policy)` | 设置中断策略，默认为 `DependencyBased`。 |
| `Builder SetFinalStep(ITaskStep<TIn, TOut> finalStep)` | 显式指定最终产出步骤；未设置时自动推断。 |
| `Builder WithMaxConcurrency(int count)` | 限制同时执行的步骤数量。 |
| `Builder MapInput<TCurrentIn>(ITaskStep<TCurrentIn, TOut> step, Func<TIn, IReadOnlyDictionary<IStep, object>, TCurrentIn> mapper)` | 为步骤定制输入映射，可访问先前步骤的输出缓存。 |
| `TaskOrchestrator<TIn, TOut> Build()` | 构建编排器，完成拓扑排序、循环检测和管道编译。每个构建器仅可调用一次。 |

---

### ValueTask 版本 API (`Orchestrator.ValueTasks`)

#### 步骤接口

| API 签名 | 作用 |
|----------|------|
| `interface IValueTaskStep<TIn, TOut> : IStep` | 标准异步步骤，返回 `ValueTask<StepResult<TOut>>`。 |
| `interface IValueTaskContextStep<TContext> : IStep` | 共享上下文步骤，返回 `ValueTask<StepFlow>`。 |

#### 行为接口

| API 签名 | 作用 |
|----------|------|
| `interface IValueTaskBehavior<TIn, TOut>` | 横切行为，实现 `HandleAsync(TIn input, Func<ValueTask<StepResult<TOut>>> next, CancellationToken token)`。 |

#### 编排器

| API 签名 | 作用 |
|----------|------|
| `sealed class ValueTaskOrchestrator<TIn, TOut>` | ValueTask 版本编排器。 |
| `ValueTask<ExecutionResult<TOut>> ExecuteAsync(TIn input, CancellationToken token = default)` | 执行编排并返回最终结果。 |

#### 编排器构建器

| API 签名 | 作用 |
|----------|------|
| `static Builder Create()` | 创建构建器实例。 |
| `Builder AddStep(IValueTaskStep<TIn, TOut> step)` | 添加步骤。 |
| `Builder AddBehavior(IValueTaskBehavior<TIn, TOut> behavior)` | 添加全局行为。 |
| `Builder UsePolicy(InterruptionPolicy policy)` | 设置中断策略。 |
| `Builder SetFinalStep(IValueTaskStep<TIn, TOut> finalStep)` | 设置最终产出步骤。 |
| `Builder WithMaxConcurrency(int count)` | 限制并发数。 |
| `Builder MapInput<TCurrentIn>(IValueTaskStep<TCurrentIn, TOut> step, Func<TIn, IReadOnlyDictionary<IStep, object>, TCurrentIn> mapper)` | 输入映射。 |
| `ValueTaskOrchestrator<TIn, TOut> Build()` | 构建编排器。 |

#### 工具类

| API 签名 | 作用 |
|----------|------|
| `static class ValueTaskOrchestratorUtility` | ValueTask 辅助工具。 |
| `static ValueTask<T[]> WhenAll<T>(ValueTask<T>[] tasks)` | 等待所有 ValueTask 完成。同步路径零分配，异步路径退化为 `Task.WhenAll`。 |

---

### UniTask 版本 API (`Orchestrator.UniTasks`)

> UniTask 版本依赖 [Cysharp.Threading.Tasks](https://github.com/Cysharp/UniTask)。

#### 步骤接口

| API 签名 | 作用 |
|----------|------|
| `interface IUniTaskStep<TIn, TOut> : IStep` | Unity 异步步骤，返回 `UniTask<StepResult<TOut>>`。 |
| `interface IUniTaskContextStep<TContext> : IStep` | 共享上下文步骤，返回 `UniTask<StepFlow>`。 |

#### 行为接口

| API 签名 | 作用 |
|----------|------|
| `interface IUniTaskBehavior<TIn, TOut>` | Unity 横切行为，实现 `HandleAsync(TIn input, Func<UniTask<StepResult<TOut>>> next, CancellationToken token)`。 |

#### 编排器

| API 签名 | 作用 |
|----------|------|
| `sealed class UniTaskOrchestrator<TIn, TOut>` | UniTask 版本编排器，专为 Unity 优化，调度器基于 PlayerLoop。 |
| `UniTask<ExecutionResult<TOut>> ExecuteAsync(TIn input, CancellationToken token = default)` | 执行编排并返回最终结果。 |

#### 编排器构建器

| API 签名 | 作用 |
|----------|------|
| `static Builder Create()` | 创建构建器实例。 |
| `Builder AddStep(IUniTaskStep<TIn, TOut> step)` | 添加步骤。 |
| `Builder AddBehavior(IUniTaskBehavior<TIn, TOut> behavior)` | 添加全局行为。 |
| `Builder UsePolicy(InterruptionPolicy policy)` | 设置中断策略。 |
| `Builder SetFinalStep(IUniTaskStep<TIn, TOut> finalStep)` | 设置最终产出步骤。 |
| `Builder WithMaxConcurrency(int count)` | 限制并发数。 |
| `Builder MapInput<TCurrentIn>(IUniTaskStep<TCurrentIn, TOut> step, Func<TIn, IReadOnlyDictionary<IStep, object>, TCurrentIn> mapper)` | 输入映射。 |
| `UniTaskOrchestrator<TIn, TOut> Build()` | 构建编排器。 |

---

## 使用示例

以下示例以 Task 版本演示典型用法，ValueTask 和 UniTask 版本仅需替换编排器类型和 `await` 语法。

### 定义步骤

```csharp
// 校验步骤
public class ValidateStep : ITaskStep<string, string>
{
    public string Name => "Validate";
    public IReadOnlyCollection<IStep> Dependencies => Array.Empty<IStep>();

    public Task<StepResult<string>> ExecuteAsync(string input, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Task.FromResult(StepResult<string>.Fail(new ArgumentException("输入为空")));
        return Task.FromResult(StepResult<string>.Continue(input.Trim()));
    }
}

// 加载步骤（依赖 ValidateStep）
public class LoadDataStep : ITaskStep<string, string>
{
    public string Name => "LoadData";
    public IReadOnlyCollection<IStep> Dependencies { get; }

    public LoadDataStep(IStep dependency) => Dependencies = new[] { dependency };

    public async Task<StepResult<string>> ExecuteAsync(string input, CancellationToken token)
    {
        await Task.Delay(100, token); // 模拟异步
        return StepResult<string>.Continue($"已加载: {input}");
    }
}
```

### 构建编排器

```csharp
var validate = new ValidateStep();
var load = new LoadDataStep(validate);

var orchestrator = TaskOrchestrator<string, string>.Builder
    .Create()
    .AddStep(validate)
    .AddStep(load)
    .SetFinalStep(load)
    .UsePolicy(InterruptionPolicy.DependencyBased)
    .AddBehavior(new LoggingBehavior<string, string>())  // 可选行为链
    .Build();
```

### 执行并处理结果

```csharp
var result = await orchestrator.ExecuteAsync("  hello  ", CancellationToken.None);

if (result.Success)
{
    Console.WriteLine($"成功: {result.Output}, 耗时: {result.Duration.TotalMilliseconds}ms");
}
else
{
    foreach (var step in result.StepResults)
    {
        if (!step.Success)
            Console.WriteLine($"{step.StepName} 失败: {step.Exception?.Message}");
    }
}
```

### 使用输入映射（ValueTask 版本示例）

```csharp
var prev = new SyncStep("Prev", "value1");
var current = new SyncStep("Current", "value2", new[] { prev });

var orchestrator = ValueTaskOrchestrator<string, string>.Builder
    .Create()
    .AddStep(prev)
    .AddStep(current)
    .MapInput(current, (input, cache) => $"{input}_{cache[prev]}")
    .SetFinalStep(current)
    .Build();

var result = await orchestrator.ExecuteAsync("start", CancellationToken.None);
// current 收到的输入将是 "start_value1"
```

### UniTask 版本在 MonoBehaviour 中使用

```csharp
public class GameController : MonoBehaviour
{
    public async UniTaskVoid OnStartGame()
    {
        var orch = UniTaskOrchestrator<string, string>.Builder.Create()
            .AddStep(new ValidateStep())
            .Build();

        var token = this.GetCancellationTokenOnDestroy();
        var result = await orch.ExecuteAsync("input", token);
        // 处理 result...
    }
}
```