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
| `Error` | 错误：阻止构建 |
| `Warning` | 仅警告，不阻止构建 |

---

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
    /// <summary>将一个已经构建完毕、填充好数据的 View 纳入架构管线并激活显示。</summary>
    UniTask OpenViewAsync(IView view, CancellationToken cancellationToken);
    
    /// <summary>将指定的 View 从架构管线中移出并隐藏。</summary>
    UniTask CloseViewAsync(IView view, CancellationToken cancellationToken);
}
```

### IExtendedViewSession

视图会话扩展接口，继承自 `IViewSession` 和 `IAsyncDisposable`。

```csharp
public interface IExtendedViewSession : IViewSession, IAsyncDisposable
{
    /// <summary>唯一标识符</summary>
    Guid Key { get; }
    
    /// <summary>注册动态中间件流式供应器</summary>
    void RegisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider);
    
    /// <summary>注销动态中间件流式供应器</summary>
    void UnregisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider);
}
```

### IViewMiddleware

中间件接口，实现切面拦截或流转控制逻辑。

```csharp
public interface IViewMiddleware
{
    /// <summary>
    /// 异步执行中间件的切面拦截或流转控制逻辑
    /// </summary>
    /// <param name="view">当前操作的视图实例</param>
    /// <param name="executor">当前管线的流转驱动器</param>
    /// <param name="token">异步取消令牌</param>
    UniTask InvokeAsync(IView view, UIPipelineExecutor executor, CancellationToken token);
}
```

### IDynamicMiddlewareProvider

动态中间件流式供应器接口。

```csharp
public interface IDynamicMiddlewareProvider
{
    /// <summary>
    /// 根据当前操作的视图上下文，向运行时中间件可动态增删集合中追加属于本扩展包的动态切面组件
    /// </summary>
    /// <param name="view">当前操作的视图实例</param>
    /// <param name="staticMiddlewares">静态中间件只读集合</param>
    /// <param name="dynamicMiddlewares">动态中间件收纳集合</param>
    void PopulateMiddlewares(IView view, IReadOnlyList<IViewMiddleware> staticMiddlewares, IDynamicMiddlewareCollection dynamicMiddlewares);
}
```

### IDynamicMiddlewareCollection

中间件可动态增删集合接口。

```csharp
public interface IDynamicMiddlewareCollection : IEnumerable<IViewMiddleware>
{
    /// <summary>添加中间件</summary>
    void Add(IViewMiddleware middleware);
}
```

### IMiddlewareValidator

验证器接口。

```csharp
public interface IMiddlewareValidator
{
    /// <summary>执行验证</summary>
    /// <param name="middlewares">静态中间件数组</param>
    /// <param name="errors">错误集合</param>
    void Validate(IReadOnlyCollection<IViewMiddleware> middlewares, IList<ValidationError> errors);
}
```

### IMiddlewareExecutionPolicy

中间件执行策略接口。

```csharp
public interface IMiddlewareExecutionPolicy
{
    /// <summary>判断指定视图是否应跳过指定中间件</summary>
    bool ShouldSkip(IView view, IViewMiddleware middleware);
}
```

### IExtension

扩展包接口，用于批量装配中间件、供应器和验证器。

```csharp
public interface IExtension
{
    /// <summary>获取静态中间件</summary>
    IEnumerable<IViewMiddleware> GetMiddlewares(PipelineDirection direction);
    
    /// <summary>获取动态中间件供应器</summary>
    IEnumerable<IDynamicMiddlewareProvider> GetDynamicProviders(PipelineDirection direction);
    
    /// <summary>获取中间件验证器</summary>
    IEnumerable<IMiddlewareValidator> GetMiddlewareValidators();
    
    /// <summary>初始化</summary>
    void Initialize();
}
```

### IViewRegistry

视图注册表接口，管理当前活跃视图。

```csharp
public interface IViewRegistry : IReadOnlyCollection<IView>
{
    void Register(IView view);
    void Unregister(IView view);
}
```

### IViewStackPolicy

视图层级组织与导航栈管理策略接口。

```csharp
public interface IViewStackPolicy : IReadOnlyCollection<IView>
{
    void Push(IView view);
    void Pop(IView view);
}
```

### IPipelineContext

执行管道上下文接口（标记接口，用于传递自定义数据）。

```csharp
public interface IPipelineContext { }
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
public interface IPipelineSession
{
    /// <summary>管道是否已执行完成</summary>
    bool IsTerminalReached { get; }
    
    /// <summary>管道是否已中断执行</summary>
    bool IsAborted { get; }
    
    /// <summary>管道执行方向</summary>
    PipelineDirection Direction { get; }
}
```

### IResstable

可重置能力接口。

```csharp
public interface IResstable
{
    void Reset();
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

### IAsyncDisposable

异步释放资源接口。

```csharp
public interface IAsyncDisposable
{
    UniTask DisposeAsync();
}
```

---

## 核心结构体

### UIPipelineExecutor

管道执行器，作为中间件的流转控制参数传递。

```csharp
public readonly struct UIPipelineExecutor
{
    /// <summary>当前索引</summary>
    public int CurrentIndex { get; }
    
    /// <summary>执行管道上下文</summary>
    public IPipelineContext Context { get; }
    
    /// <summary>管道会话实例</summary>
    public IPipelineSession Session { get; }
    
    /// <summary>异步步进到下一个阶段</summary>
    public UniTask NextAsync(IView view, CancellationToken token);
    
    /// <summary>中断执行</summary>
    public void Abort();
}
```

### ValidationError

验证错误信息结构体。

```csharp
public readonly struct ValidationError
{
    public string Message { get; }
    public ValidationSeverity Severity { get; }
    
    public ValidationError(string message, ValidationSeverity severity);
    public override string ToString();
}
```

---

## 构建器

### ViewSessionBuilder

视图会话构建器，支持流式 API 配置。

```csharp
public sealed class ViewSessionBuilder
{
    /// <summary>唯一标识</summary>
    public Guid Key { get; }
    
    /// <summary>构建器已执行构建</summary>
    public bool Built { get; }
    
    /// <summary>创建一个配置流式构建器</summary>
    public static ViewSessionBuilder Create();
    
    /// <summary>自定义视图注册表</summary>
    public ViewSessionBuilder WithRegistry(IViewRegistry registry);
    
    /// <summary>自定义导航栈策略</summary>
    public ViewSessionBuilder WithStackPolicy(IViewStackPolicy stackPolicy);
    
    /// <summary>自定义管道上下文工厂方法</summary>
    public ViewSessionBuilder WithContextFactory(Func<IPipelineContext> contextFactory);
    
    /// <summary>自定义管道上下文集合</summary>
    public ViewSessionBuilder WithContextCollection(IPipelineContextCollection contextCollection);
    
    /// <summary>自定义动态中间件集合工厂方法</summary>
    public ViewSessionBuilder WithDynamicMiddlewareCollectionFactory(Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory);
    
    /// <summary>向激活/打开管线注册静态中间件</summary>
    public ViewSessionBuilder AddOpenMiddleware(IViewMiddleware middleware);
    
    /// <summary>向激活/打开管线注册动态中间件流式供应器</summary>
    public ViewSessionBuilder AddOpenDynamicProvider(IDynamicMiddlewareProvider provider);
    
    /// <summary>向隐藏/关闭管线注册静态中间件</summary>
    public ViewSessionBuilder AddCloseMiddleware(IViewMiddleware middleware);
    
    /// <summary>向隐藏/关闭管线注册动态中间件流式供应器</summary>
    public ViewSessionBuilder AddCloseDynamicProvider(IDynamicMiddlewareProvider provider);
    
    /// <summary>构建 UI 会话实例</summary>
    public IExtendedViewSession Build();
}
```

---

## 扩展方法（Extension 类）

```csharp
public static partial class Extension
{
    /// <summary>自定义日志记录器</summary>
    public static ViewSessionBuilder WithLogger(this ViewSessionBuilder builder, ILogger logger);
    
    /// <summary>添加扩展包</summary>
    public static ViewSessionBuilder AddExtension(this ViewSessionBuilder builder, IExtension extension);
    
    /// <summary>设置中间件执行策略</summary>
    public static ViewSessionBuilder SetMiddlewareExecutionPolicy(this ViewSessionBuilder builder, IMiddlewareExecutionPolicy executionPolicy);
}
```

---

## 使用示例

### 基础用法：最简单的视图会话

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using ViewPipeline.Unity;
using ViewPipeline.Unity.Core;

public class MyView : IView
{
    public async UniTask ShowAsync(CancellationToken cancellationToken)
    {
        // 显示 UI 的逻辑
        gameObject.SetActive(true);
        await UniTask.CompletedTask;
    }
    
    public async UniTask HideAsync(CancellationToken cancellationToken)
    {
        gameObject.SetActive(false);
        await UniTask.CompletedTask;
    }
}

// 创建并打开视图
var session = ViewSessionBuilder.Create().Build();
await session.OpenViewAsync(new MyView(), CancellationToken.None);
```

### 中间件示例：日志记录器

```csharp
public class LoggingMiddleware : IViewMiddleware
{
    public async UniTask InvokeAsync(IView view, UIPipelineExecutor executor, CancellationToken token)
    {
        Debug.Log($"[Logging] Before: {view.GetType().Name}");
        await executor.NextAsync(view, token);
        Debug.Log($"[Logging] After: {view.GetType().Name}");
    }
}

// 注册
var session = ViewSessionBuilder.Create()
    .AddOpenMiddleware(new LoggingMiddleware())
    .Build();
```

### 动态中间件供应器

```csharp
public class DynamicAnalyticsProvider : IDynamicMiddlewareProvider
{
    private readonly AnalyticsMiddleware _middleware = new AnalyticsMiddleware();
    
    public void PopulateMiddlewares(IView view, IReadOnlyList<IViewMiddleware> staticMiddlewares, IDynamicMiddlewareCollection dynamicMiddlewares)
    {
        // 只为特定类型的视图添加埋点中间件
        if (view is IPageView)
        {
            dynamicMiddlewares.Add(_middleware);
        }
    }
}

// 注册
var session = ViewSessionBuilder.Create()
    .AddOpenDynamicProvider(new DynamicAnalyticsProvider())
    .Build();
```

### 自定义上下文

```csharp
public class MyCustomContext : IPipelineContext, IResstable
{
    public Dictionary<string, object> Data { get; } = new Dictionary<string, object>();
    
    public void Reset()
    {
        Data.Clear();
    }
}

var session = ViewSessionBuilder.Create()
    .WithContextFactory(() => new MyCustomContext())
    .Build();
```

### 扩展包实现

```csharp
public class UIExtension : IExtension
{
    public IEnumerable<IViewMiddleware> GetMiddlewares(PipelineDirection direction)
    {
        yield return new UILoadingMiddleware();
        if (direction == PipelineDirection.Open)
            yield return new UIOpenAnalyticsMiddleware();
    }
    
    public IEnumerable<IDynamicMiddlewareProvider> GetDynamicProviders(PipelineDirection direction)
    {
        yield return new DynamicPermissionProvider();
    }
    
    public IEnumerable<IMiddlewareValidator> GetMiddlewareValidators()
    {
        yield return new NoDuplicateValidators();
    }
    
    public void Initialize()
    {
        Debug.Log("UI Extension initialized");
    }
}

// 一键装配
var session = ViewSessionBuilder.Create()
    .AddExtension(new UIExtension())
    .Build();
```

### 自定义验证器

```csharp
public class NoDuplicateValidators : IMiddlewareValidator
{
    public void Validate(IReadOnlyCollection<IViewMiddleware> middlewares, IList<ValidationError> errors)
    {
        var types = middlewares.Select(m => m.GetType()).ToList();
        var duplicates = types.GroupBy(t => t).Where(g => g.Count() > 1);
        
        foreach (var dup in duplicates)
        {
            errors.Add(new ValidationError(
                $"Duplicate middleware type: {dup.Key}", 
                ValidationSeverity.Warning));
        }
    }
}
```

### 执行策略

```csharp
public class FeatureFlagPolicy : IMiddlewareExecutionPolicy
{
    private readonly HashSet<string> _disabledFeatures;
    
    public bool ShouldSkip(IView view, IViewMiddleware middleware)
    {
        return _disabledFeatures.Contains(middleware.GetType().Name);
    }
}

var session = ViewSessionBuilder.Create()
    .AddOpenMiddleware(new NewFeatureMiddleware())
    .SetMiddlewareExecutionPolicy(new FeatureFlagPolicy())
    .Build();
```