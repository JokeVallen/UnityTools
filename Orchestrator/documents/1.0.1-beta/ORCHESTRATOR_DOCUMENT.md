# Orchestrator API 文档

> 内容由 AI 根据核心代码生成，已通过人工审核。

## 命名空间

`Orchestrator`

---

## 枚举

### `StepFlow`

步骤执行后的流转状态。

| 成员 | 说明 |
|------|------|
| `Continue` | 正常继续，后续依赖步骤可执行 |
| `Break` | 业务中断，不视为错误，但阻断后续步骤 |
| `Fail` | 执行失败，触发异常处理 |

### `InterruptionPolicy`

工作流中断策略。

| 成员 | 说明 |
|------|------|
| `Strict` | 严格模式，任一步骤中断后全局停止 |
| `DependencyBased` | 依赖模式，仅阻断依赖该步骤的分支（默认） |
| `Ignore` | 忽略模式，尝试执行所有步骤 |

---

## 结构体

### `StepResult`

步骤执行结果，用于控制执行流向。

| API | 说明 |
|-----|------|
| `StepFlow Flow { get; }` | 流转状态 |
| `Exception Exception { get; }` | 捕获的异常（仅 Fail 时非空） |
| `static StepResult Continue()` | 创建继续执行的结果 |
| `static StepResult Break()` | 创建中断执行的结果 |
| `static StepResult Fail(Exception ex)` | 创建失败结果 |

### `StepExecutionResult<TKey>`

单个步骤的执行快照。

| API | 说明 |
|-----|------|
| `Optional<TKey> StepKey { get; }` | 步骤唯一标识（有值表示步骤被执行） |
| `bool Success { get; }` | 步骤是否成功（Flow == Continue） |
| `StepFlow Flow { get; }` | 步骤返回的流转状态 |
| `Exception Exception { get; }` | 执行异常（若有） |
| `TimeSpan Duration { get; }` | 步骤执行耗时 |

### `ExecutionResult<TKey>`

整个编排的执行结果。

| API | 说明 |
|-----|------|
| `bool Success { get; }` | 整体是否成功（无 Break/Fail 且无异常） |
| `IReadOnlyCollection<StepExecutionResult<TKey>> StepResults { get; }` | 所有已执行步骤的结果列表 |
| `TimeSpan Duration { get; }` | 总执行耗时 |

### `Optional<T>`

可选值包装器，安全处理可能缺失的值。

| API | 说明 |
|-----|------|
| `bool HasValue { get; }` | 是否包含有效值 |
| `T Value { get; }` | 获取值（若无值则抛出 `InvalidOperationException`） |
| `static Optional<T> None { get; }` | 表示无值的实例 |
| `static implicit operator Optional<T>(T value)` | 隐式转换为 Optional |
| `static explicit operator T(Optional<T> optional)` | 显式转换回 T |

### `TypedPipelineContext`

类型安全的键值存储上下文，实现 `ITypedPipelineContext`。

| API | 说明 |
|-----|------|
| `void Set<TKey, TValue>(TKey key, TValue value)` | 设置键值对 |
| `Optional<TValue> Get<TKey, TValue>(TKey key)` | 获取值（返回 Optional） |
| `bool Remove<TKey, TValue>(TKey key)` | 移除键值对 |
| `bool ContainsKey<TKey, TValue>(TKey key)` | 判断是否包含指定键 |
| `void Clear()` | 清空所有数据 |

---

## 接口

### `IStep<TKey>`

步骤标识接口，只负责图结构。

| API | 说明 |
|-----|------|
| `TKey Key { get; }` | 步骤唯一标识 |
| `IReadOnlyCollection<IStep<TKey>> Dependencies { get; }` | 依赖的步骤集合 |

### `ITypedPipelineContext`

强类型上下文接口，由 `TypedPipelineContext` 实现。

| API | 说明 |
|-----|------|
| `void Set<TKey, TValue>(TKey key, TValue value)` | 设置值 |
| `Optional<TValue> Get<TKey, TValue>(TKey key)` | 获取值 |
| `bool Remove<TKey, TValue>(TKey key)` | 移除值 |
| `bool ContainsKey<TKey, TValue>(TKey key)` | 判断是否存在 |
| `void Clear()` | 清空 |

---

## 静态类

### `OrchestratorUtility`

编排器工具类，提供图验证和排序功能。

| API | 说明 |
|-----|------|
| `static bool ValidateNoCycles<TKey>(IEnumerable<IStep<TKey>> steps, out IEnumerable<TKey> cycleSteps)` | 验证步骤依赖图是否无环，若存在循环则返回 false 并输出循环步骤 |
| `static List<IStep<TKey>> TopologicalSort<TKey>(IEnumerable<IStep<TKey>> steps)` | 对步骤进行拓扑排序，返回按依赖顺序排列的列表 |

### `ListPool`

`List<T>` 对象池。

| API | 说明 |
|-----|------|
| `static List<T> Rent<T>()` | 租借 List 实例 |
| `static void Return<T>(List<T> list)` | 归还 List 实例 |
| `static void Dispose()` | 释放对象池 |

### `DictionaryPool`

`Dictionary<TKey, TValue>` 对象池。

| API | 说明 |
|-----|------|
| `static Dictionary<TKey, TValue> Rent<TKey, TValue>()` | 租借 Dictionary 实例 |
| `static void Return<TKey, TValue>(Dictionary<TKey, TValue> dict)` | 归还 Dictionary 实例 |
| `static void Dispose()` | 释放对象池 |

### `ArrayPool`

数组对象池（按长度分桶存储，每桶最多 32 个数组）。

| API | 说明 |
|-----|------|
| `static T[] Rent<T>(int minimumLength)` | 租借数组（返回长度 ≥ minimumLength） |
| `static void Return<T>(T[] array, bool clearArray = false)` | 归还数组 |
| `static void Dispose()` | 释放对象池 |

---

## 使用示例

### 基础使用

```csharp
using Orchestrator;

// 1. 创建上下文
var context = new TypedPipelineContext();
context.Set("userId", 123);

// 2. 定义步骤（以 Task 版本为例，步骤接口实现在对应版本中）
public class LoadUserStep : ITaskStep<string>
{
    public string Key => "LoadUser";
    public IReadOnlyCollection<IStep<string>> Dependencies { get; }
        = Array.Empty<IStep<string>>();

    public async Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        var userId = context.Get<string, int>("userId").Value;
        var user = await LoadUserAsync(userId, token);
        context.Set("user", user);
        return StepResult.Continue();
    }
}

// 3. 构建并执行编排器（编排器在具体版本中）
var orchestrator = TaskOrchestrator<string>.Builder.Create()
    .AddStep(new LoadUserStep())
    .AddStep(new ProcessOrderStep())
    .Build();

var result = await orchestrator.ExecuteAsync(context);

// 4. 读取结果
var orderId = context.Get<string, int>("orderId");
if (orderId.HasValue)
    Console.WriteLine($"Order {orderId.Value} completed");
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

### 使用 Optional 安全读取

```csharp
// 方式1：检查 HasValue
var value = context.Get<string, int>("key");
if (value.HasValue)
    UseValue(value.Value);

// 方式2：使用默认值
var actual = context.Get<string, int>("key").HasValue 
    ? context.Get<string, int>("key").Value 
    : defaultValue;
```

### 对象池使用

```csharp
// 租用数组
var buffer = ArrayPool.Rent<byte>(1024);
try
{
    // 使用 buffer
}
finally
{
    ArrayPool.Return(buffer, clearArray: true);
}

// 租用 List
var list = ListPool.Rent<string>();
try
{
    list.Add("item");
}
finally
{
    ListPool.Return(list);
}

// 租用 Dictionary
var dict = DictionaryPool.Rent<string, int>();
try
{
    dict["key"] = 42;
}
finally
{
    DictionaryPool.Return(dict);
}
```

### 图验证与拓扑排序

```csharp
var steps = new List<IStep<string>> { stepA, stepB, stepC };

// 验证无环
if (!OrchestratorUtility.ValidateNoCycles(steps, out var cycles))
{
    Console.WriteLine($"Cycle detected: {string.Join(", ", cycles)}");
}

// 获取拓扑排序
var sorted = OrchestratorUtility.TopologicalSort(steps);
```

---