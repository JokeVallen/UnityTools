> 内容由 AI 根据核心代码生成，已通过人工审核。

## 公共 API 简介

### 框架层 (`EasyProgress`)

#### `IProgressNode<T>`

```csharp
public interface IProgressNode<T>
{
    T Progress { get; }
    event Action<IProgressNode<T>, T> OnProgressChanged;
}
```

- **作用**：进度节点的基本抽象，提供进度值获取和变化事件。

#### `IProgressLeaf<T> : IProgressNode<T>`

```csharp
public interface IProgressLeaf<T> : IProgressNode<T>
{
    void Report(T value);
    void Complete();
}
```

- **作用**：可主动报告进度的叶子节点，通常代表一个具体任务。

**示例**：
```csharp
IProgressLeaf<double> leaf = new DefaultLeaf();
leaf.OnProgressChanged += (node, p) => Console.WriteLine($"Progress: {p:P}");
leaf.Report(0.3);
leaf.Complete();
```

#### 标记接口

- `IProgressNode` / `IProgressLeaf`：非泛型版本，用于类型判断。

### 默认实现层 (`EasyProgress.Core`)

#### 节点类

##### `DefaultLeaf`

```csharp
public sealed class DefaultLeaf : IProgressLeaf<double>, IResettable
```

**示例**：
```csharp
var leaf = new DefaultLeaf();
leaf.Report(0.5);
Console.WriteLine(leaf.Progress); // 0.5
leaf.Complete();
leaf.Reset();
```

##### `RealtimeComposite`

```csharp
public sealed class RealtimeComposite : IProgressComposite<double>, IResettable
```

**示例**：
```csharp
var composite = new RealtimeComposite(WeightedAverageRule.Create());
var leaf = new DefaultLeaf();
composite.AddChild(leaf);
leaf.Report(0.6); // composite.Progress 立即变为 0.6
```

##### `ManualComposite`

```csharp
public sealed class ManualComposite : IProgressComposite<double>, IResettable, IManualRefreshNode
```

**示例**：
```csharp
var composite = new ManualComposite(WeightedAverageRule.Create());
var leaf = new DefaultLeaf();
composite.AddChild(leaf);
leaf.Report(0.8);
Console.WriteLine(composite.Progress); // 0 (尚未刷新)
composite.Refresh();
Console.WriteLine(composite.Progress); // 0.8
```

##### `WeightedRealtimeComposite`

```csharp
public sealed class WeightedRealtimeComposite : IProgressComposite<double>, IWeightedProgressComposite<double>, IResettable
```

**示例**：
```csharp
var composite = new WeightedRealtimeComposite(WeightedAverageRule.Create());
var leaf1 = new DefaultLeaf();
var leaf2 = new DefaultLeaf();
composite.AddChild(leaf1, 0.3f);
composite.AddChild(leaf2, 0.7f);
leaf1.Report(1.0);
leaf2.Report(0.5);
Console.WriteLine(composite.Progress); // 0.3*1 + 0.7*0.5 = 0.65
```

##### `WeightedManualComposite`

```csharp
public sealed class WeightedManualComposite : IProgressComposite<double>, IWeightedProgressComposite<double>, IResettable, IManualRefreshNode
```

**示例**：
```csharp
var composite = new WeightedManualComposite(WeightedAverageRule.Create());
var leaf = new DefaultLeaf();
composite.AddChild(leaf, 0.9f);
leaf.Report(0.2);
composite.Refresh();
Console.WriteLine(composite.Progress); // 0.18
```

#### 规则类

##### `WeightedAverageRule`

```csharp
public sealed class WeightedAverageRule : ICompositionRule<double>
```

**示例**：
```csharp
var rule = WeightedAverageRule.Create();
var children = new IProgressNode<double>[] { leaf1, leaf2 };
var result = rule.Compute(children, n => n == leaf1 ? 0.3f : 0.7f);
```

##### `SequentialRule`

```csharp
public sealed class SequentialRule : ICompositionRule<double>
```

**示例**：
```csharp
var rule = SequentialRule.Create();
// 假设 leaf1 已完成(1.0), leaf2 进度 0.5
// 权重分别为 0.6, 0.4
var result = rule.Compute(children, getWeight); 
// 返回 0.6 + 0.5*0.4 = 0.8
```

##### `MaxRule` / `MinRule`

```csharp
public sealed class MaxRule : ICompositionRule<double>
public sealed class MinRule : ICompositionRule<double>
```

**示例**：
```csharp
var rule = MaxRule.Create();
var result = rule.Compute(children, _ => 1f); // 返回子节点最大进度
```

#### 静态入口 `Progress`

```csharp
public static class Progress
{
    public static IProgressManager<T> GetProgressManager<T>();
    public static ILeafManager<T> GetLeafManager<T>();
    public static ICompositeManager<T> GetCompositeManager<T>();

    public static IProgressLeaf<T> CreateLeaf<T>();
    public static IProgressComposite<T> CreateComposite<T>(ICompositionRule<T> rule);
    public static IWeightedProgressComposite<T> CreateWeightedComposite<T>(ICompositionRule<T> rule);

    public static void ReleaseLeaf<T>(IProgressLeaf<T> leaf);
    public static void ReleaseComposite<T>(IProgressComposite<T> composite);

    public static void RegisterProgressManager<T>(IProgressManager<T> progressManager);
    public static void RegisterProgressManager(Type type, IProgressManager progressManager);
    public static void UnregisterProgressManager<T>();
    public static void UnregisterProgressManager(Type type);

    public static void Dispose();
}
```

**示例**：
```csharp
// 使用默认管理器（double）
var leaf = Progress.CreateLeaf<double>();
leaf.Report(0.3);
Progress.ReleaseLeaf(leaf);

var composite = Progress.CreateComposite(WeightedAverageRule.Create());
Progress.ReleaseComposite(composite);
```

#### 扩展方法 (`Extension` 类)

##### `ReleaseLeafChildren` / `ReleaseTree`

```csharp
public static void ReleaseLeafChildren<T>(this IProgressComposite<T> composite);
public static void ReleaseTree<T>(this IProgressComposite<T> composite);
```

**示例**：
```csharp
var root = Progress.CreateComposite(rule);
// ... 构建树
root.ReleaseTree(); // 释放整个子树
```

##### `RunWithProgress` / `RunWithProgressAsync`

```csharp
public static void RunWithProgress<T>(this IProgressComposite<T> composite, Action<IProgressLeaf<T>> work);
public static Task RunWithProgressAsync<T>(this IProgressComposite<T> composite, Func<IProgressLeaf<T>, Task> work);
```

**示例**：
```csharp
composite.RunWithProgress(leaf =>
{
    for (int i = 0; i < 10; i++)
        leaf.Report((i+1)/10.0);
    leaf.Complete();
});
```

##### `BeginProgress` / `BeginComposite`

```csharp
public static LeafScope<T> BeginProgress<T>(this IProgressComposite<T> composite);
public static CompositeScope<T> BeginComposite<T>(this IProgressComposite<T> parent, ICompositionRule<T> rule);
```

**示例**：
```csharp
using (var scope = composite.BeginProgress())
{
    scope.Report(0.5);
} // 自动清理

using (var scope = parent.BeginComposite(SequentialRule.Create()))
{
    scope.Composite.AddChild(leaf);
} // 自动释放整个子树
```

##### `AddChildren` 重载

```csharp
public static void AddChildren<T>(this IProgressComposite<T> composite, params IProgressNode<T>[] nodes);
public static void AddChildren<T>(this IWeightedProgressComposite<T> composite, params (IProgressNode<T> node, float weight)[] weightedNodes);
```

**示例**：
```csharp
composite.AddChildren(leaf1, leaf2);
weightedComposite.AddChildren((leaf1, 0.3f), (leaf2, 0.7f));
```

#### 对象池

- `ListPool` / `DictionaryPool`：静态工具类，通常内部使用，用户无需直接调用。