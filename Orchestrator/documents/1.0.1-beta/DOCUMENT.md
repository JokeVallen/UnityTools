> 内容由 AI 根据核心代码生成，已通过人工审核。

# Orchestrator 编排工具库 API 文档

## 一、公共引用部分 (`Orchestrator` 命名空间)

公共部分提供了所有编排器共享的核心类型，包括步骤结果、执行上下文、中断策略和对象池等基础设施。

### 1. 核心枚举与结构体

#### `StepFlow` 枚举
```csharp
public enum StepFlow
{
    Continue,
    Break,
    Fail
}
```
步骤流转状态，控制执行引擎在处理完当前步骤后的动作逻辑。

| 成员 | 说明 |
|------|------|
| `Continue` | 正常继续，寻找并执行后续依赖节点 |
| `Break` | 业务中断，停止当前分支，不抛出异常 |
| `Fail` | 执行失败，立即停止并进入错误处理状态 |

#### `InterruptionPolicy` 枚举
```csharp
public enum InterruptionPolicy
{
    Strict,
    DependencyBased,
    Ignore
}
```
工作流中断策略，定义当步骤返回非 `Continue` 状态时引擎如何控制后续步骤。

| 成员 | 说明 |
|------|------|
| `Strict` | 严格模式，任一步骤中断则全局取消所有未开始步骤 |
| `DependencyBased` | 依赖模式（默认），仅阻断直接或间接依赖中断步骤的节点 |
| `Ignore` | 忽略模式，尝试运行所有已定义的步骤 |

#### `StepResult` 结构体
```csharp
public readonly struct StepResult
{
    public StepFlow Flow { get; }
    public Exception Exception { get; }
    public static StepResult Continue();
    public static StepResult Break();
    public static StepResult Fail(Exception ex);
}
```
步骤执行结果，通过静态工厂方法创建。

| 方法 | 说明 |
|------|------|
| `Continue()` | 创建继续执行的结果 |
| `Break()` | 创建中断执行的结果 |
| `Fail(Exception ex)` | 创建失败结果，携带异常信息 |

#### `ExecutionResult` 结构体
```csharp
public readonly struct ExecutionResult
{
    public bool Success { get; }
    public TimeSpan Duration { get; }
    public ExecutionResult(bool success, TimeSpan duration);
}
```
编排执行结果，包含整体执行状态和总耗时。

#### `StepExecutionResult<TKey>` 结构体
```csharp
public readonly struct StepExecutionResult<TKey>
{
    public Optional<TKey> StepKey { get; }
    public bool Success { get; }
    public StepFlow Flow { get; }
    public Exception Exception { get; }
    public TimeSpan Duration { get; }
    public StepExecutionResult(TKey stepKey, bool success, StepFlow flow, Exception exception, TimeSpan duration);
}
```
单个步骤的执行结果记录，包含步骤标识、执行状态、流转状态、异常和耗时。

#### `Optional<T>` 结构体
```csharp
public readonly struct Optional<T>
{
    public bool HasValue { get; }
    public T Value { get; }
    public static Optional<T> None { get; }
    public static implicit operator Optional<T>(T value);
    public static explicit operator T(Optional<T> optional);
}
```
可选值包装器，用于安全地表示可能存在或不存在的值。提供 `HasValue` 检查，避免空引用异常。

### 2. 核心接口

#### `IStep<TKey>` 接口
```csharp
public interface IStep<TKey>
{
    TKey Key { get; }
    IReadOnlyCollection<IStep<TKey>> Dependencies { get; }
}
```
所有步骤的基础契约。

| 成员 | 说明 |
|------|------|
| `Key` | 步骤唯一标识 |
| `Dependencies` | 依赖步骤集合，用于确定执行顺序 |

#### `ITypedPipelineContext` 接口
```csharp
public interface ITypedPipelineContext
{
    void Set<TKey, TValue>(TKey key, TValue value);
    Optional<TValue> Get<TKey, TValue>(TKey key);
    bool Remove<TKey, TValue>(TKey key);
    bool ContainsKey<TKey, TValue>(TKey key);
    void AddStepExecutionResult<TStepKey>(StepExecutionResult<TStepKey> stepExecutionResult);
    Optional<StepExecutionResult<TStepKey>> GetStepExecutionResult<TStepKey>(TStepKey key);
    IEnumerable<StepExecutionResult<TStepKey>> GetAllStepExecutionResults<TStepKey>();
    void Clear();
}
```
强类型上下文接口，提供类型安全的键值存储和步骤执行结果管理。

**泛型方法说明：**
- `Set<TKey, TValue>` - 存储键值对，键和值类型共同决定存储位置
- `Get<TKey, TValue>` - 获取存储的值，返回 `Optional<TValue>`
- `Remove<TKey, TValue>` - 移除指定键的值
- `ContainsKey<TKey, TValue>` - 检查键是否存在
- `AddStepExecutionResult<TStepKey>` - 记录步骤执行结果
- `GetStepExecutionResult<TStepKey>` - 获取指定步骤的执行结果
- `GetAllStepExecutionResults<TStepKey>` - 获取所有步骤执行结果
- `Clear()` - 清空所有存储的数据

#### `TypedPipelineContext` 类
```csharp
public sealed class TypedPipelineContext : ITypedPipelineContext
```
`ITypedPipelineContext` 的默认实现，使用内部多存储机制实现类型安全的键值存储。

### 3. 对象池工具

#### `ArrayPool` 静态类
```csharp
public static class ArrayPool
{
    public static T[] Rent<T>(int minimumLength);
    public static void Return<T>(T[] array, bool clearArray = false);
    public static void Dispose();
}
```
数组对象池，用于减少高频数组分配产生的 GC 压力。

| 方法 | 说明 |
|------|------|
| `Rent<T>(minimumLength)` | 租借长度至少为 `minimumLength` 的数组 |
| `Return<T>(array, clearArray)` | 归还数组，可选择是否清空 |
| `Dispose()` | 释放对象池 |

#### `ListPool` 静态类
```csharp
public static class ListPool
{
    public static List<T> Rent<T>();
    public static void Return<T>(List<T> list);
    public static void Dispose();
}
```
列表对象池。

| 方法 | 说明 |
|------|------|
| `Rent<T>()` | 租借 `List<T>` 实例 |
| `Return<T>(list)` | 归还列表实例（自动清空） |
| `Dispose()` | 释放对象池 |

#### `DictionaryPool` 静态类
```csharp
public static class DictionaryPool
{
    public static Dictionary<TKey, TValue> Rent<TKey, TValue>();
    public static void Return<TKey, TValue>(Dictionary<TKey, TValue> dict);
    public static void Dispose();
}
```
字典对象池。

| 方法 | 说明 |
|------|------|
| `Rent<TKey, TValue>()` | 租借 `Dictionary<TKey, TValue>` 实例 |
| `Return<TKey, TValue>(dict)` | 归还字典实例（自动清空） |
| `Dispose()` | 释放对象池 |

### 4. 工具类

#### `OrchestratorUtility` 静态类
```csharp
public static class OrchestratorUtility
{
    public static bool ValidateNoCycles<TKey>(IEnumerable<IStep<TKey>> steps, out IEnumerable<TKey> cycleSteps);
    public static List<IStep<TKey>> TopologicalSort<TKey>(IEnumerable<IStep<TKey>> steps);
}
```
编排器工具类，提供依赖图分析和排序功能。

| 方法 | 说明 |
|------|------|
| `ValidateNoCycles` | 验证步骤依赖是否存在循环，存在时返回涉及的步骤标识 |
| `TopologicalSort` | 对步骤进行拓扑排序，返回按依赖顺序排列的步骤列表 |

---

## 二、`TaskOrchestrator<TKey>` - 基于 Task 的编排器

> 位于 `Orchestrator.Tasks` 命名空间

基于 `System.Threading.Tasks.Task` 的异步步骤编排器，适用于标准 .NET 异步场景。

### 核心类型

#### `ITaskStep<TKey>` 接口
```csharp
public interface ITaskStep<TKey> : IStep<TKey>
{
    Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token);
}
```
基于 Task 的异步步骤，继承自 `IStep<TKey>`，需实现 `ExecuteAsync` 方法定义步骤逻辑。

#### `ITaskBehavior<TKey>` 接口
```csharp
public interface ITaskBehavior<TKey>
{
    Task<StepResult> HandleAsync(ITypedPipelineContext context, TaskBehaviorStepper<TKey> stepper, CancellationToken token);
}
```
横切关注点行为接口，用于实现日志、监控、重试等 AOP 功能。

#### `TaskBehaviorStepper<TKey>` 结构体
```csharp
public readonly struct TaskBehaviorStepper<TKey>
{
    public Task<StepResult> NextAsync(CancellationToken token);
}
```
步进器，在行为链中调用下一个行为或最终步骤。

### 编排器 API

#### `TaskOrchestrator<TKey>` 类
```csharp
public sealed class TaskOrchestrator<TKey>
{
    public Task<ExecutionResult> ExecuteAsyncSequentially(ITypedPipelineContext context, CancellationToken token = default);
    public Task<ExecutionResult> ExecuteAsyncInParallel(ITypedPipelineContext context, CancellationToken token = default);
}
```

| 方法 | 说明 |
|------|------|
| `ExecuteAsyncSequentially` | 串行执行所有步骤，按拓扑顺序逐个执行 |
| `ExecuteAsyncInParallel` | 并行执行所有步骤，无依赖关系的步骤会并发执行 |

### Builder API

#### `TaskOrchestrator<TKey>.Builder` 类
```csharp
public class Builder
{
    public static Builder Create();
    public Builder AddStep(ITaskStep<TKey> step);
    public Builder AddBehavior<TStep>(ITaskBehavior<TKey> behavior) where TStep : ITaskStep<TKey>;
    public Builder AddBehavior<TStep1, TStep2>(ITaskBehavior<TKey> behavior);
    public Builder AddBehavior<TStep1, TStep2, TStep3>(ITaskBehavior<TKey> behavior);
    public Builder AddBehavior(ITaskBehavior<TKey> behavior, Type stepType);
    public Builder AddBehavior(ITaskBehavior<TKey> behavior, Type stepType1, Type stepType2);
    public Builder AddBehavior(ITaskBehavior<TKey> behavior, Type stepType1, Type stepType2, Type stepType3);
    public Builder AddBehavior(ITaskBehavior<TKey> behavior, params Type[] stepTypes);
    public Builder AddBehaviorForAll(ITaskBehavior<TKey> behavior);
    public Builder UsePolicy(InterruptionPolicy policy);
    public Builder WithMaxConcurrency(int count);
    public TaskOrchestrator<TKey> Build();
}
```

**Builder 方法说明：**

| 方法 | 说明 |
|------|------|
| `Create()` | 创建新的 Builder 实例（静态工厂方法） |
| `AddStep(step)` | 添加步骤，顺序不代表执行顺序，依赖关系自动分析 |
| `AddBehavior<TStep>(behavior)` | 为指定步骤类型添加行为 |
| `AddBehavior(behavior, stepTypes)` | 为多种步骤类型批量添加行为（泛型或 Type 参数重载） |
| `AddBehaviorForAll(behavior)` | 为当前所有已添加步骤添加行为 |
| `UsePolicy(policy)` | 设置中断策略，默认为 `DependencyBased` |
| `WithMaxConcurrency(count)` | 设置最大并发数，限制同时执行的步骤数量 |
| `Build()` | 构建编排器，执行依赖分析和拓扑排序，每个 Builder 只能调用一次 |

> 💡 **特别说明**：Builder 实例不可复用，调用 `Build()` 后不能再进行任何配置修改。

---

## 三、`ValueTaskOrchestrator<TKey>` - 基于 ValueTask 的编排器

> 位于 `Orchestrator.ValueTasks` 命名空间

基于 `System.Threading.Tasks.ValueTask` 的异步步骤编排器，适用于对性能敏感、可能同步完成的异步场景。

### 核心类型

#### `IValueTaskStep<TKey>` 接口
```csharp
public interface IValueTaskStep<TKey> : IStep<TKey>
{
    ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token);
}
```

#### `IValueTaskBehavior<TKey>` 接口
```csharp
public interface IValueTaskBehavior<TKey>
{
    ValueTask<StepResult> HandleAsync(ITypedPipelineContext context, ValueTaskBehaviorStepper<TKey> stepper, CancellationToken token);
}
```

#### `ValueTaskBehaviorStepper<TKey>` 结构体
```csharp
public readonly struct ValueTaskBehaviorStepper<TKey>
{
    public ValueTask<StepResult> NextAsync(CancellationToken token);
}
```

### 编排器 API

#### `ValueTaskOrchestrator<TKey>` 类
```csharp
public sealed class ValueTaskOrchestrator<TKey>
{
    public ValueTask<ExecutionResult> ExecuteAsyncSequentially(ITypedPipelineContext context, CancellationToken token = default);
}
```
> ⚠️ **注意**：当前 `ValueTaskOrchestrator` 仅提供串行执行方法，不支持并行执行。

### Builder API

#### `ValueTaskOrchestrator<TKey>.Builder` 类
```csharp
public class Builder
{
    public static Builder Create();
    public Builder AddStep(IValueTaskStep<TKey> step);
    public Builder AddBehavior<TStep>(IValueTaskBehavior<TKey> behavior) where TStep : IValueTaskStep<TKey>;
    // ... 其他 AddBehavior 重载（与 TaskOrchestrator 相同模式）
    public Builder AddBehaviorForAll(IValueTaskBehavior<TKey> behavior);
    public Builder UsePolicy(InterruptionPolicy policy);
    public Builder WithMaxConcurrency(int count);
    public ValueTaskOrchestrator<TKey> Build();
}
```

> 💡 Builder 的 `AddBehavior` 方法重载模式与 `TaskOrchestrator.Builder` 完全一致，支持 1-3 个泛型类型参数、Type 参数和 params 数组。

---

## 四、`UniTaskOrchestrator<TKey>` - 基于 UniTask 的编排器

> 位于 `Orchestrator.UniTasks` 命名空间，需要安装 `Cysharp.Threading.Tasks` 包

基于 Cysharp.Threading.Tasks.UniTask 的异步步骤编排器，适用于 Unity 环境或需要高性能异步的场景。

### 核心类型

#### `IUniTaskStep<TKey>` 接口
```csharp
public interface IUniTaskStep<TKey> : IStep<TKey>
{
    UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token);
}
```

#### `IUniTaskBehavior<TKey>` 接口
```csharp
public interface IUniTaskBehavior<TKey>
{
    UniTask<StepResult> HandleAsync(ITypedPipelineContext context, UniTaskBehaviorStepper<TKey> stepper, CancellationToken token);
}
```

#### `UniTaskBehaviorStepper<TKey>` 结构体
```csharp
public readonly struct UniTaskBehaviorStepper<TKey>
{
    public UniTask<StepResult> NextAsync(CancellationToken token);
}
```

### 编排器 API

#### `UniTaskOrchestrator<TKey>` 类
```csharp
public sealed class UniTaskOrchestrator<TKey>
{
    public UniTask<ExecutionResult> ExecuteAsyncSequentially(ITypedPipelineContext context, CancellationToken token = default);
    public UniTask<ExecutionResult> ExecuteAsyncInParallel(ITypedPipelineContext context, CancellationToken token = default);
}
```

| 方法 | 说明 |
|------|------|
| `ExecuteAsyncSequentially` | 串行执行所有步骤 |
| `ExecuteAsyncInParallel` | 并行执行所有步骤 |

### Builder API

#### `UniTaskOrchestrator<TKey>.Builder` 类
```csharp
public class Builder
{
    public static Builder Create();
    public Builder AddStep(IUniTaskStep<TKey> step);
    public Builder AddBehavior<TStep>(IUniTaskBehavior<TKey> behavior) where TStep : IUniTaskStep<TKey>;
    // ... 其他 AddBehavior 重载（与 TaskOrchestrator 相同模式）
    public Builder AddBehaviorForAll(IUniTaskBehavior<TKey> behavior);
    public Builder UsePolicy(InterruptionPolicy policy);
    public Builder WithMaxConcurrency(int count);
    public UniTaskOrchestrator<TKey> Build();
}
```

---

## 五、使用示例

### 示例 1：定义步骤和执行上下文

```csharp
using Orchestrator;
using Orchestrator.Tasks;

// 定义步骤标识类型
public enum StepKey
{
    ValidateInput,
    ProcessData,
    SaveResult,
    SendNotification
}

// 实现具体步骤
public class ValidateInputStep : ITaskStep<StepKey>
{
    public StepKey Key => StepKey.ValidateInput;
    public IReadOnlyCollection<IStep<StepKey>> Dependencies { get; } = Array.Empty<IStep<StepKey>>();

    public async Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        // 验证逻辑
        var isValid = await Task.FromResult(true);
        return isValid ? StepResult.Continue() : StepResult.Break();
    }
}

public class ProcessDataStep : ITaskStep<StepKey>
{
    public StepKey Key => StepKey.ProcessData;
    public IReadOnlyCollection<IStep<StepKey>> Dependencies { get; } = new[] { new ValidateInputStep() };

    public async Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        // 处理数据
        context.Set("processedData", new { Id = 1, Name = "Sample" });
        return StepResult.Continue();
    }
}

public class SaveResultStep : ITaskStep<StepKey>
{
    public StepKey Key => StepKey.SaveResult;
    public IReadOnlyCollection<IStep<StepKey>> Dependencies { get; } = new[] { new ProcessDataStep() };

    public async Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        var data = context.Get<string, object>("processedData");
        // 保存逻辑
        return StepResult.Continue();
    }
}
```

### 示例 2：构建和使用编排器

```csharp
public async Task RunWorkflowAsync()
{
    // 创建步骤实例
    var validateStep = new ValidateInputStep();
    var processStep = new ProcessDataStep();
    var saveStep = new SaveResultStep();

    // 构建编排器
    var orchestrator = TaskOrchestrator<StepKey>.Builder
        .Create()
        .AddStep(validateStep)
        .AddStep(processStep)
        .AddStep(saveStep)
        .UsePolicy(InterruptionPolicy.DependencyBased)
        .WithMaxConcurrency(2)
        .Build();

    // 创建上下文并执行
    var context = new TypedPipelineContext();
    var result = await orchestrator.ExecuteAsyncInParallel(context);

    Console.WriteLine($"执行结果: {(result.Success ? "成功" : "失败")}, 耗时: {result.Duration}");

    // 查看各步骤执行结果
    var stepResults = context.GetAllStepExecutionResults<StepKey>();
    foreach (var stepResult in stepResults)
    {
        Console.WriteLine($"步骤 {stepResult.StepKey} - 成功: {stepResult.Success}, 耗时: {stepResult.Duration}");
    }
}
```

### 示例 3：添加横切关注点（行为）

```csharp
// 日志行为
public class LoggingBehavior : ITaskBehavior<StepKey>
{
    public async Task<StepResult> HandleAsync(ITypedPipelineContext context, TaskBehaviorStepper<StepKey> stepper, CancellationToken token)
    {
        Console.WriteLine($"[开始] {DateTime.Now}");
        var result = await stepper.NextAsync(token);
        Console.WriteLine($"[结束] {DateTime.Now}, Flow: {result.Flow}");
        return result;
    }
}

// 重试行为
public class RetryBehavior : ITaskBehavior<StepKey>
{
    private readonly int _maxRetries = 3;

    public async Task<StepResult> HandleAsync(ITypedPipelineContext context, TaskBehaviorStepper<StepKey> stepper, CancellationToken token)
    {
        for (int i = 0; i < _maxRetries; i++)
        {
            var result = await stepper.NextAsync(token);
            if (result.Flow != StepFlow.Fail)
                return result;
            
            Console.WriteLine($"重试第 {i + 1} 次");
            await Task.Delay(100, token);
        }
        return StepResult.Fail(new Exception("重试次数耗尽"));
    }
}

// 使用行为
var orchestrator = TaskOrchestrator<StepKey>.Builder
    .Create()
    .AddStep(new ValidateInputStep())
    .AddBehavior<ValidateInputStep>(new LoggingBehavior())
    .AddBehavior<ValidateInputStep>(new RetryBehavior())
    .AddBehaviorForAll(new LoggingBehavior())  // 为所有步骤添加日志
    .Build();
```

### 示例 4：使用 UniTask 编排器（Unity 场景）

```csharp
using Orchestrator.UniTasks;
using Cysharp.Threading.Tasks;

public class UnityWorkflow : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        var orchestrator = UniTaskOrchestrator<string>.Builder
            .Create()
            .AddStep(new LoadAssetStep())
            .AddStep(new ProcessAssetStep())
            .AddStep(new RenderStep())
            .UsePolicy(InterruptionPolicy.DependencyBased)
            .Build();

        var context = new TypedPipelineContext();
        var result = await orchestrator.ExecuteAsyncSequentially(context, this.GetCancellationTokenOnDestroy());
        
        Debug.Log($"编排执行完成，成功: {result.Success}");
    }
}
```

### 示例 5：使用 ValueTask 编排器（高性能场景）

```csharp
using Orchestrator.ValueTasks;

public class FastPathStep : IValueTaskStep<int>
{
    public int Key => 1;
    public IReadOnlyCollection<IStep<int>> Dependencies { get; } = Array.Empty<IStep<int>>();

    public async ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        // 可能同步完成的快速操作
        context.Set("cache", "cached_value");
        return StepResult.Continue();
    }
}

public async Task HighPerformanceWorkflowAsync()
{
    var orchestrator = ValueTaskOrchestrator<int>.Builder
        .Create()
        .AddStep(new FastPathStep())
        .Build();

    var context = new TypedPipelineContext();
    var result = await orchestrator.ExecuteAsyncSequentially(context);
}
```

### 示例 6：结合对象池优化

```csharp
public async Task ProcessBatchAsync(IEnumerable<WorkItem> items)
{
    var orchestrator = BuildOrchestrator();
    
    foreach (var item in items)
    {
        // 复用上下文实例
        var context = new TypedPipelineContext();
        try
        {
            context.Set("workItem", item);
            var result = await orchestrator.ExecuteAsyncSequentially(context);
            
            if (!result.Success)
            {
                // 处理失败
            }
        }
        finally
        {
            context.Clear();  // 清空上下文以便复用
        }
    }
}
```

---

## 补充说明

| 项目 | 说明 |
|------|------|
| **命名空间** | 公共类型位于 `Orchestrator`，各异步实现位于 `Orchestrator.Tasks`、`Orchestrator.ValueTasks`、`Orchestrator.UniTasks` |
| **线程安全** | 编排器实例是线程安全的，可被多个并发调用共享；`TypedPipelineContext` 非线程安全，每个执行应使用独立实例 |
| **取消支持** | 所有执行方法均接受 `CancellationToken`，支持操作取消 |
| **对象池** | `ArrayPool`、`ListPool`、`DictionaryPool` 用于减少 GC，建议在应用退出时调用 `Dispose()` |