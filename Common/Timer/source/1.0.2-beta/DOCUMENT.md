> 内容由 AI 根据核心代码生成，已通过人工审核。


## 1. 公共 API 简介

### 1.1 核心静态类 `GlobalTimer`

提供所有运行时计时器注册和控制方法。

#### 注册方法（按时间源分类）

| 方法签名 | 作用 |
| :--- | :--- |
| `RegisterScaled(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)` | 注册受 `Time.timeScale` 影响的计时器，单位秒。 |
| `RegisterScaled(TimeSpan interval, ...)` | 同上，接受 `TimeSpan`。 |
| `RegisterUnscaled(float interval, ...)` | 注册不受 `Time.timeScale` 影响的计时器。 |
| `RegisterUnscaled(TimeSpan interval, ...)` | 同上，`TimeSpan` 版本。 |
| `RegisterFrame(int frameCount, Action callback, ...)` | 注册帧驱动计时器，在 `Update` 中推进。 |
| `RegisterMonoUpdate(Action callback, Optional<int> groupID = default)` | 注册每帧在 `Update` 中执行的计时器（等效于 `RegisterFrame(1, ...)`）。 |
| `RegisterMonoLateUpdate(Action callback, ...)` | 注册在 `LateUpdate` 中每帧执行的计时器。 |
| `RegisterMonoFixedUpdate(Action callback, ...)` | 注册在 `FixedUpdate` 中按物理步长执行的计时器（受缩放影响）。 |
| `RegisterMonoFixedUnscaled(float interval, ...)` | 注册在 `FixedUpdate` 中按物理步长但不受缩放影响的计时器。 |
| `RegisterCoroutineUpdate(Action callback, ...)` | 注册在协程 `yield return null` 后执行的计时器（每帧一次）。 |
| `RegisterCoroutineWaitForFixedUpdate(int frameCount, ...)` | 注册在协程 `yield return WaitForFixedUpdate` 后执行的帧驱动计时器。 |
| `RegisterCoroutineEndOfFrame(int frameCount, ...)` | 注册在协程 `yield return WaitForEndOfFrame` 后执行的帧驱动计时器。 |
| `RegisterIndependent(float interval, Action callback, float customScale, ...)` | 注册独立缩放的计时器（基于 `Unscaled` 增量 × `customScale`），不受全局缩放影响。 |
| `RegisterIndependent(TimeSpan interval, ...)` | 同上，`TimeSpan` 版本。 |
| `RegisterIndependentFrame(int frameCount, Action callback, float customScale, ...)` | 注册独立缩放的帧驱动计时器（每帧推进 `customScale` 帧）。 |
| `RegisterWallClock(float interval, ...)` | 注册挂钟计时器（基于 `Stopwatch`，App 后台仍走）。 |
| `RegisterWallClock(TimeSpan interval, ...)` | 同上，`TimeSpan` 版本。 |
| `RegisterManual(float interval, ...)` | 注册手动驱动计时器，需外部调用 `ManualUpdate` 推进。 |
| `RegisterManual(TimeSpan interval, ...)` | 同上，`TimeSpan` 版本。 |
| `Register(float interval, Action callback, TimeDelta delta, TimeSchedule schedule, ...)` | 通用注册方法，允许自由组合 `TimeDelta` 和 `TimeSchedule` 原子。 |
| `Register(TimeSpan interval, ...)` | 同上，`TimeSpan` 版本。 |

#### 分组控制

| 方法签名 | 作用 |
| :--- | :--- |
| `CancelGroup(int groupId)` | 取消指定组的所有计时器。 |
| `PauseGroup(int groupId)` | 暂停指定组的所有计时器。 |
| `ResumeGroup(int groupId)` | 恢复指定组的所有计时器。 |
| `SetGroupPaused(int groupId, bool isPaused)` | 设置指定组的暂停状态。 |

#### 全局控制

| 方法签名 | 作用 |
| :--- | :--- |
| `CancelAll()` | 取消所有正在运行的计时器（并清空待执行回调）。 |
| `ManualUpdate(float deltaTime)` | 手动推进所有 `TimeDelta.Manual` 计时器。 |


### 1.2 编辑器静态类 `EditorTimer`（仅限 UNITY_EDITOR）

提供与 `GlobalTimer` 完全相同的 API 签名，但底层由 `EditorApplication.update` 驱动，用于编辑器非运行模式下的计时支持。

> **注意**：`EditorTimer` 仅支持 `TimeSchedule.Update` 和 `TimeSchedule.Manual` 调度。传入 `LateUpdate`、`FixedUpdate`、`Coroutine` 等不支持的调度会抛出 `NotSupportedException`。


### 1.3 句柄结构 `TimerHandle`

表示一个计时器的唯一标识，用于后续控制。所有字段均为 `internal`，外部通过扩展方法操作。

| 成员 | 类型 | 说明 |
| :--- | :--- | :--- |
| `SlotIndex` | `int` | 内部槽位索引（仅内部使用）。 |
| `Generation` | `int` | 代际，用于验证句柄有效性（仅内部使用）。 |
| `IsNull` | `bool` | 指示句柄是否为空。 |
| `Null` | `static TimerHandle` | 返回空句柄单例。 |

**扩展方法（来自 `Extension` 类）**

| 方法签名 | 作用 |
| :--- | :--- |
| `Cancel(this in TimerHandle handle)` | 取消该计时器。 |
| `Pause(this in TimerHandle handle)` | 暂停该计时器。 |
| `Resume(this in TimerHandle handle)` | 恢复该计时器。 |
| `SetPaused(this in TimerHandle handle, bool isPaused)` | 设置暂停状态。 |
| `IsActive(this in TimerHandle handle)` | 返回计时器是否仍处于活动状态。 |
| `TryGetTimeRemaining(this in TimerHandle handle, out float remaining)` | 获取剩余时间（秒或帧），成功返回 `true`。 |
| `TryGetProgress(this in TimerHandle handle, out float progress)` | 获取进度（0~1），成功返回 `true`。 |
| `Reset(this in TimerHandle handle)` | 重置计时器到初始间隔。 |
| `SetInterval(this in TimerHandle handle, float interval)` | 修改间隔。 |
| `SetLoop(this in TimerHandle handle, bool loop)` | 修改循环模式。 |
| `TryGetGroupId(...)` | 获取所属组 ID。 |
| `TryGetInterval(...)` | 获取间隔。 |
| `TryGetIsLoop(...)` | 获取循环状态。 |
| `TryGetFramesRemainingInt(...)` | 获取剩余帧数（仅对帧驱动有效）。 |


### 1.4 编辑器句柄 `EditorTimerHandle`（仅限 UNITY_EDITOR）

与 `TimerHandle` 结构完全相同，但专用于 `EditorTimer`。扩展方法位于 `EditorExtension` 类中，接口与 `Extension` 一致。


### 1.5 原子时间源相关类型

#### `TimeDelta` 枚举

定义时间流逝的计算方式：

- `Scaled`：`Time.deltaTime`（受缩放影响）
- `Unscaled`：`Time.unscaledDeltaTime`（不受缩放）
- `WallClock`：`Stopwatch` 高精度真实时间（后台继续）
- `Frame`：离散帧计数（每 Tick 推进 1）
- `Manual`：外部手动注入增量

#### `TimeSchedule` 枚举

定义驱动调度时机：

- `Update`：`MonoBehaviour.Update`
- `LateUpdate`：`MonoBehaviour.LateUpdate`
- `FixedUpdate`：`MonoBehaviour.FixedUpdate`
- `Coroutine`：`yield return null`
- `EndOfFrame`：`yield return WaitForEndOfFrame`
- `WaitForFixedUpdate`：`yield return WaitForFixedUpdate`
- `Manual`：外部手动调用 `ManualUpdate`

#### `TimeSource` 结构体

组合 `TimeDelta` 与 `TimeSchedule`，并提供预定义常用组合（如 `ScaledUpdate`、`FrameCoroutine` 等）。


### 1.6 可选值包装器 `Optional<T>`

用于表示可能不存在的值，常用于可选参数。

| 成员 | 说明 |
| :--- | :--- |
| `HasValue` | 是否包含值。 |
| `Value` | 获取值，若无值则抛出异常。 |
| `None` | 返回无值实例。 |
| `implicit operator Optional<T>(T value)` | 支持从 `T` 隐式转换。 |
| `explicit operator T(Optional<T> optional)` | 支持显式转换回 `T`。 |


## 2. 使用示例

### 2.1 基础计时

```csharp
using Timer;

public class Example : MonoBehaviour
{
    void Start()
    {
        // 每秒执行一次，受暂停影响（默认循环）
        var handle = GlobalTimer.RegisterScaled(1f, () => Debug.Log("Tick"));
        
        // 3 秒后执行一次（非循环）
        GlobalTimer.RegisterScaled(3f, () => Debug.Log("Delayed"), loop: false);
        
        // 不受暂停影响的计时器
        GlobalTimer.RegisterUnscaled(0.5f, () => Debug.Log("Unscaled"));
    }
}
```

### 2.2 帧驱动计时器

```csharp
// 每帧执行（等效于 Update）
GlobalTimer.RegisterMonoUpdate(() => Debug.Log("每帧"));

// 30 帧后执行
GlobalTimer.RegisterFrame(30, () => Debug.Log("30帧后"));
```

### 2.3 独立缩放与挂钟计时

```csharp
// 2 倍速运行，不受 Time.timeScale 影响
GlobalTimer.RegisterIndependent(1f, () => Debug.Log("2x Speed"), customScale: 2f);

// 后台挂机计时（应用切到后台继续）
GlobalTimer.RegisterWallClock(300f, () => Debug.Log("5 分钟到了"), loop: false);
```

### 2.4 手动驱动（适用于编辑器工具或自定义循环）

```csharp
var manualHandle = GlobalTimer.RegisterManual(1f, () => Debug.Log("Manual Tick"));

void Update()
{
    // 在自定义循环中推进
    GlobalTimer.ManualUpdate(Time.deltaTime);
}
```

### 2.5 句柄控制

```csharp
var handle = GlobalTimer.RegisterScaled(1f, OnTick);
handle.Pause();          // 暂停
handle.Resume();         // 恢复
handle.Cancel();         // 取消

if (handle.TryGetTimeRemaining(out float remaining))
    Debug.Log($"剩余: {remaining}s");

if (handle.TryGetProgress(out float progress))
    Debug.Log($"进度: {progress:P2}");

handle.Reset();          // 重置
handle.SetInterval(2f);  // 修改为 2 秒
handle.SetLoop(false);   // 改为单次
```

### 2.6 分组批量控制

```csharp
const int GROUP_ID = 100;

GlobalTimer.RegisterScaled(1f, () => Debug.Log("A"), groupID: GROUP_ID);
GlobalTimer.RegisterScaled(2f, () => Debug.Log("B"), groupID: GROUP_ID);

GlobalTimer.PauseGroup(GROUP_ID);   // 暂停整组
GlobalTimer.ResumeGroup(GROUP_ID);  // 恢复
GlobalTimer.CancelGroup(GROUP_ID);  // 取消整组
```

### 2.7 自定义原子组合

```csharp
// 使用通用注册方法组合 Unscaled + LateUpdate
GlobalTimer.Register(
    interval: 0.5f,
    callback: () => Debug.Log("Unscaled LateUpdate"),
    delta: TimeDelta.Unscaled,
    schedule: TimeSchedule.LateUpdate
);
```

### 2.8 编辑器计时器（仅限 UNITY_EDITOR）

```csharp
#if UNITY_EDITOR
public class MyEditorWindow : EditorWindow
{
    private void OnEnable()
    {
        // 在编辑器窗口激活时注册计时器
        EditorTimer.RegisterScaled(1f, () => {
            // 刷新窗口预览
            Repaint();
        }, loop: true);
    }

    private void OnDisable()
    {
        EditorTimer.CancelAll();
    }
}
#endif
```

### 2.9 全局清理

```csharp
// 场景切换时清理所有计时器
GlobalTimer.CancelAll();
```


## 3. 注意事项

- **性能**：所有运行时操作均零 GC，但请**缓存委托**避免 Lambda 闭包分配。
- **容量**：默认容量 2048，超过会返回空句柄并输出警告。
- **线程安全**：本工具库设计用于 Unity 主线程，不支持多线程并发。
- **编辑器支持**：在非运行状态下调用 `GlobalTimer` 会抛出异常，请改用 `EditorTimer`。
- **环境要求**：Unity 2020.3 或更高版本，仅支持运行时（PlayMode）和 Unity 编辑器环境。