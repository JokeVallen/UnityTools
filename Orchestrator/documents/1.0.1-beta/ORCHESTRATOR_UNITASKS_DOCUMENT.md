# Orchestrator.UniTasks API 文档

> 内容由 AI 根据核心代码生成，已通过人工审核。

## 命名空间

`Orchestrator.UniTasks`

---

## 概述

UniTask 版本专为 Unity 环境设计，使用 `Cysharp.Threading.Tasks.UniTask` 作为异步基元。

**前置依赖**：需安装 [UniTask](https://github.com/Cysharp/UniTask) v2.3.1+

**核心类型说明**：以下类型定义在 `Orchestrator` 命名空间中，UniTask 版本直接复用：

- `IStep<TKey>`
- `StepResult`
- `StepExecutionResult<TKey>`
- `ExecutionResult<TKey>`
- `ITypedPipelineContext`
- `TypedPipelineContext`
- `Optional<T>`
- `StepFlow`
- `InterruptionPolicy`
- `ListPool` / `DictionaryPool` / `ArrayPool`
- `OrchestratorUtility`

详见 [Orchestrator API 文档](./ORCHESTRATOR_DOCUMENT.md)。

---

## 接口

### `IUniTaskStep<TKey>`

异步步骤接口，继承自 `IStep<TKey>`。

| API | 说明 |
|-----|------|
| `UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)` | 异步执行业务逻辑 |

**示例**：

```csharp
public class LoadDataStep : IUniTaskStep<string>
{
    public string Key => "LoadData";
    public IReadOnlyCollection<IStep<string>> Dependencies { get; }
        = Array.Empty<IStep<string>>();

    public async UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        var id = context.Get<string, int>("id").Value;
        var data = await LoadDataAsync(id, token);
        context.Set("data", data);
        return StepResult.Continue();
    }
}
```

### `IUniTaskBehavior<TKey>`

横切行为接口，用于实现日志、重试、监控等横切关注点。

| API | 说明 |
|-----|------|
| `UniTask<StepResult> HandleAsync(ITypedPipelineContext context, UniTaskBehaviorStepper<TKey> stepper, CancellationToken token)` | 环绕处理，通过 `stepper.NextAsync()` 调用下一个行为或步骤 |

**示例**：

```csharp
public class LogBehavior : IUniTaskBehavior<string>
{
    public async UniTask<StepResult> HandleAsync(
        ITypedPipelineContext context,
        UniTaskBehaviorStepper<string> stepper,
        CancellationToken token)
    {
        Debug.Log("Before");
        var result = await stepper.NextAsync(token);
        Debug.Log("After");
        return result;
    }
}
```

---

## 结构体

### `UniTaskBehaviorStepper<TKey>`

行为步进器（只读结构体，零分配）。由编排器内部创建，传递给行为。

| API | 说明 |
|-----|------|
| `UniTask<StepResult> NextAsync(CancellationToken token)` | 步进到下一个行为或执行步骤 |

**说明**：用户无需直接创建此结构体，在行为实现中调用 `stepper.NextAsync(token)` 即可继续执行管道。

---

## 类

### `UniTaskOrchestrator<TKey>`

UniTask 版本编排器，负责工作流的调度和执行。

| API | 说明 |
|-----|------|
| `UniTask<ExecutionResult<TKey>> ExecuteAsync(ITypedPipelineContext context, CancellationToken token = default)` | 执行编排，返回执行结果 |

### `UniTaskOrchestrator<TKey>.Builder`

编排器构建器，采用流式 API 配置工作流。

| API | 说明 |
|-----|------|
| `static Builder Create()` | 创建构建器实例 |
| `Builder AddStep(IUniTaskStep<TKey> step)` | 添加步骤 |
| `Builder AddBehavior<TStep>(IUniTaskBehavior<TKey> behavior) where TStep : IUniTaskStep<TKey>` | 为指定类型的步骤添加行为 |
| `Builder AddBehavior<TStep1, TStep2>(IUniTaskBehavior<TKey> behavior)` | 批量为两种类型步骤添加行为 |
| `Builder AddBehavior<TStep1, TStep2, TStep3>(IUniTaskBehavior<TKey> behavior)` | 批量为三种类型步骤添加行为 |
| `Builder AddBehavior(IUniTaskBehavior<TKey> behavior, Type stepType)` | 为指定类型步骤添加行为（运行时） |
| `Builder AddBehavior(IUniTaskBehavior<TKey> behavior, params Type[] stepTypes)` | 批量为多种类型步骤添加行为 |
| `Builder AddBehaviorForAll(IUniTaskBehavior<TKey> behavior)` | 为当前所有已添加步骤添加行为 |
| `Builder UsePolicy(InterruptionPolicy policy)` | 设置中断策略 |
| `Builder WithMaxConcurrency(int count)` | 设置最大并发数 |
| `UniTaskOrchestrator<TKey> Build()` | 构建编排器 |

---

## 使用示例

### 基础使用

```csharp
using Orchestrator;
using Orchestrator.UniTasks;
using Cysharp.Threading.Tasks;

// 1. 定义步骤
public class LoadUserStep : IUniTaskStep<string>
{
    public string Key => "LoadUser";
    public IReadOnlyCollection<IStep<string>> Dependencies { get; }
        = Array.Empty<IStep<string>>();

    public async UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        var userId = context.Get<string, int>("userId").Value;
        var user = await LoadUserAsync(userId, token);
        context.Set("user", user);
        return StepResult.Continue();
    }
}

public class ProcessOrderStep : IUniTaskStep<string>
{
    public string Key => "ProcessOrder";
    public IReadOnlyCollection<IStep<string>> Dependencies { get; }
        = new[] { new LoadUserStep() };

    public async UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        var user = context.Get<string, User>("user").Value;
        var orderId = await ProcessOrderAsync(user, token);
        context.Set("orderId", orderId);
        return StepResult.Continue();
    }
}

// 2. 创建上下文
var context = new TypedPipelineContext();
context.Set("userId", 100);

// 3. 构建编排器
var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
    .AddStep(new LoadUserStep())
    .AddStep(new ProcessOrderStep())
    .AddStep(new SaveResultStep())
    .Build();

// 4. 执行（Unity 中可使用 GetCancellationTokenOnDestroy）
var result = await orchestrator.ExecuteAsync(context, this.GetCancellationTokenOnDestroy());

// 5. 读取结果
var orderId = context.Get<string, int>("orderId");
if (orderId.HasValue)
    Debug.Log($"Order {orderId.Value} completed");
```

### 添加行为

```csharp
// 定义日志行为
public class LogBehavior : IUniTaskBehavior<string>
{
    public async UniTask<StepResult> HandleAsync(
        ITypedPipelineContext context,
        UniTaskBehaviorStepper<string> stepper,
        CancellationToken token)
    {
        Debug.Log("Before");
        var result = await stepper.NextAsync(token);
        Debug.Log("After");
        return result;
    }
}

// 定义重试行为
public class RetryBehavior : IUniTaskBehavior<string>
{
    private readonly int _maxRetries;
    public RetryBehavior(int maxRetries) => _maxRetries = maxRetries;

    public async UniTask<StepResult> HandleAsync(
        ITypedPipelineContext context,
        UniTaskBehaviorStepper<string> stepper,
        CancellationToken token)
    {
        for (int i = 0; i <= _maxRetries; i++)
        {
            var result = await stepper.NextAsync(token);
            if (result.Flow != StepFlow.Fail)
                return result;
            await UniTask.Delay(1000, cancellationToken: token);
        }
        return StepResult.Fail(new Exception($"Failed after {_maxRetries} retries"));
    }
}

// 使用行为
var builder = UniTaskOrchestrator<string>.Builder.Create()
    .AddStep(new UnstableApiStep())
    .AddBehavior<UnstableApiStep>(new RetryBehavior(3))
    .AddBehaviorForAll(new LogBehavior());
```

### 中断策略与并发控制

```csharp
var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
    .AddStep(new StepA())
    .AddStep(new StepB())
    .AddStep(new StepC())
    .UsePolicy(InterruptionPolicy.Strict)  // 严格模式
    .WithMaxConcurrency(2)                 // 最多 2 个步骤并行
    .Build();
```

### 在 Unity MonoBehaviour 中使用

```csharp
public class GameBootstrapper : MonoBehaviour
{
    private async UniTask Start()
    {
        var context = new TypedPipelineContext();
        context.Set("sceneName", "MainScene");

        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(new LoadConfigStep())
            .AddStep(new LoadAssetsStep())
            .AddStep(new InitializeGameStep())
            .AddBehaviorForAll(new LogBehavior())
            .Build();

        var result = await orchestrator.ExecuteAsync(context, this.GetCancellationTokenOnDestroy());

        if (result.Success)
            Debug.Log("Game initialized successfully");
    }
}
```

### 批量添加行为

```csharp
// 方式1：为单个类型添加
builder.AddBehavior<LoadUserStep>(new LogBehavior());

// 方式2：为多个类型添加（泛型重载）
builder.AddBehavior<LoadUserStep, ProcessOrderStep>(new MetricsBehavior());

// 方式3：为多个类型添加（数组参数）
builder.AddBehavior(new RetryBehavior(3), typeof(UnstableApiStep), typeof(DatabaseStep));

// 方式4：为所有已添加步骤添加
builder.AddBehaviorForAll(new TracingBehavior());
```

---