> 内容由 AI 根据核心代码生成，已通过人工审核。

# 📘 MonoSingleton 公共 API 文档

本文档列出所有公开的 API 并附带使用示例。更详细的功能说明请参阅 [README.md](./README.md)。

---

## 🔧 公共 API

### 1. `MonoSingleton<T>` 抽象类
**非持久化单例基类**  
泛型约束：`T : MonoBehaviour`

- **`public static T Instance`**  
  获取唯一实例，场景销毁后为 `null`。

- **`protected virtual void Awake()`**  
  注册实例（若为第一个）或销毁重复组件。子类重写时**必须**调用 `base.Awake()`。

- **`protected virtual void OnDestroy()`**  
  在实例销毁时将静态引用置空。子类重写时**必须**调用 `base.OnDestroy()`。

### 2. `MonoSingleton<T, I>` 抽象类
**非持久化 + 接口访问基类**  
泛型约束：`T : MonoBehaviour, I`

- **`public static I Instance`**  
  以接口 `I` 类型返回实例，调用者只能使用接口成员。

- **`protected virtual void Awake()`**  
  同 `MonoSingleton<T>`，需调用 `base.Awake()`。

- **`protected virtual void OnDestroy()`**  
  同 `MonoSingleton<T>`，需调用 `base.OnDestroy()`。

### 3. `MonoSingletonPersistant<T>` 抽象类
**持久化单例基类**  
泛型约束：`T : MonoBehaviour`  
继承自 `MonoSingleton<T>`

- 无新增公共成员，重写了 `Awake` 以调用 `DontDestroyOnLoad`。  
- 实例跨场景存活，直至手动 `Destroy`。

### 4. `MonoSingletonPersistant<T, I>` 抽象类
**持久化 + 接口访问基类**  
泛型约束：`T : MonoBehaviour, I`  
继承自 `MonoSingleton<T, I>`

- 静态属性 `Instance` 返回 `I` 类型。  
- 拥有跨场景持久化特性。

---

## 🧪 使用示例

### 普通单例（AudioManager）
```csharp
public class AudioManager : MonoSingleton<AudioManager>
{
    protected override void Awake()
    {
        base.Awake();
        // 初始化音频系统
    }

    public void PlaySound(string clipName) { /* ... */ }
}

// 在任何地方调用
AudioManager.Instance.PlaySound("BGM");
```

### 接口访问版本（IInventoryService）
```csharp
public interface IInventoryService
{
    void AddItem(string itemId, int count);
}

public class InventoryManager : MonoSingleton<InventoryManager, IInventoryService>, IInventoryService
{
    protected override void Awake() => base.Awake();

    public void AddItem(string itemId, int count) { /* ... */ }
}

// 调用者只能看到接口方法
IInventoryService inventory = InventoryManager.Instance;
inventory.AddItem("coin", 10);
```

### 持久化单例（GameManager）
```csharp
public class GameManager : MonoSingletonPersistant<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        // 全局状态初始化
    }

    public int CurrentLevel { get; set; }
}

// 切换场景后仍然有效
Debug.Log(GameManager.Instance.CurrentLevel);
```

### 持久化 + 接口（ISettingsProvider）
```csharp
public interface ISettingsProvider
{
    float MusicVolume { get; set; }
}

public class SettingsManager : MonoSingletonPersistant<SettingsManager, ISettingsProvider>, ISettingsProvider
{
    public float MusicVolume { get; set; } = 1f;

    protected override void Awake()
    {
        base.Awake();
        // 加载持久化设置
    }
}

// 外部只能通过接口访问
ISettingsProvider settings = SettingsManager.Instance;
settings.MusicVolume = 0.5f;
```

> 💡 所有示例都需要在场景中挂载对应的 MonoBehaviour 组件。持久化单例建议放在专门的“初始化场景”中，避免未创建时便访问。