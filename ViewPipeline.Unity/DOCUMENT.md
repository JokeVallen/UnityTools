# ViewPipeline - API 文档

> 内容由 AI 根据核心代码生成，已通过人工审核。

## 枚举类型

### PipelineDirection

表示管线当前的拓扑执行流向。

| 字段 | 说明 |
|------|------|
| `Open` | 打开/激活视图流向 |
| `Close` | 关闭/隐藏视图流向 |

### ValidationSeverity

验证器的严重等级。

| 字段 | 说明 |
|------|------|
| `Info` | 普通信息 |
| `Warning` | 警告 |
| `Error` | 错误 |


## 核心接口

### IView

最高层视口行为契约（最小原语：只有可显示、可隐藏两个原子动作）。

```csharp
public interface IView
{
    UniTask ShowAsync(CancellationToken cancellationToken);
    UniTask HideAsync(CancellationToken cancellationToken);
}
```

### IViewSession

视图会话接口，提供视图打开/关闭的核心功能。

```csharp
public interface IViewSession
{
    UniTask OpenViewAsync(IView view, CancellationToken cancellationToken);
    UniTask CloseViewAsync(IView view, CancellationToken cancellationToken);
}
```

### IExtendedViewSession

视图会话扩展接口，继承自 `IViewSession`、`IAsyncDisposable` 和 `ISessionKeyGetter`。

```csharp
public interface IExtendedViewSession : IViewSession, IAsyncDisposable, ISessionKeyGetter
{
    void RegisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider);
    void UnregisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider);
}
```

### IViewMiddleware

中间件接口，实现切面拦截或流转控制逻辑。

```csharp
public interface IViewMiddleware
{
    UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token);
}
```

### IDynamicMiddlewareProvider

动态中间件流式供应器接口。

```csharp
public interface IDynamicMiddlewareProvider
{
    void PopulateMiddlewares(IView view, IDynamicMiddlewareCollection dynamicMiddlewares);
}
```

### IDynamicMiddlewareCollection

中间件可动态增删集合接口。

```csharp
public interface IDynamicMiddlewareCollection : IEnumerable<IViewMiddleware>
{
    void Add(IViewMiddleware middleware);
}
```

### IValidatable

可验证接口，扩展包或中间件可实现此接口进行前置条件检查。

```csharp
public interface IValidatable
{
    IValidator GetValidator();
}
```

### IValidator

验证器接口。

```csharp
public interface IValidator
{
    ValidationResult Validate();
}
```

### IExecutionPolicy

执行策略接口，用于跳过或终止中间件执行。

```csharp
public interface IExecutionPolicy
{
    bool ShouldSkipMiddleware(IView view, IViewMiddleware middleware);
    bool ShouldSkipView(IViewMiddleware middleware, IView view);
    bool ShouldTerminate(IView view);
    bool ShouldTerminate(IViewMiddleware middleware);
}
```

### ISkippableView

附带跳过中间件处理功能的视图。

```csharp
public interface ISkippableView
{
    bool ShouldSkip(IViewMiddleware middleware);
}
```

### ISkippableMiddleware

附带跳过视图功能的中间件。

```csharp
public interface ISkippableMiddleware
{
    bool ShouldSkip(IView view);
}
```

### ITerminable

可终止执行流程的功能接口。

```csharp
public interface ITerminable
{
    bool ShouldTerminate();
}
```

### IExtension

扩展包接口，用于批量装配中间件和供应器。

```csharp
public interface IExtension
{
    bool IsInitialized { get; }
    void Initialize();
    IEnumerable<IViewMiddleware> GetMiddlewares(PipelineDirection direction);
    IEnumerable<IDynamicMiddlewareProvider> GetDynamicProviders(PipelineDirection direction);
}
```

### IPipelineContext

执行管道上下文接口（标记接口，用于传递自定义数据）。

```csharp
public interface IPipelineContext { }
```

### ITypedPipelineContext

强类型管道上下文接口，提供类型安全的键值存储能力。

```csharp
public interface ITypedPipelineContext : IPipelineContext
{
    void Set<TKey, TValue>(TKey key, TValue value);
    Optional<TValue> Get<TKey, TValue>(TKey key);
    bool Remove<TKey, TValue>(TKey key);
    bool ContainsKey<TKey, TValue>(TKey key);
    void Clear();
}
```

### IPipelineContextCollection

管道上下文集合接口，管理上下文实例的获取与归还。

```csharp
public interface IPipelineContextCollection
{
    IPipelineContext Acquire();
    void Return(IPipelineContext context);
}
```

### IPipelineSession

管道会话接口，记录执行状态。

```csharp
public interface IPipelineSession : ISessionKeyGetter
{
    bool IsTerminalReached { get; }
    bool IsAborted { get; }
    PipelineDirection Direction { get; }
}
```

### ISessionKeyGetter

会话唯一标识访问接口。

```csharp
public interface ISessionKeyGetter
{
    Guid Key { get; }
}
```

### IResettable

可重置能力接口。

```csharp
public interface IResettable
{
    void Reset();
}
```

### IAsyncDisposable

异步释放资源接口。

```csharp
public interface IAsyncDisposable
{
    UniTask DisposeAsync();
}
```

### ILogger

日志记录器接口。

```csharp
public interface ILogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
}
```


## 核心结构体

### Optional\<T\>

可选值包装器，明确区分「无值」和「值为默认值」。

```csharp
public readonly struct Optional<T> : IEquatable<Optional<T>>
{
    public bool HasValue { get; }
    public T Value { get; }
    
    public static Optional<T> None { get; }
    public static implicit operator Optional<T>(T value);
    public static explicit operator T(Optional<T> optional);
    public static bool operator ==(Optional<T> left, Optional<T> right);
    public static bool operator !=(Optional<T> left, Optional<T> right);
    
    public T GetValueOrDefault();
    public T GetValueOrDefault(T defaultValue);
}
```

### ViewPipelineExecutor

管道执行器，作为中间件的流转控制参数传递。

```csharp
public readonly struct ViewPipelineExecutor
{
    public int CurrentIndex { get; }
    public IPipelineContext Context { get; }
    public IPipelineSession Session { get; }
    
    public UniTask NextAsync(IView view, CancellationToken token);
    public void Abort();
}
```

### ValidationResult

验证结果结构体。

```csharp
public readonly struct ValidationResult
{
    public bool IsValid { get; }
    public string Message { get; }
    public ValidationSeverity Severity { get; }
    
    public static ValidationResult Success();
    public static ValidationResult Error(string msg);
    public static ValidationResult Warning(string msg);
}
```


## 快照系统

### SnapshotCache

快照缓存类。

```csharp
public static class SnapshotCache
{
    public static event Action<Guid, Type> OnRefresh;
    
    public static void Store<TSnapshot>(Guid key, TSnapshot snapshot);
    public static bool TryGet<TSnapshot>(Guid key, out TSnapshot snapshot);
    public static TSnapshot Get<TSnapshot>(Guid key);
    public static bool Exists<TSnapshot>(Guid key);
    public static void Remove<TSnapshot>(Guid key);
    public static void RemoveAll(Guid key);
    public static void Clear();
    public static void Refresh<TSnapshot>(Guid key);
    public static void Refresh(Guid key);
}
```

### SnapshotCache\<TTag\>

带标签的快照缓存类。

```csharp
public static class SnapshotCache<TTag>
{
    public static event Action<Guid, Optional<TTag>, Type> OnRefresh;
    
    public static void Store<TSnapshot>(Guid key, TSnapshot snapshot, TTag tag);
    public static bool TryGet<TSnapshot>(Guid key, out TSnapshot snapshot, TTag tag);
    public static TSnapshot Get<TSnapshot>(Guid key, TTag tag);
    public static void Refresh<TSnapshot>(Guid key, TTag tag);
}
```

### 快照类型

| 快照类型 | 说明 |
|----------|------|
| `ViewSessionBuilderSnapshot` | 构建器快照，包含 `ContextType`、中间件列表、扩展包列表等 |
| `ViewSessionSnapshot` | 会话快照，包含扩展包状态、管线快照、活跃操作数 |
| `ViewPipelineEngineSnapshot` | 引擎快照，包含静态中间件、动态供应器、当前中间件列表 |
| `ViewPipelineExecutorSnapshot` | 执行器快照，包含当前索引、会话快照 |
| `PipelineSessionSnapshot` | 管道会话快照，包含执行进度、方向、中断状态 |
| `MiddlewareSnapshot` | 中间件快照，包含中间件类型 |
| `ExtensionSnapshot` | 扩展包快照，包含扩展包类型和初始化状态 |
| `DynamicMiddlewareProviderSnapshot` | 动态供应器快照，包含供应器类型 |


## 会话注册表

### ViewSessionRegistry

全局会话注册表，可查看所有活跃会话。

```csharp
public static class ViewSessionRegistry
{
    public static IReadOnlyDictionary<Guid, IViewSession> Sessions { get; }
    public static UniTask DisposeAsync();
    public static void Clear();
}
```


## 构建器

### ViewSessionBuilder

视图会话构建器，支持流式 API 配置。

```csharp
public sealed class ViewSessionBuilder : ISessionKeyGetter, IFullSnapshotable<ViewSessionBuilderSnapshot>
{
    public Guid Key { get; }
    public bool Built { get; }
    
    public static ViewSessionBuilder Create();
    
    /// <summary>自定义管道上下文工厂方法（泛型版本，自动记录类型）</summary>
    public ViewSessionBuilder WithContextFactory<TContextType>(Func<IPipelineContext> contextFactory)
        where TContextType : IPipelineContext;
    
    /// <summary>使用强类型可读写上下文（推荐）</summary>
    public ViewSessionBuilder WithTypedContext();
    
    /// <summary>自定义管道上下文集合</summary>
    public ViewSessionBuilder WithContextCollection<TContextType>(IPipelineContextCollection contextCollection)
        where TContextType : IPipelineContext;
    
    /// <summary>自定义动态中间件集合工厂方法</summary>
    public ViewSessionBuilder WithDynamicMiddlewareCollectionFactory(Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory);
    
    /// <summary>向打开管线添加静态中间件</summary>
    public ViewSessionBuilder AddOpenMiddleware(IViewMiddleware middleware);
    
    /// <summary>向打开管线添加动态中间件流式供应器</summary>
    public ViewSessionBuilder AddOpenDynamicProvider(IDynamicMiddlewareProvider provider);
    
    /// <summary>向关闭管线添加静态中间件</summary>
    public ViewSessionBuilder AddCloseMiddleware(IViewMiddleware middleware);
    
    /// <summary>向关闭管线添加动态中间件流式供应器</summary>
    public ViewSessionBuilder AddCloseDynamicProvider(IDynamicMiddlewareProvider provider);
    
    /// <summary>添加扩展包</summary>
    public ViewSessionBuilder AddExtension(IExtension extension);
    
    /// <summary>自定义日志记录器</summary>
    public ViewSessionBuilder WithLogger(ILogger logger);
    
    /// <summary>自定义中间件执行策略</summary>
    public ViewSessionBuilder WithMiddlewareExecutionPolicy(IExecutionPolicy executionPolicy);
    
    /// <summary>构建视图会话实例</summary>
    public IExtendedViewSession Build();
    
    /// <summary>获取构建器快照</summary>
    public ViewSessionBuilderSnapshot GetFullSnapshot();
}
```


## 扩展方法

### ViewPipelineExecutor 扩展方法

```csharp
public static class TypedPipelineContextExtensions
{
    /// <summary>获取强类型上下文（不支持时抛出异常）</summary>
    public static ITypedPipelineContext GetTypedContext(this ViewPipelineExecutor executor);
    
    /// <summary>尝试获取强类型上下文</summary>
    public static bool TryGetTypedContext(this ViewPipelineExecutor executor, out ITypedPipelineContext typedContext);
    
    /// <summary>设置数据（不支持时抛出异常）</summary>
    public static void SetData<T>(this ViewPipelineExecutor executor, string key, T value);
    
    /// <summary>获取数据（不支持时抛出异常）</summary>
    public static Optional<T> GetData<T>(this ViewPipelineExecutor executor, string key);
    
    /// <summary>移除数据（不支持时抛出异常）</summary>
    public static bool RemoveData<T>(this ViewPipelineExecutor executor, string key);
    
    /// <summary>尝试设置数据（不抛出异常）</summary>
    public static bool TrySetData<T>(this ViewPipelineExecutor executor, string key, T value);
    
    /// <summary>尝试获取数据（不抛出异常）</summary>
    public static Optional<T> TryGetData<T>(this ViewPipelineExecutor executor, string key);
    
    /// <summary>尝试移除数据（不抛出异常）</summary>
    public static bool TryRemoveData<T>(this ViewPipelineExecutor executor, string key);
    
    /// <summary>检查是否包含指定键（不支持时抛出异常）</summary>
    public static bool ContainsKey<T>(this ViewPipelineExecutor executor, string key);
    
    /// <summary>尝试检查是否包含指定键（不抛出异常）</summary>
    public static bool TryContainsKey<T>(this ViewPipelineExecutor executor, string key);
}
```


## 使用示例

### 基础用法

```csharp
public class MyView : IView
{
    public async UniTask ShowAsync(CancellationToken cancellationToken)
    {
        gameObject.SetActive(true);
        await UniTask.CompletedTask;
    }
    
    public async UniTask HideAsync(CancellationToken cancellationToken)
    {
        gameObject.SetActive(false);
        await UniTask.CompletedTask;
    }
}

var session = ViewSessionBuilder.Create().Build();
await session.OpenViewAsync(new MyView(), CancellationToken.None);
```

### 中间件示例

```csharp
public class LoggingMiddleware : IViewMiddleware
{
    public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
    {
        Debug.Log($"[Logging] Before: {view.GetType().Name}");
        await executor.NextAsync(view, token);
        Debug.Log($"[Logging] After: {view.GetType().Name}");
    }
}

var session = ViewSessionBuilder.Create()
    .AddOpenMiddleware(new LoggingMiddleware())
    .Build();
```

### 强类型上下文

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

### 扩展包实现

```csharp
public class UIExtension : IExtension, IValidatable
{
    private readonly Guid _builderKey;
    
    public UIExtension(Guid builderKey) => _builderKey = builderKey;
    
    public bool IsInitialized { get; private set; }
    public void Initialize() => IsInitialized = true;
    
    public IEnumerable<IViewMiddleware> GetMiddlewares(PipelineDirection direction)
    {
        if (direction == PipelineDirection.Open)
            yield return new UILoadingMiddleware();
    }
    
    public IEnumerable<IDynamicMiddlewareProvider> GetDynamicProviders(PipelineDirection direction)
        => Array.Empty<IDynamicMiddlewareProvider>();
    
    public IValidator GetValidator()
        => new UIExtensionValidator(_builderKey);
}

// 使用
var session = ViewSessionBuilder.Create()
    .WithTypedContext()
    .AddExtension(new UIExtension(builder.Key))
    .Build();
```

### 扩展包验证器

```csharp
public class UIExtensionValidator : IValidator
{
    private readonly Guid _builderKey;
    
    public UIExtensionValidator(Guid builderKey) => _builderKey = builderKey;
    
    public ValidationResult Validate()
    {
        var snapshot = SnapshotCache.Get<ViewSessionBuilderSnapshot>(_builderKey);
        if (!typeof(ITypedPipelineContext).IsAssignableFrom(snapshot.ContextType))
        {
            return ValidationResult.Error("This extension requires ITypedPipelineContext");
        }
        return ValidationResult.Success();
    }
}
```


## 版本信息

当前版本：1.0.1-beta