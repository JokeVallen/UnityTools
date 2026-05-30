## 1.0.1-beta

### 修改

#### 状态接口（IState<TKey, TContext>）

- 修改定义为 `public interface IContextState<TKey, TContext> where TContext : class`。

#### 状态转换接口（ITransition<TKey, TContext>）

- 修改定义为 `public interface IContextTransition<TKey, TEvent, TContext> where TContext : class`。

#### 状态机接口（IStateMachine<TKey, TContext>）

- 修改定义为 `public interface IContextStateMachine<TKey, TEvent, TContext> where TContext : class`。

#### 附带上下文的状态机系列

- 移除对上下文的泛型约束。

#### 状态转换扩展接口（IExtendedTransition\<TContext>）

- 修改定义为 `public interface IExtendContextTransition<TKey, TEvent, TContext> : IContextTransition<TKey, TEvent, TContext> where TContext : class`。

#### 内置状态标识（InnerStates）

- 修改定义为 `public static class InnerStates<TKey>`。
- 补充内置赋值状态检测，若获取未赋值的内置状态标识将触发异常 `System.InvalidOperationException`。

#### 状态基类（StateBase\<TContext>）

- 修改定义为 `public abstract class ContextStateBase<TKey, TContext> : IContextState<TKey, TContext>, IRestable, IRestableWithContext<TContext> where TContext : class`。
- 实现 `IResetable` 接口。
- 实现 `IResetableWithContext` 接口。

#### 状态行为基类（StateBehaviour\<TContext>）

- 修改定义为 `public abstract class ContextStateBehaviour<TKey, TContext> : ContextStateBase<TKey, TContext> where TContext : class`。

#### 状态机基类（StateMachine\<TContext>）

- 修改定义为 `public class ContextStateMachine<TKey, TEvent, TContext> : IContextStateMachine<TKey, TEvent, TContext>, IRestable where TContext : class`。
- 实现 `IResetable` 接口。

#### 状态转换基类（Transition\<TContext>）

- 修改定义为 `public class ContextTransition<TKey, TEvent, TContext> : IExtendContextTransition<TKey, TEvent, TContext>, IRestable where TContext : class`。
- 方法 `ResetRuntimeStates()` 改名为 `Reset()`。
- 实现 `IResetable` 接口。

### 添加

- 添加接口 `public interface IState<TKey>`。
- 添加接口 `public interface ITransition<TKey, TEvent>`。
- 添加接口 `public interface IStateMachine<TKey, TEvent>`。
- 添加非泛型统一接口 `public interface IState`。
- 添加非泛型统一接口 `public interface IStateMachine`。
- 添加非泛型统一接口 `public interface ITransition`。
- 添加类 `public abstract class StateBase<TKey> : IState<TKey>, IRestable`。
- 添加类 `public abstract class StateBehaviour<TKey> : StateBase<TKey>`。
- 添加类 `public class StateMachine<TKey, TEvent> : IStateMachine<TKey, TEvent>, IRestable`。
- 添加类 `public class Transition<TKey, TEvent> : IExtendTransition<TKey, TEvent>, IRestable`。
- 添加接口 `public interface IExtendTransition<TKey, TEvent> : ITransition<TKey, TEvent>`。
- 添加接口 `public interface IRestable`。
- 添加接口 `public interface IRestableWithContext<TContext>`。

> TKey 表示状态标识的类型，TEvent 表示事件标识的类型。

### 说明

- 划分附带上下文和无上下文状态机，并对命名重新进行了规范。
- 已将原本的状态标识从默认的 `string` 改为通用泛型 `TKey`。
- 已将原本的事件标识从默认的 `string` 改为通用泛型 `TEvent`。
- 原本的状态机系列默认附带上下文，本次版本划分为附带上下文和无上下文两种状态机系列。
- 命名重新规范。

