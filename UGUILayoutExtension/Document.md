# EventHub.Unity API 参考文档

## 命名空间

`EventHub.Unity`

---

## 接口

### IAsyncEventDispatcher

异步事件分发器接口，提供异步订阅、取消订阅和发布事件的方法。

```csharp
public interface IAsyncEventDispatcher
{
    ISubscription SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
    int UnsubscribeAsync<TEvent>(Func<TEvent, CancellationToken, UniTask> handler);
    UniTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
}
```

#### 成员

| 名称 | 说明 |
|------|------|
| `SubscribeAsync<TEvent>` | 订阅异步事件。返回 `ISubscription` 令牌，可用于取消订阅。 |
| `UnsubscribeAsync<TEvent>` | 通过委托取消异步订阅。返回移除的订阅数量（通常为 0 或 1）。 |
| `PublishAsync<TEvent>` | 异步串行发布事件，按优先级顺序执行所有订阅者，等待每个完成后再执行下一个。自动切换到主线程。 |

#### 参数

- `handler`：事件处理器，类型为 `Func<TEvent, CancellationToken, UniTask>`。
- `priority`：优先级，数值越大越先执行，默认 0。
- `@event`：事件实例。
- `cancellationToken`：取消令牌，用于提前终止发布。

---

### ISyncEventDispatcher

同步事件分发器接口，提供同步订阅、取消订阅和发布事件的方法。

```csharp
public interface ISyncEventDispatcher
{
    ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0);
    int Unsubscribe<TEvent>(Action<TEvent> handler);
    void Publish<TEvent>(TEvent @event);
}
```

#### 成员

| 名称 | 说明 |
|------|------|
| `Subscribe<TEvent>` | 订阅同步事件。返回 `ISubscription` 令牌。 |
| `Unsubscribe<TEvent>` | 通过委托取消同步订阅。返回移除的订阅数量。 |
| `Publish<TEvent>` | 同步发布事件，按优先级顺序执行所有订阅者。**注意：不会自动切换到主线程，请确保在主线程调用。** |

---

### IEventDispatcher

复合接口，同时继承 `IAsyncEventDispatcher` 和 `ISyncEventDispatcher`。

```csharp
public interface IEventDispatcher : IAsyncEventDispatcher, ISyncEventDispatcher
{
}
```

---

### ISubscription

订阅令牌接口，用于取消订阅和查询订阅信息。实现 `IDisposable`。

```csharp
public interface ISubscription : IDisposable
{
    void Unsubscribe();
    bool IsDisposed { get; }
    Type EventType { get; }
    int Priority { get; }
}
```

#### 成员

| 名称 | 说明 |
|------|------|
| `Unsubscribe()` | 取消订阅，等效于 `Dispose()`。 |
| `IsDisposed` | 指示该订阅是否已被取消/释放。 |
| `EventType` | 订阅的事件类型。 |
| `Priority` | 订阅时的优先级。 |

---

### ILogger

日志记录器接口，用于自定义错误输出。

```csharp
public interface ILogger
{
    bool Enabled { get; set; }
    void LogError(Type eventType, Delegate handler, Exception exception);
    void LogWarning(string message);
    void LogInfo(string message);
}
```

#### 成员

| 名称 | 说明 |
|------|------|
| `Enabled` | 是否启用日志记录。 |
| `LogError` | 记录错误，参数为事件类型、处理器委托和异常。 |
| `LogWarning` | 记录警告信息。 |
| `LogInfo` | 记录普通信息。 |

---

### IInterruptHandle

中断句柄接口，用于 `PublishUntilInterrupt` 方法。事件类需实现此接口以支持中断。

```csharp
public interface IInterruptHandle
{
    bool IsInterrupted { get; }
    void Interrupt();
}
```

#### 成员

| 名称 | 说明 |
|------|------|
| `IsInterrupted` | 是否已被中断。 |
| `Interrupt()` | 中断后续订阅者的执行。 |

---

## 静态类 EventDispatcher

提供全局静态入口，内部通过 `Dispatcher` 属性委托给具体实现。

```csharp
public static class EventDispatcher
```

### 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| `Dispatcher` | `IEventDispatcher` | 获取或设置当前使用的事件分发器实例。默认内部实现为 `EventDispatcherInternal`。 |
| `Logger` | `ILogger` | 获取或设置日志记录器。默认使用 Unity 的 `Debug` 输出。 |

### 事件

| 名称 | 说明 |
|------|------|
| `OnError` | 当订阅者抛出异常时触发。参数依次为：事件类型、引发异常的委托、异常对象。 |

### 方法

#### 发布

| 方法 | 说明 |
|------|------|
| `Publish<TEvent>(TEvent @event)` | 同步发布事件。**必须主线程调用**。 |
| `PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)` | 异步串行发布，自动主线程切换。 |
| `PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)` | 异步并行发布，所有订阅者并发执行，异常会聚合抛出。 |
| `PublishParallelSilentAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)` | 异步并行静默发布，异常被捕获并记录到 `ILogger`，不向外抛出。 |
| `PublishUntilInterrupt<TEvent>(TEvent @event) where TEvent : IInterruptHandle` | 同步中断发布，按优先级执行，直到某个订阅者调用 `Interrupt()`。 |

#### 订阅

| 方法 | 说明 |
|------|------|
| `Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)` | 同步订阅，返回 `ISubscription`。 |
| `SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)` | 异步订阅，返回 `ISubscription`。 |
| `SubscribeOnce<TEvent>(Action<TEvent> handler, int priority = 0)` | 一次性同步订阅，执行一次后自动取消。 |
| `SubscribeOnce<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)` | 一次性异步订阅。 |
| `SubscribeIf<TEvent>(Predicate<TEvent> filter, Action<TEvent> handler, int priority = 0)` | 条件同步订阅，仅当 `filter` 返回 `true` 时执行 `handler`。 |
| `SubscribeIf<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)` | 条件异步订阅。 |

#### 取消订阅

| 方法 | 说明 |
|------|------|
| `Unsubscribe<TEvent>(Action<TEvent> handler)` | 取消同步订阅，返回移除的数量。 |
| `UnsubscribeAsync<TEvent>(Func<TEvent, CancellationToken, UniTask> handler)` | 取消异步订阅，返回移除的数量。 |

---

## 类 EventHubBehaviour

MonoBehaviour 组件，用于自动管理订阅生命周期。当组件销毁时，会自动取消所有通过它注册的订阅。

```csharp
public sealed class EventHubBehaviour : MonoBehaviour
```

### 方法

| 名称 | 说明 |
|------|------|
| `Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)` | 同步订阅，订阅令牌自动存储，销毁时自动取消。 |
| `SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)` | 异步订阅，自动管理生命周期。 |
| `UnsubscribeAll()` | 立即取消所有通过此组件注册的订阅。 |

---

## 静态类 UnityExtension

为 `MonoBehaviour` 提供扩展方法，自动添加/获取 `EventHubBehaviour` 组件，实现订阅生命周期自动绑定。

```csharp
public static class UnityExtension
```

### 方法

| 名称 | 说明 |
|------|------|
| `Subscribe<TEvent>(this MonoBehaviour behaviour, Action<TEvent> handler, int priority = 0)` | 为 `MonoBehaviour` 订阅同步事件，对象销毁时自动取消。 |
| `SubscribeAsync<TEvent>(this MonoBehaviour behaviour, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)` | 为 `MonoBehaviour` 订阅异步事件，自动管理生命周期。 |
| `UnsubscribeAll(this MonoBehaviour behaviour)` | 取消该 `MonoBehaviour` 上通过扩展方法注册的所有订阅。 |

---

## 注意事项

- 所有异步发布方法都会通过 `UniTask.SwitchToMainThread()` 自动切换到主线程，可安全访问 Unity API。
- 同步发布 `Publish` 不会切换线程，请确保在 Unity 主线程调用，否则可能引发异常。
- 优先级数值越大越先执行，相同优先级按订阅顺序执行。
- 一次性订阅和条件订阅在内部通过扩展方法实现，接口未强制要求，但默认实现已提供。
- 若需完全替换事件系统，请在项目设置中定义 `EVENTHUB_EXTENSION_ENABLE` 宏，并提供自己的 `IEventDispatcher` 实现，赋值给 `EventDispatcher.Dispatcher`。

---

## 示例

请参考项目 README 中的快速开始部分。