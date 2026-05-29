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

#### 标记接口

- `IProgressNode` / `IProgressLeaf`：非泛型版本，用于类型判断。

### 默认实现层 (`EasyProgress.Core`)

#### 节点类

- `DefaultLeaf`：默认叶子节点（`double` 进度值，范围 [0,1]），线程安全，实现 `IResettable`。
- `RealtimeComposite`：无权重实时组合节点，子节点变化立即重算。
- `ManualComposite`：无权重手动刷新组合节点，需调用 `Refresh()` 更新总进度。
- `WeightedRealtimeComposite`：加权实时组合节点，支持为每个子节点设置权重。
- `WeightedManualComposite`：加权手动刷新组合节点。

#### 规则类

- `WeightedAverageRule`：加权平均规则。
- `SequentialRule`：顺序规则（串行任务链）。
- `MaxRule` / `MinRule`：最大值 / 最小值规则。

所有规则类均提供静态工厂方法 `Create()`，返回全局单例。

#### 管理器

- `ILeafManager<T>` / `ICompositeManager<T>`：叶子/组合节点管理器接口。
- `IProgressManager<T>`：组合接口，同时提供叶子与组合节点管理。
- `DefaultProgressManager<T>`：默认管理器，内部使用 `PooledNodeManager` 实现对象池。

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

#### 扩展方法 (`Extension` 类)

- `ReleaseLeafChildren` / `ReleaseTree`：释放子节点或整个子树。
- `RunWithProgress` / `RunWithProgressAsync`：执行委托并自动管理临时叶子节点。
- `BeginProgress`：返回 `LeafScope<T>`，支持 `using` 作用域管理临时叶子节点。
- `BeginComposite`：返回 `CompositeScope<T>`，支持 `using` 作用域管理临时组合节点（自动释放整个子树）。
- `AddChildren` 重载：批量添加子节点（普通和加权）。

#### 对象池

- `ListPool` / `DictionaryPool`：静态工具类，提供 `Rent<T>()` 和 `Return<T>(List<T>)` 等方法，按类型隔离池。
- `PooledNodeManager<T, TNode>`：内部类，用于节点池化。