> 内容由 AI 根据核心代码生成，已通过人工审核。

### 公共 API 简介

#### 全局注册 (GlobalTimer)

| API | 参数 | 说明 |
|-----|------|------|
| `RegisterMonoUpdate` | `Action action` / `(Action action, int groupID)` | 每帧 `Update` 执行，默认循环 |
| `RegisterMonoLateUpdate` | 同上 | 每帧 `LateUpdate` 执行，默认循环 |
| `RegisterMonoFixedUpdate` | `(Action action)` / `(float interval, Action action, bool loop = true)` / `(float interval, Action action, bool loop, int groupID)` | 物理帧 `FixedUpdate` 执行，可指定间隔 |
| `RegisterCoroutineUpdate` | `Action action` / `(Action action, int groupID)` | 协程 `yield return null` 后执行，默认循环 |
| `RegisterCoroutineEndOfFrame` | 同上 | 协程 `yield return WaitForEndOfFrame` 后执行，默认循环 |
| `RegisterScaled` | 重载：`(float, Action)` / `(TimeSpan, Action)` / `(..., bool loop)` / `(..., int groupID)` | 受 `Time.timeScale` 影响的时间计时，默认循环 |
| `RegisterUnscaled` | 同上 | 不受 `Time.timeScale` 影响，真实物理时间 |
| `RegisterFrame` | `(int frameCount, Action)` / `(..., bool loop)` / `(..., int groupID)` | 帧驱动，按帧数间隔执行，默认循环 |
| `CancelGroup` | `int groupId` | 取消指定组的所有计时任务 |
| `PauseGroup` / `ResumeGroup` | `int groupId` | 暂停/恢复指定组的所有任务 |
| `SetGroupPaused` | `int groupId, bool isPaused` | 设置指定组的暂停状态 |
| `CancelAll` | 无 | 取消所有计时任务，重置整个系统 |

#### 句柄扩展方法 (Extension)

| 方法 | 参数 | 说明 |
|------|------|------|
| `Cancel` | `this in TimerHandle` | 取消该计时任务 |
| `Pause` / `Resume` | 同上 | 暂停/恢复该任务 |
| `SetPaused` | `bool isPaused` | 设置暂停状态 |
| `IsActive` | 无 | 任务是否仍在系统中 |
| `TryGetTimeRemaining` | `out float` | 获取剩余时间（秒） |
| `TryGetProgress` | `out float` | 获取进度 0~1 |
| `Reset` | 无 | 重置剩余时间为间隔 |
| `SetInterval` | `float interval` | 修改间隔 |
| `TryGetGroupId` | `out int` | 获取所属组 ID |
| `TryGetInterval` | `out float` | 获取间隔 |
| `TryGetIsLoop` | `out bool` | 是否为循环 |
| `SetLoop` | `bool loop` | 设置循环标志 |
| `TryGetFramesRemaining` | `out int` | 获取剩余帧数（仅帧驱动有效） |

#### 句柄结构体 (TimerHandle)

- `SlotIndex`：槽位索引  
- `Generation`：代版本，用于验证句柄有效性  
- `IsNull`：是否为无效句柄  
- `Null`：静态无效句柄  

### 使用示例

```csharp
using Timer;

// 1. 延迟 2 秒执行一次（受缩放影响）
GlobalTimer.RegisterScaled(2f, () => Debug.Log("2 seconds later"), loop: false);

// 2. 每 0.5 秒循环播放音效（不受缩放影响，组管理）
int soundGroup = 10;
var handle = GlobalTimer.RegisterUnscaled(0.5f, () => PlayTickSound(), loop: true, groupID: soundGroup);

// 3. 每帧执行相机跟随
GlobalTimer.RegisterMonoUpdate(() => FollowTarget());

// 4. 每 10 帧执行一次
GlobalTimer.RegisterFrame(10, () => CheckCollision(), loop: true);

// 5. 物理帧计时（每 0.1 秒）
GlobalTimer.RegisterMonoFixedUpdate(0.1f, () => ApplyPhysicsForce());

// 6. 组操作
GlobalTimer.PauseGroup(soundGroup);   // 暂停所有音效计时
GlobalTimer.ResumeGroup(soundGroup);
GlobalTimer.CancelGroup(soundGroup);  // 彻底取消

// 7. 句柄高级操作
handle.Pause();
handle.TryGetProgress(out float p);   // 获取进度
handle.SetInterval(0.2f);             // 动态加快频率
handle.Cancel();
```