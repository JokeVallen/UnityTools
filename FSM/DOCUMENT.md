> 内容由 AI 根据核心代码生成，已通过人工审核。

## 框架层接口（FSM.Framework，命名空间 `FSM`）

框架层定义状态机的抽象契约，不包含具体实现。所有实现均依赖于这些接口。

### `IState<TKey, TContext>`
状态接口，表示一个可被状态机管理的状态单元。

```csharp
public interface IState<TKey, TContext> where TContext : class
{
    TKey Key { get; }                                // 状态唯一标识
    void Enter(TContext context);                     // 进入状态
    void Update(TContext context, TimeSpan deltaTime);// 状态更新
    void Exit(TContext context);                      // 退出状态
}
```

### `ITransition<TKey, TContext>`
转换接口，描述从源状态到目标状态的切换规则。

```csharp
public interface ITransition<TKey, TContext> where TContext : class
{
    TKey FromState { get; }          // 源状态标识
    TKey ToState { get; }            // 目标状态标识
    int Priority { get; }            // 优先级（越小越优先）
    string EventName { get; }        // 事件名：null 为自动转换，否则为事件驱动
    bool CanTransit(TContext context);// 转换条件
}
```

### `IStateMachine<TKey, TContext>`
状态机接口，管理状态、转换及运行时驱动。

```csharp
public interface IStateMachine<TKey, TContext> where TContext : class
{
    TContext Context { get; }
    IState<TKey, TContext> CurrentState { get; }
    bool IsRunning { get; }
    IReadOnlyList<IState<TKey, TContext>> States { get; }
    IReadOnlyList<ITransition<TKey, TContext>> Transitions { get; }

    event Action<IState<TKey, TContext>, IState<TKey, TContext>> OnStateChanged;
    event Action OnStarted;
    event Action OnStopped;

    void Start();
    void Update(TimeSpan deltaTime);
    void Stop();
    void Reset();
    void SendEvent(string eventName);
    void ForceTransition(TKey stateKey);
}
```

---

## 扩展层实现（FSM.Runtime，命名空间 `FSM.Runtime`）

扩展层基于上述接口提供了完整的默认实现，并添加了实用的扩展接口和工具类。

### `IExtendedTransition<TContext>`
继承 `ITransition<string, TContext>`，扩展了高级转换行为。

```csharp
public interface IExtendedTransition<TContext> : ITransition<string, TContext> where TContext : class
{
    TimeSpan? ExitTime { get; }  // 退出时间：状态必须运行的最短时长
    TimeSpan? Delay { get; }     // 转换延迟：条件满足后需稳定等待的时长
    bool IsOneShot { get; }      // 单次触发：为 true 时整个生命周期仅触发一次
}
```

### `Transition<TContext>`
实现 `IExtendedTransition<TContext>`，通过内部 `Builder` 创建。

**公共属性**（来自接口）：
- `string FromState`, `string ToState`
- `int Priority`
- `string EventName`
- `TimeSpan? ExitTime`, `TimeSpan? Delay`, `bool IsOneShot`
- `bool CanTransit(TContext context)`

**内嵌 Builder 类**：
```csharp
public class Builder
{
    public static Builder Create(string fromState, string toState);  // 入口
    public Builder When(Func<TContext, bool> condition);               // 条件
    public Builder WithPriority(int priority);
    public Builder WithExitTime(TimeSpan exitTime);
    public Builder WithDelay(TimeSpan delay);
    public Builder OneShot();
    public Builder Auto();                                            // 设为自动转换
    public Builder OnEvent(string eventName);                         // 设为事件转换
    public Transition<TContext> Build();                               // 构建
}
```

### `StateBase<TContext>`
抽象基类，实现 `IState<string, TContext>`，提供三个虚方法，子类可按需重写。

```csharp
public abstract class StateBase<TContext> : IState<string, TContext> where TContext : class
{
    public abstract string Key { get; }               // 状态名，必须实现
    public virtual void Enter(TContext context) { }
    public virtual void Update(TContext context, TimeSpan deltaTime) { }
    public virtual void Exit(TContext context) { }
}
```

### `StateBehaviour<TContext>`
继承 `StateBase<TContext>`，强制子类必须实现 `Update` 方法，适用于必须有更新逻辑的状态。

```csharp
public abstract class StateBehaviour<TContext> : StateBase<TContext> where TContext : class
{
    public abstract override void Update(TContext context, TimeSpan deltaTime);
}
```

### `StateMachine<TContext>`
实现 `IStateMachine<string, TContext>`，默认的状态机核心，通过内部 `Builder` 构建。

**内嵌 Builder 类**：
```csharp
public class Builder
{
    public static Builder Create();                                                    // 入口
    public Builder WithContext(TContext context);                                      // 设置共享上下文
    public Builder AddState(IState<string, TContext> state);                           // 注册状态
    public Builder AddTransition(Transition<TContext> transition);                     // 注册转换
    public Builder SetInitialState(string stateName);                                  // 设置初始状态
    public IStateMachine<string, TContext> Build();                                    // 构建状态机
}
```

构建时会严格校验状态及转换的完整性，并在非法配置时抛出 `StateMachineException`。

### `InnerStates`
内置状态常量，包含 `AnyState` 及名称校验方法。

```csharp
public static class InnerStates
{
    public const string AnyState = "__Any__";                  // 任意状态源标识
    public static bool IsInnerState(string name);              // 判断名称是否为保留字
}
```

### `StateMachineException`
框架统一异常类型，包含两个构造函数，用于传递错误消息和内部异常。

---

## 使用示例

下面展示一个简单的角色状态机：待机 ↔ 移动，以及死亡事件。

```csharp
using FSM.Runtime;

// 1. 上下文
public class PlayerContext
{
    public float Speed { get; set; }
    public float Health { get; set; } = 100;
    public bool IsDead => Health <= 0;
}

// 2. 状态
public class IdleState : StateBase<PlayerContext>
{
    public override string Key => "Idle";
    public override void Enter(PlayerContext ctx) => Console.WriteLine("进入待机");
}
public class RunState : StateBase<PlayerContext>
{
    public override string Key => "Run";
    public override void Enter(PlayerContext ctx) => Console.WriteLine("开始奔跑");
}
public class DeadState : StateBase<PlayerContext>
{
    public override string Key => "Dead";
    public override void Enter(PlayerContext ctx) => Console.WriteLine("角色死亡");
}

// 3. 构建状态机
var ctx = new PlayerContext();
var machine = StateMachine<PlayerContext>.Builder
    .Create()
    .WithContext(ctx)
    .AddState(new IdleState())
    .AddState(new RunState())
    .AddState(new DeadState())
    // 自动转换：空闲 ↔ 奔跑
    .AddTransition(Transition<PlayerContext>.Builder
        .Create("Idle", "Run").When(c => c.Speed > 0).Auto().Build())
    .AddTransition(Transition<PlayerContext>.Builder
        .Create("Run", "Idle").When(c => c.Speed <= 0).Auto().Build())
    // 事件转换：任意状态 → 死亡
    .AddTransition(Transition<PlayerContext>.Builder
        .Create(InnerStates.AnyState, "Dead").OnEvent("Die").Build())
    .SetInitialState("Idle")
    .Build();

// 4. 驱动
machine.Start();
machine.Update(TimeSpan.FromMilliseconds(16));  // 当前状态 Idle

ctx.Speed = 10;
machine.Update(TimeSpan.FromMilliseconds(16));  // 自动切换到 Run

machine.SendEvent("Die");                       // 强制进入 Dead
Console.WriteLine(machine.CurrentState.Key);    // Dead

machine.Stop();
```