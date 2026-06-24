> 内容由 AI 根据核心代码生成，已通过人工审核。

# Unity Object Factory

![MIT License](https://img.shields.io/badge/license-MIT-green)
![Unity](https://img.shields.io/badge/Unity-2020.3%2B-blue)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)
![](https://img.shields.io/badge/GC-Zero--Allocation-brightgreen)

一个轻量级、高性能的 Unity 对象工厂框架，为 `GameObject` 、 `Component` 和其它对象类型的创建提供统一入口，并支持在运行时无缝替换底层实现（如对象池、测试替身等）。

---

## 简介

在 Unity 项目开发中，直接使用 `new GameObject()` 或 `AddComponent<T>()` 会导致创建逻辑分散、难以统一维护和测试。本工具库通过全局注册中心与可扩展的工厂接口，将所有“创建”动作收敛到统一入口，让您在不修改业务代码的前提下切换创建策略，同时内置了安全的错误处理与自动资源清理机制。

**核心特点**：
- ✅ **零额外 GC 分配**（基准测试验证）
- ✅ **类型安全**（泛型注册与解析）
- ✅ **线程安全**（`ConcurrentDictionary` 支持）
- ✅ **可扩展**（支持自定义工厂与 `IFactoryCreator`）
- ✅ **测试友好**（一键重置注册状态）

---

## 安装环境要求

- Unity 2020.3 或更高版本
- .NET Standard 2.0 兼容环境（C# 7.0）

---

## 安装方式

### 源码导入
将源码文件夹下的所有 `.cs` 文件拷贝到项目的 `Assets` 目录中。

---

## 设计理念

- **统一入口**：所有对象创建都通过 `ObjectFactory` 获取工厂实例，杜绝散落的 `new` 调用。
- **开放封闭**：通过 `RegisterCreator<T>` 注入自定义工厂，扩展时无需修改现有业务代码。
- **安全第一**：初始化失败自动销毁已创建资源，通过工厂构造函数参数 `throwOnError` 灵活控制异常行为。
- **性能至上**：采用泛型静态存储与结构体设计，实现**零额外 GC 分配**，适合高频创建场景。
- **最小依赖**：零第三方库，仅依赖 Unity 内置 API。

---

## 核心功能

### 1. 工厂注册与解析

```csharp
// 注册默认工厂（启动时执行一次）
ObjectFactory.RegisterCreator<GameObjectFactory>(() => new GameObjectFactory());
ObjectFactory.RegisterCreator<ComponentFactory>(() => new ComponentFactory());

// 后续使用
var goFactory = ObjectFactory.GetFactory<GameObjectFactory>();
var compFactory = ObjectFactory.TryGetFactory<ComponentFactory>(out var cf) ? cf : null;
```

### 2. 创建 GameObject

```csharp
var factory = new GameObjectFactory(throwOnError: false);

// 基本创建
GameObject go = factory.Create();

// 指定名称与初始化
GameObject enemy = factory.Create("Enemy", go => {
    go.transform.position = Vector3.zero;
    go.AddComponent<Rigidbody>();
});

// 带自定义参数初始化
float speed = 10f;
GameObject player = factory.Create(speed, (go, spd) => {
    go.name = "Player";
    go.GetComponent<PlayerMovement>().Speed = spd;
});

// 同时预挂载组件
GameObject withComponents = factory.Create("Complex", null, typeof(Rigidbody), typeof(BoxCollider));
```

### 3. 添加 Component

```csharp
var factory = new ComponentFactory(throwOnError: false);

// 泛型方式
Rigidbody rb = factory.Create<Rigidbody>(gameObject, r => r.mass = 5f);

// 带自定义参数
int health = 100;
var healthComp = factory.Create<HealthComponent, int>(gameObject, health, (hc, hp) => {
    hc.MaxHealth = hp;
    hc.CurrentHealth = hp;
});

// 非泛型方式（运行时动态类型）
Component comp = factory.Create(gameObject, typeof(Animator), c => {
    ((Animator)c).enabled = true;
});
```

### 4. 自定义工厂扩展

```csharp
// 继承默认工厂添加池化逻辑
public class PooledGameObjectFactory : GameObjectFactory
{
    private Stack<GameObject> pool = new Stack<GameObject>();
    public override GameObject Create(Action<GameObject> initialize = null) {
        // 池化逻辑...
    }
}

// 注册自定义工厂
ObjectFactory.RegisterCreator<GameObjectFactory>(() => new PooledGameObjectFactory());
```

---

## 异常策略

每个工厂实例独立控制异常行为，通过构造函数参数 `throwOnError` 设置：

```csharp
// 严格模式：初始化失败将抛出异常
var strictFactory = new GameObjectFactory(throwOnError: true);

// 容错模式：初始化失败返回 null 并记录日志（默认）
var tolerantFactory = new GameObjectFactory(throwOnError: false);
```

- **`throwOnError = true`**：初始化回调中的异常将向上传播，对象会被销毁。
- **`throwOnError = false`**（默认）：异常被捕获并记录，返回 `null`，已创建对象被销毁。

---

## 测试与重置

```csharp
[SetUp]
public void SetUp()
{
    // 每个测试前清空注册表，保证隔离
    ObjectFactory.ClearCreators();
}
```

---

## 性能特性

- 泛型注册使用 **`Storage<T>` 静态字段**，获取时 **O(1) 零字典查找**。
- 注册与获取操作 **零堆内存分配**（基准测试验证）。
- 初始化回调使用**静态方法组**，避免闭包捕获。
- 压力测试：**1000 次 GameObject 创建耗时约 2.5ms**，GC 分配仅来自 Unity 内部（工厂逻辑零额外分配）。

详细基准测试报告请参阅 [TEST_REPORT.md](./tests/1.0.1-beta/TEST_REPORT.md)。

---

## 常见问题

**问：我需要为每个场景注册一次工厂吗？**  
答：不需要，`ObjectFactory` 是静态全局的，一次注册全局生效。建议在游戏启动时（如 `RuntimeInitializeOnLoadMethod`）完成注册。

**问：如果注册了多个同类型工厂会怎样？**  
答：最后注册的会覆盖之前的，符合幂等注册习惯。

**问：`ObjectFactory` 为什么不提供默认工厂？**  
答：为了保持职责单一和灵活性。您可以根据项目需求自由选择注册默认工厂、自定义工厂或特定场景工厂，避免隐式降级带来的不可预期行为。

**问：如何实现对象池？**  
答：继承 `GameObjectFactory` 并重写创建逻辑，内部使用 `Stack` 或 `Queue` 管理对象，注册时传入池工厂即可。

---

## 文档导航

- [API 文档](./source/1.0.1-beta/DOCUMENT.md)
- [测试报告](./tests/1.0.1-beta/TEST_REPORT.md)
- [发布说明](./RELEASE.md)

---

## 许可证

本项目基于 [MIT License](../../LICENSE) 开放使用。