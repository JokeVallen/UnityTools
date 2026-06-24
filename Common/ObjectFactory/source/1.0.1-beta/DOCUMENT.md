> 内容由 AI 根据核心代码生成，已通过人工审核。

---

## 一、公共 API 简介

### 1. `ObjectFactory` 静态类

全局工厂注册与解析中心。所有工厂的注册和获取都通过此类完成。**不提供任何内置默认工厂，必须先注册后使用。**

#### 方法

| 方法签名 | 说明 |
|---------|------|
| `static void RegisterCreator<T>(Func<T> callback) where T : class, IObjectFactory` | 使用委托注册泛型工厂的创建方法。`callback` 不能为 `null`。 |
| `static void RegisterCreator<T>(IFactoryCreator<T> creator) where T : class, IObjectFactory` | 使用 `IFactoryCreator<T>` 注册泛型工厂的创建器。`creator` 不能为 `null`。 |
| `static void RegisterCreator(Type factoryType, Func<IObjectFactory> callback)` | 使用委托注册非泛型工厂的创建方法。`factoryType` 必须实现 `IObjectFactory`，`callback` 不能为 `null`。 |
| `static void RegisterCreator(IFactoryCreator creator)` | 使用 `IFactoryCreator` 注册非泛型工厂的创建器。`creator.FactoryType` 必须有效且实现 `IObjectFactory`。 |
| `static T GetFactory<T>() where T : class, IObjectFactory` | 获取指定类型的工厂实例。若未注册则抛出 `InvalidOperationException`。 |
| `static IObjectFactory GetFactory(Type factoryType)` | 获取指定类型的工厂实例（非泛型）。若 `factoryType` 无效则记录错误并返回 `null`；若未注册则抛出异常。 |
| `static bool TryGetFactory<T>(out T factory) where T : class, IObjectFactory` | 尝试获取工厂实例，成功返回 `true`，否则返回 `false`。 |
| `static bool TryGetFactory(Type factoryType, out IObjectFactory factory)` | 尝试获取工厂实例（非泛型）。若 `factoryType` 无效则记录错误并返回 `false`；未注册返回 `false`。 |
| `static void ClearCreators()` | 清除所有已注册的工厂创建器，同时清空泛型与非泛型存储（用于测试或热重载）。 |

---

### 2. `GameObjectFactory` 类

默认的 `GameObject` 工厂实现，提供多种创建重载。

| 构造函数 | 说明 |
|---------|------|
| `GameObjectFactory(bool throwOnError = false)` | 创建工厂实例。`throwOnError` 控制初始化回调异常时是否抛出异常。 |

| 方法 | 说明 |
|------|------|
| `GameObject Create(Action<GameObject> initialize = null)` | 创建无名称的空 `GameObject`，可选初始化回调。 |
| `GameObject Create<TArg>(TArg arg, Action<GameObject, TArg> initialize = null)` | 创建无名称的空 `GameObject`，传递自定义参数给初始化回调。 |
| `GameObject Create(string name, Action<GameObject> initialize = null)` | 创建指定名称的 `GameObject`。 |
| `GameObject Create<TArg>(string name, TArg arg, Action<GameObject, TArg> initialize = null)` | 创建指定名称的 `GameObject`，传递自定义参数。 |
| `GameObject Create(string name, Action<GameObject> initialize = null, params Type[] components)` | 创建指定名称的 `GameObject`，并预先添加指定类型的组件。 |
| `GameObject Create<TArg>(string name, TArg arg, Action<GameObject, TArg> initialize = null, params Type[] components)` | 创建指定名称的 `GameObject`，传递自定义参数，并预先添加组件。 |

> **异常行为**：若 `initialize` 抛出异常，且 `throwOnError` 为 `true`，则异常向上抛出；若为 `false`，则对象被销毁，返回 `null` 并记录错误日志。

---

### 3. `ComponentFactory` 类

默认的组件工厂实现，提供向现有 `GameObject` 添加组件的多种重载。

| 构造函数 | 说明 |
|---------|------|
| `ComponentFactory(bool throwOnError = false)` | 创建工厂实例。`throwOnError` 控制初始化回调异常时是否抛出异常。 |

| 方法 | 说明 |
|------|------|
| `T Create<T>(GameObject gameObject, Action<T> initialize = null) where T : Component` | 在 `gameObject` 上添加泛型组件 `T`，可选初始化回调。 |
| `T Create<T, TArg>(GameObject gameObject, TArg arg, Action<T, TArg> initialize = null) where T : Component` | 在 `gameObject` 上添加泛型组件，传递自定义参数给初始化回调。 |
| `Component Create(GameObject gameObject, Type type, Action<Component> initialize = null)` | 在 `gameObject` 上添加指定类型的组件（非泛型）。 |
| `Component Create(GameObject gameObject, Type type, object arg, Action<Component, object> initialize = null)` | 在 `gameObject` 上添加指定类型的组件，传递自定义参数。 |

> **异常行为**：若 `gameObject` 或 `type` 为 `null`，或 `type` 不是 `Component` 派生类，且 `throwOnError` 为 `true`，则抛出相应异常；否则记录错误并返回 `null`。若 `initialize` 抛出异常，行为同 `GameObjectFactory`。

---

### 4. `IFactoryCreator` / `IFactoryCreator<T>` 接口

用于高级注册场景，允许将工厂的创建逻辑封装为独立对象。通常直接使用 `Func<T>` 即可。

| 接口 | 成员 | 说明 |
|------|------|------|
| `IFactoryCreator` | `Type FactoryType { get; }` <br> `IObjectFactory Create()` | 返回工厂类型，并创建工厂实例（非泛型）。 |
| `IFactoryCreator<T>` | `T Create()` | 创建工厂实例（泛型）。 |

---

## 二、使用示例

### 示例 1：注册并使用默认工厂

```csharp
// 启动时注册（全局执行一次）
ObjectFactory.RegisterCreator<GameObjectFactory>(() => new GameObjectFactory());
ObjectFactory.RegisterCreator<ComponentFactory>(() => new ComponentFactory());

// 使用时获取
var goFactory = ObjectFactory.GetFactory<GameObjectFactory>();
GameObject enemy = goFactory.Create("Enemy", go => {
    go.transform.position = Vector3.zero;
    go.AddComponent<Rigidbody>();
});

var compFactory = ObjectFactory.GetFactory<ComponentFactory>();
Rigidbody rb = compFactory.Create<Rigidbody>(enemy, r => r.mass = 10f);
```

### 示例 2：自定义工厂（继承或新建）

```csharp
public class PooledGameObjectFactory : GameObjectFactory
{
    public PooledGameObjectFactory(bool throwOnError = false) : base(throwOnError) { }
    // 可重写或扩展创建逻辑
}

// 注册自定义工厂
ObjectFactory.RegisterCreator<GameObjectFactory>(() => new PooledGameObjectFactory(true));
var factory = ObjectFactory.GetFactory<GameObjectFactory>();
```

### 示例 3：使用 `IFactoryCreator<T>` 注册

```csharp
public class MyFactoryCreator : IFactoryCreator<GameObjectFactory>
{
    public GameObjectFactory Create() => new GameObjectFactory(true);
}

ObjectFactory.RegisterCreator<GameObjectFactory>(new MyFactoryCreator());
var factory = ObjectFactory.GetFactory<GameObjectFactory>();
```

### 示例 4：带参数的初始化回调

```csharp
var goFactory = new GameObjectFactory();
float speed = 5f;
GameObject player = goFactory.Create(speed, (go, spd) => {
    go.name = "Player";
    var mover = go.AddComponent<PlayerMover>();
    mover.Speed = spd;
});

var compFactory = new ComponentFactory();
int health = 100;
var healthComp = compFactory.Create<HealthComponent, int>(player, health, (hc, hp) => {
    hc.MaxHealth = hp;
    hc.CurrentHealth = hp;
});
```

### 示例 5：异常处理策略

```csharp
var strictFactory = new GameObjectFactory(throwOnError: true);
try
{
    GameObject go = strictFactory.Create("Fail", g => throw new Exception("Init error"));
}
catch (Exception ex)
{
    Debug.LogError($"创建失败: {ex.Message}");
}
```

### 示例 6：测试环境清理

```csharp
[SetUp]
public void SetUp()
{
    ObjectFactory.ClearCreators(); // 清空所有注册，保证测试隔离
}
```

---

## 三、注意事项

1. **线程安全**：`ObjectFactory` 内部使用 `ConcurrentDictionary`，注册和获取操作是线程安全的。
2. **零 GC 分配**：使用泛型注册和静态方法作为初始化回调时，工厂本身不会产生额外堆内存分配（已通过基准测试验证）。
3. **必须注册**：`ObjectFactory` 不提供内置默认工厂，必须先通过 `RegisterCreator` 注册后，`GetFactory` 才能正常获取。
4. **异常策略**：每个工厂实例可通过构造函数 `throwOnError` 独立控制异常行为，互不影响。
5. **类型安全**：泛型注册使用具体工厂类型，避免接口模糊，确保类型匹配精确。