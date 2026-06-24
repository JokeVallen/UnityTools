> 内容由 AI 根据核心代码生成，已通过人工审核。

# API 文档

## 公共 API 简介

以下列出所有对外暴露的公共类型及其成员。

### 1. `IObjectFactory` 接口
**签名**：`public interface IObjectFactory`

**作用**：所有对象工厂的基础接口，提供错误处理策略的配置。

#### 属性
- **`bool ThrowOnError { get; set; }`**  
  获取或设置一个值，指示初始化回调抛出异常时是否直接向上抛出。  
  默认为 `false`，此时异常被捕获并记录，资源被安全销毁；若为 `true`，异常会重新抛出。

---

### 2. `IGameObjectFactory` 接口
**签名**：`public interface IGameObjectFactory : IObjectFactory`

**作用**：定义创建 `GameObject` 的标准行为。

#### 方法
- **`GameObject Create(Action<GameObject> initialize = null)`**  
  创建一个未命名空对象，可选初始化回调。

- **`GameObject Create(string name, Action<GameObject> initialize = null)`**  
  创建指定名称的空对象。

- **`GameObject Create(string name, Action<GameObject> initialize = null, params Type[] components)`**  
  创建指定名称的对象并添加 `components` 中所有组件，可选初始化回调。

---

### 3. `IComponentFactory` 接口
**签名**：`public interface IComponentFactory : IObjectFactory`

**作用**：定义在已有 `GameObject` 上添加组件的标准行为，同时支持泛型和非泛型方式。

#### 方法
- **`T Create<T>(GameObject gameObject, Action<T> initialize = null) where T : Component`**  
  泛型版本，添加 `T` 组件并用强类型回调初始化。

- **`Component Create(GameObject gameObject, Type type, Action<Component> initialize = null)`**  
  非泛型版本，运行时通过 `Type` 动态添加组件，可使用 `Component` 类型的回调。

---

### 4. `ObjectFactory` 静态类
**签名**：`public static class ObjectFactory`

**作用**：全局工厂注册与解析中心，提供统一获取工厂实例的方法。

#### 方法
- **`void RegisterCreator<T>(Func<T> creator) where T : class, IObjectFactory`**  
  注册工厂创建委托。传入的委托在每次获取工厂时调用。

- **`T GetFactory<T>() where T : class, IObjectFactory`**  
  获取工厂实例。优先返回注册的自定义实现；若未注册，对于内置接口返回默认实现，否则返回 `null`。

- **`IObjectFactory GetFactory(Type factoryType)`**  
  非泛型版本，行为同上。

- **`bool TryGetFactory<T>(out T factory) where T : class, IObjectFactory`**  
  尝试获取工厂，返回是否成功的布尔值，推荐在不确定工厂是否已注册时使用。

- **`bool TryGetFactory(Type factoryType, out IObjectFactory factory)`**  
  非泛型尝试获取版本。

- **`void ClearCreators()`**  
  清除所有已注册的自定义工厂，主要用于测试和编辑器重置。

## 使用示例

### 获取默认工厂并创建对象
```csharp
var goFactory = ObjectFactory.GetFactory<IGameObjectFactory>();
GameObject enemy = goFactory.Create("Enemy", go => {
    go.tag = "Enemy";
}, typeof(Rigidbody));
```

### 泛型创建组件
```csharp
var compFactory = ObjectFactory.GetFactory<IComponentFactory>();
Rigidbody rb = compFactory.Create<Rigidbody>(gameObject, r => r.mass = 2.5f);
```

### 运行时动态创建组件
```csharp
Component comp = compFactory.Create(gameObject, typeof(BoxCollider), c => {
    ((BoxCollider)c).size = new Vector3(1, 1, 1);
});
```

### 注册自定义工厂（如对象池）
```csharp
ObjectFactory.RegisterCreator<IGameObjectFactory>(() => new PooledGameObjectFactory());
// 之后所有 GetFactory<IGameObjectFactory>() 都会返回池化版本
```

### 安全获取与测试
```csharp
if (ObjectFactory.TryGetFactory<IGameObjectFactory>(out var gf))
{
    gf.Create("SafeObject");
}
```

> 注意：内置的 `GameObjectFactory` 和 `ComponentFactory` 是内部类，未在文档中直接暴露，但可通过上述接口间接使用。