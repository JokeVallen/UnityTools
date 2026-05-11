> 内容由 AI 根据核心代码生成，已通过人工审核。

## 框架层 API

### `EasyAttribute`
- **签名**：`public abstract class EasyAttribute : Attribute`
- **作用**：所有自定义属性的基类，提供 `Enabled`（是否启用）和 `Priority`（优先级，数值越小越先执行）属性。

### `IContext`
- **签名**：`public interface IContext`
- **作用**：执行上下文，携带当前属性实例、共享状态字典 `Items`、功能扩展槽 `Features`、启用状态快照和优先级快照。

### `IProcessor`
- **签名**：`public interface IProcessor`
- **作用**：同步处理器契约，定义 `Before` → `Process` → `After` 生命周期。

### `IProcessorAsync`
- **签名**：`public interface IProcessorAsync`
- **作用**：异步处理器契约，定义 `BeforeAsync` → `ProcessAsync` → `AfterAsync` 生命周期。

### `IProcessorHandle`
- **签名**：`public interface IProcessorHandle`
- **作用**：处理器返回的执行句柄，控制是否中止 (`IsAborted`)、是否跳过 `After` 回调 (`SkipAfterCallbacks`) 以及中止时的替代结果 (`Result`)。

### `IExecutor`
- **签名**：`public interface IExecutor`  
  `IProcessorHandle Execute(IContext context)`
- **作用**：同步执行器，驱动处理器链。

### `IExecutorAsync`
- **签名**：`public interface IExecutorAsync`  
  `Task<IProcessorHandle> ExecuteAsync(IContext context, CancellationToken cancellationToken = default)`
- **作用**：异步执行器，支持取消。

### `IFeature`
- **签名**：`public interface IFeature`
- **作用**：功能标记接口，实现此接口的类型可注入到 `Features` 中。

---

## 扩展层 API

### 场景上下文接口

#### `IMethodContext`
- **签名**：`public interface IMethodContext : IContext`
- **作用**：方法上下文，提供 `MethodInfo`、参数列表、参数值、`Proceed` 委托、返回值、异常。

#### `IPropertyContext`
- **签名**：`public interface IPropertyContext : IContext`
- **作用**：属性上下文，提供 `PropertyInfo`、访问器 (`Get`/`Set`)、属性值、原始 Getter/Setter 委托。

#### `IFieldContext`
- **签名**：`public interface IFieldContext : IContext`
- **作用**：字段上下文。

#### `IParameterContext`
- **签名**：`public interface IParameterContext : IContext`
- **作用**：参数上下文，提供所在方法、参数元数据、索引、当前值。

#### `IReturnValueContext`
- **签名**：`public interface IReturnValueContext : IContext`
- **作用**：返回值上下文，提供方法元数据、返回参数、返回值。

#### `ITypeContext`
- **签名**：`public interface ITypeContext : IContext`
- **作用**：类型上下文，提供被标注的类型。

#### `IConstructorContext`
- **签名**：`public interface IConstructorContext : IContext`
- **作用**：构造函数上下文。

#### `IEventContext`
- **签名**：`public interface IEventContext : IContext`
- **作用**：事件上下文，提供事件元数据、访问器 (`Add`/`Remove`)、处理程序委托、原始 Add/Remove 委托。

#### `IGenericParameterContext`
- **签名**：`public interface IGenericParameterContext : IContext`
- **作用**：泛型参数上下文，提供泛型参数类型、声明成员。

### 异步上下文

#### `IAsyncContext`
- **签名**：`public interface IAsyncContext`
- **作用**：提供 `CancellationToken`，异步处理器可通过此接口获取取消令牌。

### 处理器基类（同步）

- **`Processor<TAttr>`**：通用同步处理器基类。
- **`MethodProcessor<TAttr>`**：方法专用处理器，当上下文为 `IMethodContext` 时执行。
- **`PropertyProcessor<TAttr>`**：属性专用处理器。
- **`FieldProcessor<TAttr>`**、**`ParameterProcessor<TAttr>`**、**`ReturnValueProcessor<TAttr>`**、**`TypeProcessor<TAttr>`**、**`ConstructorProcessor<TAttr>`**、**`EventProcessor<TAttr>`**、**`GenericParameterProcessor<TAttr>`** 同理。

### 处理器基类（异步）

- **`AsyncProcessor<TAttr>`**
- **`AsyncMethodProcessor<TAttr>`**、**`AsyncPropertyProcessor<TAttr>`** 等 9 种异步场景处理器。

### 上下文工厂

#### `ContextFactory`
- **签名**：`public static class ContextFactory`
- **作用**：提供一系列静态方法创建各场景上下文，返回强类型接口。
- **方法**：
  - `CreateMethodContext(EasyAttribute, MethodInfo, object target, object[] arguments, ...)`
  - `CreatePropertyContext(...)`, `CreateFieldContext(...)`, `CreateTypeContext(...)`, `CreateConstructorContext(...)`, `CreateParameterContext(...)`, `CreateReturnValueContext(...)`, `CreateEventContext(...)`, `CreateGenericParameterContext(...)`

### 构建器

#### `DefaultExecutorBuilder`
- **签名**：`public sealed class DefaultExecutorBuilder`
- **作用**：链式配置处理器注册、全局功能注入、工厂和异常处理器，最终构建同步/异步执行器。
- **关键方法**：
  - `UseProcessor<TAttr, TProcessor>()`
  - `UseFeature<TFeature>(TFeature)`
  - `Scan(Assembly)`
  - `UseFactory(IProcessorFactory)`
  - `UseExceptionHandler(IExceptionHandler)`
  - `Build()` / `BuildAsync()`

### 处理器句柄

#### `ProcessorHandle`
- **签名**：`public sealed class ProcessorHandle : IProcessorHandle`
- **作用**：提供 `Continue`、`Aborted`、`AbortedAll` 单例，以及 `Abort(object)`、`AbortAll(object)` 工厂方法。

### 异常体系

- **`EasyAttributeException`**：框架异常基类。
- **`ProcessorException`** / **`ProcessorBeforeException`** / **`ProcessorExecuteException`** / **`ProcessorAfterException`**：处理器阶段异常。
- **`ExecutorException`** / **`ProcessorNotFoundException`**：执行器异常。
- **`ContextException`** / **`FeatureTypeException`**：上下文异常。

### 扩展方法

- `GetItem<T>`, `TryGetItem<T>`, `SetItem`, `RemoveItem`：Items 读写。
- `GetFeature<T>`, `TryGetFeature<T>`：Features 读取。
- `GetResult<T>`, `TryGetResult<T>`：句柄结果提取。

---

## 使用示例

### 定义属性与处理器
```csharp
public class LogAttribute : EasyAttribute { }

public class LogMethodProcessor : MethodProcessor<LogAttribute>
{
    protected override IProcessorHandle Process(IMethodContext context, LogAttribute attr)
    {
        Console.WriteLine($"调用 {context.Method.Name}");
        return ProcessorHandle.Continue;
    }
}
```

### 构建执行器
```csharp
var executor = DefaultExecutorBuilder.Create()
    .UseProcessor<LogAttribute, LogMethodProcessor>()
    .Build();
```

### 拦截调用
```csharp
var context = ContextFactory.CreateMethodContext(new LogAttribute(), method, target, args);
var handle = executor.Execute(context);
if (!handle.IsAborted)
{
    var result = method.Invoke(target, args);
}
```