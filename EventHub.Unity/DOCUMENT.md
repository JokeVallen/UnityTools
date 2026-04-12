> 本文档由作者与 AI 协作完成，内容已通过人工审阅确保准确性。

# EventHub API 文档（v1.0）

本文档面向事件系统使用者，详细说明所有对外公开的 API 及其用法，文档涵盖命名空间 `EventHub` 和 `EventHub.Unity`。

---

## 命名空间

- `EventHub`：框架级核心接口定义，用于抽象事件分发器的行为。
- `EventHub.Unity`：Unity 静态门面类、扩展方法、配置与监视器等。

---

## 核心接口

### ISubscription

事件订阅句柄。每个订阅操作返回一个 `ISubscription` 实例，调用方可借此取消订阅或查询状态。

**方法**

| 名称 | 说明 |
|------|------|
| `void Unsubscribe()` | 取消当前订阅。若已取消，调用无副作用。 |
| `void Dispose()` | 继承自 `System.IDisposable`，作用同 `Unsubscribe`，支持 `using` 语句。 |

**属性**

| 名称 | 类型 | 说明 |
|------|------|------|
| `IsDisposed` | `bool` | 是否已取消/释放。 |
| `EventType` | `Type` | 订阅的事件类型。 |
| `Priority` | `int` | 订阅时指定的优先级。 |

**使用示例**

```csharp
using EventHub.Unity;

public class ScoreChangedEvent { public int NewScore; }

// 订阅并保存句柄
ISubscription subscription = EventDispatcher.Subscribe<ScoreChangedEvent>(e =>
{
    Debug.Log($"Score changed to: {e.NewScore}");
});

// 稍后取消订阅
subscription.Dispose();

// 或使用 using 模式（适合一次性操作）
using (var sub = EventDispatcher.Subscribe<ScoreChangedEvent>(e => { }))
{
    // 在此作用域内有效
}
```

---

### ISyncEventDispatcher

同步事件分发器接口，提供基于 `Action<T>` 的订阅与发布。

**方法**

| 名称 | 说明 |
|------|------|
| `ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)` | 订阅同步事件。 |
| `int Unsubscribe<TEvent>(Action<TEvent> handler)` | 取消订阅，返回实际移除的委托个数。 |
| `void Publish<TEvent>(TEvent @event)` | 发布同步事件，按优先级依次调用订阅者。 |

**完整示例：游戏暂停与恢复**

```csharp
using EventHub.Unity;
using UnityEngine;

// 定义事件
public class GamePausedEvent { public bool Paused; }

public class GameManager : MonoBehaviour
{
    private bool isPaused;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            EventDispatcher.Publish(new GamePausedEvent { Paused = isPaused });
            // 其他系统（音频、输入、UI）会收到通知
        }
    }
}

public class AudioManager : MonoBehaviour
{
    private void Awake()
    {
        EventDispatcher.Subscribe<GamePausedEvent>(OnGamePaused, priority: 100);
    }

    private void OnGamePaused(GamePausedEvent e)
    {
        if (e.Paused)
            AudioListener.pause = true;
        else
            AudioListener.pause = false;
    }
}
```

**取消订阅的几种方式**

```csharp
// 方式一：通过委托取消
void Handler(ScoreChangedEvent e) { }
EventDispatcher.Subscribe<ScoreChangedEvent>(Handler);
EventDispatcher.Unsubscribe<ScoreChangedEvent>(Handler);

// 方式二：通过句柄取消（推荐）
ISubscription sub = EventDispatcher.Subscribe<ScoreChangedEvent>(e => { });
sub.Unsubscribe(); // 或 sub.Dispose();

// 方式三：取消多个重复订阅
EventDispatcher.Subscribe<ScoreChangedEvent>(Handler);
EventDispatcher.Subscribe<ScoreChangedEvent>(Handler);
int removed = EventDispatcher.Unsubscribe<ScoreChangedEvent>(Handler);
Debug.Log($"Removed {removed} handlers"); // 输出 2
```

---

### IAsyncEventDispatcher

异步事件分发器接口，基于 `Func<TEvent, CancellationToken, UniTask>`。

**方法**

| 名称 | 说明 |
|------|------|
| `ISubscription Subscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)` | 订阅异步事件。 |
| `int Unsubscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler)` | 取消订阅。 |
| `UniTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)` | 异步串行发布。 |

**示例：异步加载场景数据**

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using EventHub.Unity;
using UnityEngine;

public class LoadSceneEvent
{
    public string SceneName;
    public int Progress;
}

public class SceneLoader : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        var cts = new CancellationTokenSource();

        // 模拟加载进度更新
        for (int i = 0; i <= 100; i += 20)
        {
            await EventDispatcher.PublishAsync(new LoadSceneEvent
            {
                SceneName = "Level_01",
                Progress = i
            }, cts.Token);

            await UniTask.Delay(200);
        }
    }
}

public class LoadingScreen : MonoBehaviour
{
    private void Awake()
    {
        EventDispatcher.Subscribe<LoadSceneEvent>(UpdateProgressBar);
    }

    private async UniTask UpdateProgressBar(LoadSceneEvent e, CancellationToken ct)
    {
        // 模拟动画过渡
        await UniTask.Delay(50, cancellationToken: ct);
        Debug.Log($"Loading {e.SceneName}: {e.Progress}%");
    }
}
```

**取消令牌的完整用法**

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using EventHub.Unity;

public class NetworkRequestEvent { public string Url; }

public class Example
{
    private CancellationTokenSource cts;

    public async UniTask SendRequest()
    {
        cts = new CancellationTokenSource();

        // 订阅者可以响应取消
        EventDispatcher.Subscribe<NetworkRequestEvent>(async (e, ct) =>
        {
            await UniTask.Delay(5000, cancellationToken: ct);
            Debug.Log("Request completed");
        });

        // 发布并传入令牌
        var publishTask = EventDispatcher.PublishAsync(
            new NetworkRequestEvent { Url = "https://api.example.com" },
            cts.Token
        );

        // 模拟用户取消操作
        await UniTask.Delay(1000);
        cts.Cancel();

        try
        {
            await publishTask;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Request was cancelled");
        }
    }
}
```

---

### IParallelizable

并发发布接口，用于同时执行多个异步订阅者。

**方法**

| 名称 | 说明 |
|------|------|
| `UniTask PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)` | 异步并发发布，所有订阅者并发执行。若任意订阅者抛出异常，将聚合为 `AggregateException` 抛出。 |

**示例：并行加载多个资源**

```csharp
using Cysharp.Threading.Tasks;
using EventHub.Unity;
using UnityEngine;

public class PreloadEvent { }

public class ResourceManager : MonoBehaviour
{
    private async void Start()
    {
        // 假设有多个系统订阅了 PreloadEvent 并行加载各自资源
        await EventDispatcher.PublishParallelAsync(new PreloadEvent());
        Debug.Log("All preloading completed");
    }
}

public class TextureLoader : MonoBehaviour
{
    private void Awake()
    {
        EventDispatcher.Subscribe<PreloadEvent>(LoadTexturesAsync);
    }

    private async UniTask LoadTexturesAsync(PreloadEvent e, CancellationToken ct)
    {
        await UniTask.Delay(2000, cancellationToken: ct);
        Debug.Log("Textures loaded");
    }
}

public class AudioLoader : MonoBehaviour
{
    private void Awake()
    {
        EventDispatcher.Subscribe<PreloadEvent>(LoadAudioAsync);
    }

    private async UniTask LoadAudioAsync(PreloadEvent e, CancellationToken ct)
    {
        await UniTask.Delay(1500, cancellationToken: ct);
        Debug.Log("Audio loaded");
    }
}
// 输出顺序可能是 "Audio loaded" 然后 "Textures loaded"（取决于耗时）
// 总耗时约 2000ms（最长的那个），而非 3500ms
```

**并行发布异常处理**

```csharp
using Cysharp.Threading.Tasks;
using System;
using EventHub.Unity;

public class BatchProcessEvent { }

public class Example
{
    public async UniTask RunBatch()
    {
        // 订阅三个处理器，其中两个会抛出异常
        EventDispatcher.Subscribe<BatchProcessEvent>(async (_, __) => throw new Exception("Error A"));
        EventDispatcher.Subscribe<BatchProcessEvent>(async (_, __) => await UniTask.Yield());
        EventDispatcher.Subscribe<BatchProcessEvent>(async (_, __) => throw new Exception("Error B"));

        try
        {
            await EventDispatcher.PublishParallelAsync(new BatchProcessEvent());
        }
        catch (AggregateException ex)
        {
            Debug.Log($"Caught {ex.InnerExceptions.Count} exceptions");
            foreach (var inner in ex.InnerExceptions)
            {
                Debug.Log($" - {inner.Message}");
            }
        }
    }
}
```

---

### IEventDispatcher

复合分发器标记接口，不含任何成员。自定义分发器可同时实现 `ISyncEventDispatcher`、`IAsyncEventDispatcher` 等，并通过此接口统一注入。

---

## 静态门面类 EventDispatcher

`EventHub.Unity.EventDispatcher` 是所有功能的主入口，提供静态方法简化调用。

### 同步事件 API

```csharp
// 订阅
public static ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0);

// 取消订阅
public static int Unsubscribe<TEvent>(Action<TEvent> handler);

// 发布
public static void Publish<TEvent>(TEvent @event);
```

### 异步事件 API

```csharp
public static ISubscription Subscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
public static int Unsubscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler);
public static UniTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
```

### 并行发布 API

```csharp
public static UniTask PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
```

> **注意**：当使用 `PublishParallelAsync` 时，若有多个订阅者抛出异常，它们会被聚合为一个 `AggregateException`。您可以通过 `ex.InnerExceptions` 访问所有独立异常。若没有任何订阅者抛出异常，则正常完成。

### 扩展订阅方法

```csharp
// 订阅一次性事件（同步与异步）
public static ISubscription SubscribeOnce<TEvent>(Action<TEvent> handler, int priority = 0);
public static ISubscription SubscribeOnce<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);

// 订阅条件事件（同步与异步）
public static ISubscription SubscribeIf<TEvent>(Predicate<TEvent> filter, Action<TEvent> handler, int priority = 0);
public static ISubscription SubscribeIf<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
```

**订阅一次性事件示例：玩家首次死亡**

```csharp
using EventHub.Unity;
using UnityEngine;

public class PlayerDiedEvent { public string Reason; }

public class AchievementManager : MonoBehaviour
{
    private void Awake()
    {
        // 只在玩家第一次死亡时触发，之后自动取消
        EventDispatcher.SubscribeOnce<PlayerDiedEvent>(e =>
        {
            Debug.Log("Achievement Unlocked: First Blood!");
        });
    }
}

public class Player : MonoBehaviour
{
    public void Die()
    {
        EventDispatcher.Publish(new PlayerDiedEvent { Reason = "Fell into void" });
        // 第一次发布会触发成就，后续发布不会再触发
    }
}
```

**订阅条件事件示例：高分通知**

```csharp
using EventHub.Unity;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int currentScore;

    private void Awake()
    {
        // 仅当分数超过 1000 时才触发
        EventDispatcher.SubscribeIf<ScoreChangedEvent>(
            filter: e => e.NewScore > 1000,
            handler: OnHighScore
        );
    }

    private void OnHighScore(ScoreChangedEvent e)
    {
        Debug.Log($"New high score: {e.NewScore}!");
    }

    public void AddScore(int points)
    {
        currentScore += points;
        EventDispatcher.Publish(new ScoreChangedEvent { NewScore = currentScore });
    }
}
```

### 特殊发布方法

```csharp
// 可中断事件发布
public static void PublishInterruptableEvents<TEvent>(TEvent @event) where TEvent : IInterruptableEvent;

// 可取消事件发布
public static void PublishCancelableEvents<TEvent>(TEvent @event) where TEvent : ICancelableEvent;
```

### 主线程订阅扩展

```csharp
public static ISubscription SubscribeOnMainThread<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
public static ISubscription SubscribeOnceOnMainThread<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
public static ISubscription SubscribeIfOnMainThread<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
```

**主线程订阅示例：更新 UI 文本**

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using EventHub.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text scoreText;

    private void Awake()
    {
        // 确保 UI 更新在主线程执行
        EventDispatcher.SubscribeOnMainThread<ScoreChangedEvent>(UpdateScoreUI);
    }

    private async UniTask UpdateScoreUI(ScoreChangedEvent e, CancellationToken ct)
    {
        // 模拟一些异步计算（可能在线程池）
        await UniTask.RunOnThreadPool(() => HeavyCalculation(e));

        // 自动切回主线程，安全更新 UI
        scoreText.text = $"Score: {e.NewScore}";
    }

    private void HeavyCalculation(ScoreChangedEvent e)
    {
        // 耗时计算...
    }
}
```
> **注意**：`SubscribeOnMainThread` 保证处理函数的**第一行代码**在主线程执行。若您在函数内部主动调用 `UniTask.SwitchToThreadPool()` 切换线程，后续代码将脱离主线程。如需再次操作 Unity API，请手动调用 `UniTask.SwitchToMainThread()` 切回。

### 配置与状态管理

```csharp
// 替换默认分发器
public static IEventDispatcher Dispatcher { set; }
public static IAsyncEventDispatcher AsyncDispatcher { set; }
public static ISyncEventDispatcher SyncDispatcher { set; }
public static IParallelizable Parallelizable { set; }

// 日志配置
public static ILogger Logger { set; }
public static bool LogEnabled { get; set; }

// 异常捕获配置
public static event Action<Type, Delegate, Exception> OnError;
public static bool ExceptionCatchEnabled { get; set; }
```

**全局异常监听示例**

```csharp
using System;
using EventHub.Unity;
using UnityEngine;

public class ErrorReporter : MonoBehaviour
{
    private void Awake()
    {
        EventDispatcher.OnError += OnEventError;
    }

    private void OnDestroy()
    {
        EventDispatcher.OnError -= OnEventError;
    }

    private void OnEventError(Type eventType, Delegate handler, Exception exception)
    {
        // 上报到远程服务器或显示错误提示
        Debug.LogError($"[EventHub] Error in {handler.Method.Name} for event {eventType.Name}: {exception.Message}");
    }
}
```

### 资源清理

```csharp
public static int TryCleanupUnusedLocks();                // 清理未使用的读写锁
public static int TryCleanupUnusedCollections();          // 清理空订阅者集合
public static int TryCleanupUnusedLocksAndCollections();  // 同时清理
```

**示例：在场景加载间隙主动清理**

```csharp
using EventHub.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CleanupManager : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // 场景卸载后清理未使用的内部资源
        int cleaned = EventDispatcher.TryCleanupUnusedLocksAndCollections();
        Debug.Log($"[EventHub] Cleaned up {cleaned} unused resources");
    }
}
```

这些清理方法通常在以下场景调用：
- 场景切换后，大量对象被销毁时。
- 游戏进入后台或暂停时。
- 主动调用以确保内存占用最小化。
框架自身不会自动调用这些方法，以避免在不恰当的时机引入性能开销。

---

## Component 扩展方法

命名空间 `EventHub.Unity` 为 `UnityEngine.Component` 提供了扩展方法，自动将订阅句柄注册到全局监视器，通过定时轮询检测组件是否销毁，若销毁则自动取消与之关联的订阅。

**可用扩展方法列表**

```csharp
// 基础订阅
public static ISubscription Subscribe<TEvent>(this Component component, Action<TEvent> handler, int priority = 0);
public static ISubscription Subscribe<TEvent>(this Component component, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);

// 一次性事件订阅
public static ISubscription SubscribeOnce<TEvent>(this Component component, Action<TEvent> handler, int priority = 0);
public static ISubscription SubscribeOnce<TEvent>(this Component component, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);

// 条件事件订阅
public static ISubscription SubscribeIf<TEvent>(this Component component, Predicate<TEvent> filter, Action<TEvent> handler, int priority = 0);
public static ISubscription SubscribeIf<TEvent>(this Component component, Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);

// 主线程安全版本的事件订阅
public static ISubscription SubscribeOnMainThread<TEvent>(this Component component, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
public static ISubscription SubscribeOnceOnMainThread<TEvent>(this Component component, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
public static ISubscription SubscribeIfOnMainThread<TEvent>(this Component component, Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);

// 取消与组件关联的所有订阅
public static void UnsubscribeAll(this Component component);
```

**完整示例：使用 Component 扩展简化生命周期管理**

```csharp
using EventHub.Unity;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private void Awake()
    {
        // 使用 this.Subscribe，当 Enemy 被销毁时自动取消所有订阅
        this.Subscribe<GamePausedEvent>(OnGamePaused);
        this.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnGamePaused(GamePausedEvent e)
    {
        enabled = !e.Paused;
    }

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        // 玩家死亡，敌人播放庆祝动画
        GetComponent<Animator>().SetTrigger("Victory");
    }

    // 无需在 OnDestroy 中手动取消，框架自动处理
}

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            var enemy = Instantiate(enemyPrefab);
            // enemy 的订阅由 Enemy 组件中的 this.Subscribe 管理
        }
    }
}
```

**手动取消组件所有订阅**

```csharp
using EventHub.Unity;
using UnityEngine;

public class TemporaryListener : MonoBehaviour
{
    private void Awake()
    {
        this.Subscribe<SomeEvent>(OnSomeEvent);
    }

    public void StopListening()
    {
        // 一次性取消该组件上的所有订阅，而无需逐个保存句柄
        this.UnsubscribeAll();
    }

    private void OnSomeEvent(SomeEvent e) { }
}
```

---

## 特殊事件接口

### IInterruptableEvent

可中断事件。在调用 `PublishInterruptableEvents` 时，若某订阅者将 `Interrupted` 属性设为 `true`，则自身及后续订阅者都将被跳过执行。

```csharp
public interface IInterruptableEvent
{
    bool Interrupted { get; }
}
```

**完整示例：技能释放前的校验链**

```csharp
using EventHub.Unity;
using UnityEngine;

public class SkillCastEvent : IInterruptableEvent
{
    public string SkillName;
    public bool Interrupted { get; private set; }
    public void Interrupt() => Interrupted = true;
}

public class SkillSystem : MonoBehaviour
{
    private void Awake()
    {
        // 校验链，按优先级执行
        EventDispatcher.Subscribe<SkillCastEvent>(CheckMana, priority: 100);
        EventDispatcher.Subscribe<SkillCastEvent>(CheckCooldown, priority: 90);
        EventDispatcher.Subscribe<SkillCastEvent>(ExecuteSkill, priority: 0);
    }

    private void CheckMana(SkillCastEvent e)
    {
        if (CurrentMana < 10)
        {
            Debug.Log("Not enough mana!");
            e.Interrupt();
        }
    }

    private void CheckCooldown(SkillCastEvent e)
    {
        if (IsOnCooldown(e.SkillName))
        {
            Debug.Log("Skill on cooldown!");
            e.Interrupt();
        }
    }

    private void ExecuteSkill(SkillCastEvent e)
    {
        // 只有前面都未中断才会执行
        Debug.Log($"Casting {e.SkillName}!");
    }

    public void CastSkill(string skillName)
    {
        EventDispatcher.PublishInterruptableEvents(new SkillCastEvent { SkillName = skillName });
    }
}
```

### ICancelableEvent

可取消事件。在调用 `PublishCancelableEvents` 时，若事件实例的 `Cancelled` 为 `true`，则对应的订阅者不会被执行，但不影响其他订阅者。

```csharp
public interface ICancelableEvent
{
    bool Cancelled { get; }
}
```

**完整示例：动态决定是否处理事件**

```csharp
using EventHub.Unity;
using UnityEngine;

public class DamageEvent : ICancelableEvent
{
    public GameObject Target;
    public int Amount;
    public bool Cancelled { get; private set; }
    public void Cancel() => Cancelled = true;
}

public class DamageHandler : MonoBehaviour
{
    private void Awake()
    {
        EventDispatcher.Subscribe<DamageEvent>(ApplyDamage, priority: 0);
        EventDispatcher.Subscribe<DamageEvent>(CheckInvincibility, priority: 100);
    }

    private void CheckInvincibility(DamageEvent e)
    {
        if (e.Target.CompareTag("Invincible"))
        {
            Debug.Log("Target is invincible!");
            e.Cancel(); // 标记为取消，后续 ApplyDamage 将不执行
        }
    }

    private void ApplyDamage(DamageEvent e)
    {
        // 此处理器只有在事件未被取消时才会执行
        var health = e.Target.GetComponent<Health>();
        if (health != null) health.TakeDamage(e.Amount);
    }

    public void DealDamage(GameObject target, int amount)
    {
        EventDispatcher.PublishCancelableEvents(new DamageEvent
        {
            Target = target,
            Amount = amount
        });
    }
}
```

---

## ILogger 日志接口

实现 `EventHub.Unity.ILogger` 接口可自定义日志输出。

```csharp
public interface ILogger
{
    void LogError(Type eventType, Delegate originalHandler, Exception exception);
    void LogError(Exception exception);
    void LogError(string message);
    void LogWarning(string message);
    void LogInfo(string message);
}
```

**完整示例：接入 Unity 日志系统并上报异常**

```csharp
using System;
using EventHub.Unity;
using UnityEngine;

public class CustomLogger : ILogger
{
    public void LogError(Type eventType, Delegate handler, Exception exception)
    {
        Debug.LogError($"[EventHub] [{eventType.Name}] {handler.Method.Name}: {exception}");

        // 可在此处上报到崩溃收集服务
        // CrashReporter.Report(exception);
    }

    public void LogError(Exception exception)
    {
        Debug.LogException(exception);
    }

    public void LogError(string message)
    {
        Debug.LogError($"[EventHub] {message}");
    }

    public void LogWarning(string message)
    {
        Debug.LogWarning($"[EventHub] {message}");
    }

    public void LogInfo(string message)
    {
        Debug.Log($"[EventHub] {message}");
    }
}

// 在游戏启动时设置
public class GameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        EventDispatcher.Logger = new CustomLogger();
    }
}
```

---

## 订阅监视器

全局单例 `SubscriptionMonitor` 负责追踪通过 Component 扩展方法创建的订阅，并在组件销毁时自动通过事件订阅句柄取消订阅以及清理订阅句柄。

### ISubscriptionMonitor

```csharp
public interface ISubscriptionMonitor : IDisposable
{
    void StartTimer(CancellationToken cancellationToken = default);
    void StopTimer();
    void Register(Component component, ISubscription subscription);
    void Register(Component component, ISubscription subscription1, ISubscription subscription2);
    void Register(Component component, params ISubscription[] subscriptions);
    void UnsubscribeAll(Component component);
}
```

### ISubscriptionMonitorConfig

配置监视器的清理策略。

```csharp
public interface ISubscriptionMonitorConfig
{
    int MilliSecondsDelay { get; set; }       // 清理检查间隔（毫秒），默认1000ms
    bool StartTimerOnInitialize { get; set; } // 是否自动启动清理定时器，默认true
    bool AutoSave { get; set; }               // 是否自动保存配置，默认true
    int AutoSaveDirtyCount { get; set; }      // 触发自动保存的配置修改次数阈值，默认5
}
```

**配置示例：调整清理间隔并手动控制定时器**

```csharp
using EventHub.Unity;
using UnityEngine;

public class MonitorConfigurator : MonoBehaviour
{
    private void Awake()
    {
        // 获取配置实例
        var config = EventDispatcher.SubscriptionMonitorConfig;

        // 修改配置
        config.MilliSecondsDelay = 5000;       // 每 5 秒检查一次
        config.StartTimerOnInitialize = false; // 不自动启动
        config.AutoSave = true;
        config.AutoSaveDirtyCount = 3;

        // 手动启动监视器
        EventDispatcher.SubscriptionMonitor.StartTimer();
    }

    private void OnDestroy()
    {
        // 停止监视器（可选）
        EventDispatcher.SubscriptionMonitor.StopTimer();
    }
}
```

**手动注册订阅到监视器（高级用法）**

```csharp
using EventHub.Unity;
using UnityEngine;

public class ManualRegistrationExample : MonoBehaviour
{
    private ISubscription manualSub;

    private void Start()
    {
        // 通过静态方法订阅，获得句柄
        manualSub = EventDispatcher.Subscribe<GamePausedEvent>(OnPaused);

        // 手动将句柄注册到监视器，绑定到当前组件
        EventDispatcher.SubscriptionMonitor.Register(this, manualSub);

        // 当此组件销毁时，manualSub 也会被自动取消
    }

    private void OnPaused(GamePausedEvent e) { }
}
```

---

## 自定义分发器

开发者可实现 `ISyncEventDispatcher` 等接口替换默认分发器，用于单元测试、行为扩展或性能优化。

**示例：用于单元测试的 Mock 分发器**

```csharp
using System;
using System.Collections.Generic;
using EventHub;
using EventHub.Unity;

public class MockEventDispatcher : IEventDispatcher, ISyncEventDispatcher
{
    public List<object> PublishedEvents { get; } = new List<object>();

    public ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)
    {
        // 测试中可以不实际存储订阅者，或简单记录
        return new MockSubscription(typeof(TEvent));
    }

    public int Unsubscribe<TEvent>(Action<TEvent> handler) => 0;

    public void Publish<TEvent>(TEvent @event)
    {
        PublishedEvents.Add(@event);
    }

    private class MockSubscription : ISubscription
    {
        public Type EventType { get; }
        public int Priority => 0;
        public bool IsDisposed { get; private set; }

        public MockSubscription(Type eventType) => EventType = eventType;

        public void Unsubscribe() => IsDisposed = true;
        public void Dispose() => Unsubscribe();
    }
}

// 在测试中使用
public class MyTest
{
    [SetUp]
    public void Setup()
    {
        EventDispatcher.Dispatcher = new MockEventDispatcher();
    }

    // 测试方法...
}
```

**示例：带性能监控的分发器装饰器**

```csharp
using System;
using System.Diagnostics;
using EventHub;
using EventHub.Unity;

public class ProfiledSyncDispatcher : ISyncEventDispatcher
{
    private readonly ISyncEventDispatcher inner;

    public ProfiledSyncDispatcher(ISyncEventDispatcher inner)
    {
        this.inner = inner;
    }

    public void Publish<TEvent>(TEvent @event)
    {
        var sw = Stopwatch.StartNew();
        inner.Publish(@event);
        sw.Stop();

        if (sw.ElapsedMilliseconds > 5)
        {
            UnityEngine.Debug.LogWarning($"[EventHub] Publish<{typeof(TEvent).Name}> took {sw.ElapsedMilliseconds}ms");
        }
    }

    public ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)
        => inner.Subscribe(handler, priority);

    public int Unsubscribe<TEvent>(Action<TEvent> handler)
        => inner.Unsubscribe(handler);
}

// 在游戏启动时包装默认分发器
public class GameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        var customSyncEventDispatcher = new CustomSyncEventDispatcher();
        EventDispatcher.Dispatcher = new ProfiledSyncDispatcher(customSyncEventDispatcher);
    }
}
```

---

## 高级场景示例

### 示例：全局事件总线封装

```csharp
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using EventHub.Unity;

/// <summary>
/// 对 EventDispatcher 的静态封装，便于在大型项目中统一管理事件类型
/// </summary>
public static class GameEvents
{
    // 同步事件
    public static ISubscription Subscribe<T>(Action<T> handler, int priority = 0)
        => EventDispatcher.Subscribe(handler, priority);

    public static void Publish<T>(T @event)
        => EventDispatcher.Publish(@event);

    // 异步事件
    public static ISubscription SubscribeAsync<T>(Func<T, CancellationToken, UniTask> handler, int priority = 0)
        => EventDispatcher.Subscribe(handler, priority);

    public static UniTask PublishAsync<T>(T @event, CancellationToken ct = default)
        => EventDispatcher.PublishAsync(@event, ct);

    // 并行发布
    public static UniTask PublishParallelAsync<T>(T @event, CancellationToken ct = default)
        => EventDispatcher.PublishParallelAsync(@event, ct);
}
```

### 示例：基于事件的状态机

```csharp
using EventHub.Unity;
using UnityEngine;

public enum GameState { Menu, Playing, Paused, GameOver }

public class StateChangedEvent { public GameState NewState; }

public class GameStateMachine : MonoBehaviour
{
    public GameState CurrentState { get; private set; }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        EventDispatcher.Publish(new StateChangedEvent { NewState = newState });
    }
}

public class UIPanelController : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    private void Awake()
    {
        this.Subscribe<StateChangedEvent>(OnStateChanged);
    }

    private void OnStateChanged(StateChangedEvent e)
    {
        menuPanel.SetActive(e.NewState == GameState.Menu);
        hudPanel.SetActive(e.NewState == GameState.Playing);
        pausePanel.SetActive(e.NewState == GameState.Paused);
        gameOverPanel.SetActive(e.NewState == GameState.GameOver);
    }
}
```

### 示例：防抖动的搜索输入事件

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;
using EventHub.Unity;
using UnityEngine;

public class SearchInputEvent { public string Keyword; }

public class SearchBar : MonoBehaviour
{
    private CancellationTokenSource searchCts;

    public async void OnInputFieldChanged(string input)
    {
        // 取消上一次搜索
        searchCts?.Cancel();
        searchCts?.Dispose();
        searchCts = new CancellationTokenSource();

        try
        {
            // 发布输入事件（订阅者可以实现防抖）
            await EventDispatcher.PublishAsync(new SearchInputEvent { Keyword = input }, searchCts.Token);
        }
        catch(OperationCanceledException)
        {

        }
    }
}

public class SearchHandler : MonoBehaviour
{
    private CancellationTokenSource debounceCts;

    private void Awake()
    {
        EventDispatcher.Subscribe<SearchInputEvent>(OnSearchInput);
    }

    private async UniTask OnSearchInput(SearchInputEvent e, CancellationToken ct)
    {
        // 防抖：等待 300ms，如果期间有新输入则取消
        debounceCts?.Cancel();
        debounceCts?.Dispose();
        debounceCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        try
        {
            await UniTask.Delay(300, cancellationToken: debounceCts.Token);

            // 执行实际搜索
            Debug.Log($"Searching for: {e.Keyword}");
            await PerformSearch(e.Keyword, ct);
        }
        catch (OperationCanceledException)
        {
            // 被防抖取消，忽略
        }
    }

    private async UniTask PerformSearch(string keyword, CancellationToken ct)
    {
        // 模拟网络请求
        await UniTask.Delay(1000, cancellationToken: ct);
        Debug.Log($"Search results for '{keyword}' received");
    }
}
```

## 注意事项

### 事件类型选择

- **推荐使用引用类型（`class`）作为事件**：当前版本下，`class` 事件无装箱开销，与框架内部设计天然契合。
- **值类型（`struct`）事件**：虽然允许使用，但因内部委托包装涉及 `object` 参数，发布时会发生装箱，性能收益无法体现。未来版本将针对值类型事件进行优化，届时可无缝切换。

### 同步与异步隔离

- 通过 `Subscribe(Action<TEvent>)` 注册的同步订阅者**仅响应** `Publish<TEvent>` 同步发布。
- 通过 `Subscribe(Func<TEvent, CancellationToken, UniTask>)` 注册的异步订阅者**仅响应** `PublishAsync<TEvent>` 或 `PublishParallelAsync<TEvent>` 异步发布。
- 同步发布永远不会触发异步订阅者，异步发布也永远不会触发同步订阅者。请确保订阅方法与发布方法匹配。

### 主线程安全

- `SubscribeOnMainThread` 等方法仅保证处理函数的**入口在主线程**。若在处理函数内部主动调用 `SwitchToThreadPool` 等切换线程，后续代码将脱离主线程，操作 Unity API 需自行切回。
- 在后台线程中发布事件是安全的，订阅者的执行线程可能受到订阅方式影响，例如主线程安全版本的订阅会提供主线程安全的委托包装，但更关键的影响来自订阅的事件逻辑本身。

### 资源释放

- 通过 `Component` 扩展方法创建的订阅由 `SubscriptionMonitor` 自动管理，无需手动释放。
- 通过静态方法订阅并保存的 `ISubscription` 句柄，应在适当位置（如 `OnDestroy`）调用 `Dispose`。
- 自行创建的 `CancellationTokenSource` 在使用后务必调用 `Dispose`，避免资源泄漏。

### 性能考量

- **高频同步事件**：使用 `Publish`，稳定状态下零 GC 分配，适合 `Update` 等场景。
- **低频异步事件**：使用 `PublishAsync` 或 `PublishParallelAsync`，单次调用约有数毫秒的调度开销，适合资源加载、网络请求等场景。
- **订阅者数量**：订阅者集合使用快照机制，发布时可安全增删。单个订阅者调用开销极低（微秒级），可承载数千订阅者。

### 自定义分发器

- 替换 `EventDispatcher.Dispatcher` 时，建议在游戏启动时（如 `RuntimeInitializeOnLoadMethod`）完成，避免运行时替换导致状态不一致。

## 常见问题

### Q1：同步发布和异步发布可以混用吗？
不可以。同步和异步是完全独立的管道，互不调用。您需要为同一个事件类型分别注册同步和异步订阅者，并分别使用对应的发布方法。

### Q2：可以在多个线程中同时发布事件吗？
可以。框架内部使用读写锁和快照机制保证线程安全。

### Q3：如何知道一个事件有哪些订阅者？
框架不直接暴露订阅者列表。您可以通过实现自定义 `ILogger` 记录订阅日志，或通过 `EventDispatcher.OnError` 监听异常来间接排查。

### Q4：`SubscriptionMonitor` 的清理定时器会影响性能吗？
定时器仅在每次间隔触发时检查已销毁的组件，开销极低。您可以通过 `SubscriptionMonitorConfig.MilliSecondsDelay` 调整间隔，甚至关闭定时器手动调用清理。

### Q5：如何迁移旧版的事件系统到 EventHub？
建议渐进式迁移：先在新功能中使用 EventHub，逐步替换旧的耦合代码。如需完全替换，可编写适配层将旧事件转发到 EventHub。

## 适用工具版本

|系列版本号|具体版本号|
|--|--|
|1.0|1.0.0-beta|

## 文档变更记录

|修订时间|修改说明|
|--|--|
|......|......|