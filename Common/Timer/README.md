> 内容由 AI 根据核心代码生成，已通过人工审核。

# GlobalTimer – Unity 高性能计时器库

[![MIT License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Unity 2020.3+](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com)
[![Test Framework](https://img.shields.io/badge/Test%20Framework-1.1.33-blue)](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1)
[![Performance Testing](https://img.shields.io/badge/Performance%20Testing-3.0.3-blue)](https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.0)

**GlobalTimer** 是一个为 Unity 设计的轻量级、零 GC、多时间源计时器工具库。它提供了比 `Invoke`、协程更强大、更灵活的计时能力，支持缩放/未缩放时间、MonoBehaviour 生命周期、协程、物理帧、帧驱动等多种时间源，并内置组管理、动态间隔调整、进度查询等功能。

### 工具库简介

在游戏开发中，计时任务无处不在：技能冷却、Buff 倒计时、延迟销毁、周期性攻击、UI 动画…… Unity 自带的 `Invoke` 性能差且无法动态调整；协程会产生 GC 分配；手写 `Update` 累加代码重复且难以复用。

**GlobalTimer** 一次性解决所有痛点：
- **零 GC**：内部使用对象池和结构体句柄，无堆分配。
- **多时间源**：覆盖游戏开发中 99% 的计时场景。
- **完全可控**：暂停、恢复、取消、动态改间隔、查询进度/剩余时间。
- **组管理**：批量取消/暂停，轻松管理技能组、敌人波次等。
- **高性能**：单次操作 < 0.5ms，数千并发计时器每帧开销 < 0.07ms。

### 安装环境要求

- Unity 2020.3 或更高版本（支持 .NET Standard 2.0）
- 仅支持运行时（PlayMode），不支持编辑器非播放模式
- 必须从主线程调用

### 安装方式

#### 方式一：源码导入

1. 将 `Timer` 文件夹（包含 `GlobalTimer.cs`、`InnerTimer.cs`、`TimerHandle.cs`、`Extension.cs`、`TimeSource.cs`）复制到 Unity 项目的 `Assets/Scripts` 目录下。
2. 在需要使用计时器的脚本顶部添加 `using Timer;`。

#### 方式二：DLL 导入

1. 将项目编译为 `Timer.dll`（目标框架 .NET Standard 2.0）。
2. 将 DLL 放入 `Assets/Plugins` 目录。
3. 同样使用 `using Timer;` 引用。

### 设计理念

- **最小上层 API，最大表达力**：`GlobalTimer` 提供所有注册入口，`TimerHandle` 提供链式操作，避免繁杂配置。
- **安全优先**：句柄代验证防止悬挂引用；回调中取消/注册任务不会破坏内部遍历。
- **零 GC 友好**：所有操作均使用结构体和数组复用，避免委托和临时对象分配。
- **性能与简洁平衡**：不盲目追求“最小原语”，而是提供直接的组操作等方法以提升易用性和性能。

### 具体功能说明

#### 1. 多时间源支持

| 时间源 | 注册方法 | 特点 |
|--------|----------|------|
| 缩放时间 | `RegisterScaled` | 受 `Time.timeScale` 影响，适合游戏逻辑 |
| 未缩放时间 | `RegisterUnscaled` | 不受缩放影响，适合 UI、过场、网络心跳 |
| MonoBehaviour Update | `RegisterMonoUpdate` | 每帧 `Update` 执行 |
| MonoBehaviour LateUpdate | `RegisterMonoLateUpdate` | 每帧 `LateUpdate` 执行 |
| MonoBehaviour FixedUpdate | `RegisterMonoFixedUpdate` | 物理帧执行，支持自定义间隔 |
| 协程每帧 | `RegisterCoroutineUpdate` | `yield return null` 后执行 |
| 协程渲染后 | `RegisterCoroutineEndOfFrame` | `yield return WaitForEndOfFrame` 后执行 |
| 帧驱动 | `RegisterFrame` | 按整帧数间隔执行 |

#### 2. 循环与单次

所有注册方法均支持 `loop` 参数：`true` 为循环，`false` 为单次。循环计时器可通过句柄的 `SetLoop` 动态修改。

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
- `TryGetFramesRemaining` 获取剩余帧数（帧驱动专用）

#### 5. 全局控制

`GlobalTimer.CancelAll()` 可取消所有计时任务并重置内部状态，适合场景切换等场景。

### 常见问题

**Q1：计时器在编辑器非播放模式下报错怎么办？**  
A：该库设计为运行时使用。如需编辑器预览，可自行封装一个基于 `EditorApplication.update` 的模拟层。

**Q2：最多支持多少个并发计时器？**  
A：默认容量 2048，超过后 `Register` 会返回 `TimerHandle.Null`。如需要更大容量，可修改 `InnerTimer` 构造函数中的 `capacity` 参数。

**Q3：回调中是否可以注册新计时器？**  
A：可以，新注册的任务会设置 `SkipCurrentFrame` 标志，保证最早下一帧触发，不会在本帧的剩余循环中意外执行。

**Q4：计时器会跨场景持续运行吗？**  
A：会。`InnerTimer` 的 `Proxy` 组件挂载在 `DontDestroyOnLoad` 对象上，场景切换不会自动清除计时任务。如需场景重置时清理，请手动调用 `CancelAll` 或通过组取消。

**Q5：物理帧计时器的间隔如何理解？**  
A：`RegisterMonoFixedUpdate` 默认使用 `Time.fixedDeltaTime`（通常 0.02 秒）作为间隔。你也可以传入自定义间隔（如 0.1 秒），系统会每隔 `interval` 秒触发一次（在物理帧中检查）。

### 其他文档

- [API 详细文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)

### 许可证

[MIT](./LICENSE)