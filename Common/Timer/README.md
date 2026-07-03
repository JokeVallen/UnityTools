> 内容由 AI 根据核心代码生成，已通过人工审核。

# GlobalTimer – Unity 高性能计时器库

[![MIT License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Unity 2020.3+](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com)
[![Test Framework](https://img.shields.io/badge/Test%20Framework-1.1.33-blue)](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1)
[![Performance Testing](https://img.shields.io/badge/Performance%20Testing-3.0.3-blue)](https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.0)
[![Zero GC](https://img.shields.io/badge/Zero%20GC-brightgreen.svg)](https://github.com)

**GlobalTimer** 是一个为 Unity 设计的轻量级、零 GC、多时间源计时器工具库。它提供了比 `Invoke`、协程更强大、更灵活的计时能力，支持缩放/未缩放时间、MonoBehaviour 生命周期、协程、物理帧、帧驱动等多种时间源，并内置组管理、动态间隔调整、进度查询等功能。


### 工具库简介

在游戏开发中，计时任务无处不在：技能冷却、Buff 倒计时、延迟销毁、周期性攻击、UI 动画…… Unity 自带的 `Invoke` 性能差且无法动态调整；协程会产生 GC 分配；手写 `Update` 累加代码重复且难以复用。

**GlobalTimer** 一次性解决所有痛点：
- **零 GC**：内部使用对象池和结构体句柄，无运行时堆分配。
- **多时间源**：覆盖游戏开发中 99% 的计时场景。
- **完全可控**：暂停、恢复、取消、动态改间隔、查询进度/剩余时间。
- **组管理**：批量取消/暂停，轻松管理技能组、敌人波次等。
- **高性能**：单次操作 < 0.5ms，数千并发计时器每帧开销 < 0.07ms。


### 安装环境要求

- Unity 2020.3 或更高版本（支持 .NET Standard 2.0）
- 支持运行时（PlayMode）和编辑器非运行模式（EditMode）
- 必须从主线程调用


### 安装方式

#### 方式一：源码导入

1. 将 `Timer` 文件夹复制到 Unity 项目的 `Assets` 目录下。
2. 在需要使用计时器的脚本顶部添加 `using Timer;`。

#### 方式二：DLL 导入

1. 将 `Timer.dll`（目标框架 .NET Standard 2.0）放入 `Assets/Plugins` 目录。
2. 同样使用 `using Timer;` 引用。

### 设计理念

- **原子组合，无限表达**：将时间源拆解为独立的 `TimeDelta`（时间如何流逝）和 `TimeSchedule`（何时驱动检查），通过正交组合覆盖任意计时场景，而非硬编码枚举。
- **安全优先**：句柄代际验证防止悬挂引用；回调中取消/注册任务不会破坏内部遍历；`pendingCallbacks` 边界保护防止数组越界。
- **零 GC 友好**：所有操作均使用结构体和数组复用，避免委托和临时对象分配。
- **性能与简洁平衡**：不盲目追求“最小原语”，提供直接的组操作、`CancelAll` 等方法以提升易用性。
- **双环境兼容**：运行时由 `MonoBehaviour` 驱动，编辑器由 `EditorApplication.update` 驱动，两套体系 API 完全一致，无缝切换。


### 原子时间源设计

本库将传统“硬编码时间源”拆解为两个正交的原子维度：

1. **`TimeDelta`**：定义时间如何流逝（Scaled/Unscaled/WallClock/Frame/Manual）。
2. **`TimeSchedule`**：定义何时检查并推进计时器（Update/LateUpdate/FixedUpdate/Coroutine/EndOfFrame/WaitForFixedUpdate/Manual）。

两者自由组合，可表达任意时间源需求。预置的注册方法（如 `RegisterScaled`）只是常用组合的快捷方式，而非全部能力。这一设计保证了工具库的**无限扩展性**。


### 具体功能说明

#### 1. 多时间源支持

本库采用 **原子组合模型**：`TimeDelta`（增量计算方式）× `TimeSchedule`（驱动调度时机）。以下为预置常用组合及对应的注册方法：

| 时间源 | 注册方法 | 特点 |
|--------|----------|------|
| 缩放时间 + Update | `RegisterScaled` | 受 `Time.timeScale` 影响，适合游戏逻辑 |
| 未缩放时间 + Update | `RegisterUnscaled` | 不受缩放影响，适合 UI、过场、网络心跳 |
| 帧驱动 + Update | `RegisterFrame` / `RegisterMonoUpdate` | 按帧数间隔，适合帧同步逻辑 |
| 缩放时间 + LateUpdate | `RegisterMonoLateUpdate` | 在 `LateUpdate` 中按缩放时间执行 |
| 缩放时间 + FixedUpdate | `RegisterMonoFixedUpdate` | 物理帧执行，受缩放影响 |
| 未缩放时间 + FixedUpdate | `RegisterMonoFixedUnscaled` | 物理帧执行，不受缩放影响 |
| 帧驱动 + Coroutine | `RegisterCoroutineUpdate` | `yield return null` 后执行 |
| 帧驱动 + WaitForFixedUpdate | `RegisterCoroutineWaitForFixedUpdate` | `yield return WaitForFixedUpdate` 后执行 |
| 帧驱动 + EndOfFrame | `RegisterCoroutineEndOfFrame` | `yield return WaitForEndOfFrame` 后执行 |
| 独立缩放 (Unscaled × customScale) | `RegisterIndependent` | 自定义倍率，不受全局缩放影响 |
| 挂钟时间 (WallClock) | `RegisterWallClock` | 基于 `Stopwatch`，App 后台继续流逝 |
| 手动驱动 | `RegisterManual` | 需外部调用 `ManualUpdate` 推进，适用于编辑器工具或自定义循环 |

**自定义组合**：以上均为基础原子组合，如有特殊需求，可使用通用 `Register(interval, callback, TimeDelta, TimeSchedule, ...)` 自由拼接。

#### 2. 循环与单次

所有注册方法均支持 `loop` 参数：`true` 为循环，`false` 为单次。循环计时器可通过句柄的 `SetLoop` 动态修改，即使计时器已注册。

#### 3. 组管理

注册时可指定 `groupID`。之后可批量：
- `CancelGroup(int groupId)`：取消整组
- `PauseGroup(int groupId)` / `ResumeGroup(int groupId)`：暂停/恢复整组
- `SetGroupPaused(int groupId, bool isPaused)`：统一设置

#### 4. 句柄操作

每个注册返回一个 `TimerHandle`，支持：
- `Cancel`、`Pause`、`Resume`、`SetPaused`
- `IsActive` 检查是否存活
- `TryGetTimeRemaining`、`TryGetProgress` 查询状态
- `Reset` 重置剩余时间
- `SetInterval` 动态改变间隔
- `SetLoop` 动态改变循环标志
- `TryGetGroupId`、`TryGetInterval`、`TryGetIsLoop` 查询元数据
- `TryGetFramesRemainingInt` 获取剩余帧数（**仅对帧驱动类型有效**）

#### 5. 全局控制

`GlobalTimer.CancelAll()` 可取消所有计时任务并清空待执行回调，重置内部状态，适用于场景切换、游戏重置等场景。

#### 6. 编辑器支持（EditorTimer）

`EditorTimer` 是与 `GlobalTimer` API 完全一致的编辑器版本，用于 Unity 编辑器非运行模式下的计时需求：

- **API 完全一致**：`RegisterScaled`、`RegisterFrame`、`RegisterIndependent` 等方法签名完全相同。
- **驱动方式**：由 `EditorApplication.update` 驱动，不依赖 `MonoBehaviour` 生命周期。
- **支持范围**：仅支持 `TimeSchedule.Update` 和 `TimeSchedule.Manual` 调度（传入 `LateUpdate`、`FixedUpdate`、`Coroutine` 等会抛出 `NotSupportedException`）。
- **程序集重载**：代码修改触发重载后，所有编辑器计时器任务会被重置，这是 Unity 编辑器机制的正常行为。

```csharp
#if UNITY_EDITOR
// 在编辑器窗口中使用
public class MyEditorWindow : EditorWindow
{
    private EditorTimerHandle _handle;
    
    private void OnEnable()
    {
        _handle = EditorTimer.RegisterScaled(1f, () => Repaint(), loop: true);
    }
    
    private void OnDisable()
    {
        _handle.Cancel();
    }
}
#endif
```


### 常见问题

**Q1：计时器在编辑器非播放模式下报错怎么办？**  
A：运行时请使用 `GlobalTimer`，编辑器非运行模式下请使用 `EditorTimer`。两者 API 完全一致。

**Q2：最多支持多少个并发计时器？**  
A：默认容量 2048，超过后 `Register` 会返回 `TimerHandle.Null`。如需更大容量，可修改 `InnerRuntimeTimer` 或 `InnerEditorTimer` 构造函数中的 `capacity` 参数。

**Q3：回调中是否可以注册新计时器？**  
A：可以，新注册的任务会设置 `skipCurrentFrame` 标志，保证最早下一帧触发，不会在本帧的剩余循环中意外执行。

**Q4：计时器会跨场景持续运行吗？**  
A：会。运行时 `InnerRuntimeTimer` 的 `Proxy` 组件挂载在 `DontDestroyOnLoad` 对象上，场景切换不会自动清除计时任务。如需场景重置时清理，请手动调用 `CancelAll` 或通过组取消。

**Q5：物理帧计时器的间隔如何理解？**  
A：`RegisterMonoFixedUpdate` 默认使用 `Time.fixedDeltaTime`（通常 0.02 秒）作为间隔。你也可以传入自定义间隔（如 0.1 秒），系统会每隔 `interval` 秒触发一次（在物理帧中检查）。

**Q6：如何自定义一个“未缩放 + LateUpdate”的计时器？**  
A：使用通用注册方法：
```csharp
GlobalTimer.Register(
    interval: 0.5f,
    callback: () => Debug.Log("Unscaled + LateUpdate"),
    delta: TimeDelta.Unscaled,
    schedule: TimeSchedule.LateUpdate
);
```
所有 `TimeDelta`（Scaled/Unscaled/WallClock/Frame/Manual）与 `TimeSchedule`（Update/LateUpdate/FixedUpdate/Coroutine/EndOfFrame/WaitForFixedUpdate/Manual）可任意组合，共 35 种可能。

**Q7：`RegisterMonoFixedUpdate` 和 `RegisterMonoFixedUnscaled` 有什么区别？**  
A：前者使用 `Time.fixedDeltaTime`（受 `Time.timeScale` 影响），适用于受暂停控制的物理逻辑；后者使用 `Time.fixedUnscaledDeltaTime`（不受缩放影响），适用于需要物理帧但不应受暂停影响的逻辑，如网络同步、输入处理。

**Q8：`GlobalTimer` 和 `EditorTimer` 有什么区别？**  
A：`GlobalTimer` 用于运行时（PlayMode），由 `MonoBehaviour` 驱动；`EditorTimer` 用于编辑器非运行模式（EditMode），由 `EditorApplication.update` 驱动。两者 API 完全一致，但底层驱动独立，计时任务不共享。编辑器版本仅支持 `Update` 和 `Manual` 调度。

**Q9：编辑器模式下程序集重载后计时器会怎样？**  
A：编辑器模式下，修改代码触发程序集重载（Assembly Reload）后，所有 `EditorTimer` 任务会被重置。这是 Unity 编辑器域重载机制决定的正常行为，不影响运行时（PlayMode）的计时器。


### 其他文档

- [API 详细文档](./source/1.0.2-beta/DOCUMENT.md)
- [测试报告](./tests/1.0.2-beta/TEST_REPORT.md)


### 许可证

[MIT](../../LICENSE)