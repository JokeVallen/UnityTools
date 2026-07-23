# CoroutineRunner 使用文档（v1.0.1-beta）

> 内容由 AI 根据核心代码生成，已通过人工审核。


## 一、概述

**CoroutineRunner** 是一个高性能、零分配热路径的 Unity 协程管理框架，在原生协程基础上提供了**暂停/恢复**、**取消控制**、**状态查询**、**并发限流**以及**池化自定义指令**等完整能力。

### 版本信息

- **当前版本**：1.0.1-beta
- **Unity 兼容性**：2020.3 LTS 及以上
- **C# 兼容性**：7.0 及以上
- **.NET Standard**：2.0


## 二、公共 API 参考

### 2.1 `GlobalCoroutineRunner` 静态类

全局入口，所有方法委托给内部单例运行器。

#### 原生协程兼容 API

```csharp
public static Coroutine StartCoroutine(IEnumerator routine)
```
启动原生协程，返回 `Coroutine` 对象。与 `MonoBehaviour.StartCoroutine` 行为一致。

```csharp
public static Coroutine StartCoroutine(string methodName, object value)
```
通过方法名启动协程（**运行时开销较大**，不推荐热路径使用）。

```csharp
public static void StopCoroutine(Coroutine coroutine)
public static void StopCoroutine(IEnumerator routine)
public static void StopCoroutine(string methodName)
public static void StopAllCoroutines()
```
停止协程，与原生 API 行为一致。

#### 可控协程 API

```csharp
public static CoroutineHandleToken Run(IEnumerator routine)
```
启动一个**可控协程**，不排队，立即运行。返回 `CoroutineHandleToken` 句柄，可用于后续暂停/恢复/取消/状态查询。

```csharp
public static CoroutineHandleToken RunQueued<T>(IEnumerator routine, T channelKey)
```
将可控协程送入指定通道排队执行。通道可配置最大并发数，适用于资源敏感或需要保序的场景。

> **类型参数 `T` 说明**：通道键支持任意类型（`string`、`int`、`enum` 等）。相同 `T` 类型的通道共用存储容器，但不同类型之间相互隔离。

#### 通道配置

```csharp
public static void ConfigureChannel<T>(T channelKey, int maxConcurrent)
```
配置指定通道的最大并发数。
- `maxConcurrent <= 0`：不限制并发
- `maxConcurrent > 0`：限制同时运行的协程数量

> 通道必须在使用前配置，重复配置会抛出 `InvalidOperationException`。若未配置直接调用 `RunQueued`，将使用默认并发数 1 创建通道（单并发，强制排队）。

#### 资源释放

```csharp
public static void Dispose()
```
释放全局运行器资源，销毁底层 `GameObject`。**应用退出时自动调用**，一般情况下无需手动调用。


### 2.2 `CoroutineHandleToken` 结构体

协程句柄令牌，包含 `Id` 和 `Version`，用于安全地操作协程。

```csharp
public readonly struct CoroutineHandleToken
```

| 成员 | 说明 |
|------|------|
| `bool IsValid { get; }` | 句柄是否有效（`Id > 0`） |
| `static CoroutineHandleToken None { get; }` | 空句柄单例 |
| `bool Equals(CoroutineHandleToken other)` | 值相等比较（比较 Id + Version） |
| `static bool operator == / !=` | 相等/不等运算符重载 |

> **安全设计**：Token 包含 `Version` 字段，每次协程句柄被回收再分配时版本号递增，防止过期 Token 误操作新协程。


### 2.3 扩展方法（`Extensions` 类）

所有扩展方法均作用于 `in CoroutineHandleToken`，支持 `ref readonly` 参数传递，避免结构体复制。

```csharp
public static void Pause(this in CoroutineHandleToken token)
```
暂停协程。若协程已完成或已取消，调用无效果。

```csharp
public static void Resume(this in CoroutineHandleToken token)
```
恢复暂停的协程。仅对 `Paused` 状态有效。

```csharp
public static void Cancel(this in CoroutineHandleToken token)
```
取消协程。协程会立即终止并转为 `Canceled` 状态。

```csharp
public static CoroutineState GetState(this in CoroutineHandleToken token)
```
获取协程状态。若 Token 无效，返回 `CoroutineState.Completed`。

```csharp
public static bool TryGetState(this in CoroutineHandleToken token, out CoroutineState state)
```
尝试获取状态，返回 `false` 表示 Token 无效（协程已不存在）。

```csharp
public static bool IsDone(this in CoroutineHandleToken token)
```
判断协程是否已完成（`Completed` 或 `Canceled` 状态）。

```csharp
public static TaskAwaiter<bool> GetAwaiter(this CoroutineHandleToken token)
```
支持 `await token` 语法，等待协程结束。

```csharp
// 使用示例
await token;  // 等待协程执行完成
```


### 2.4 `CoroutineState` 枚举

```csharp
public enum CoroutineState
{
    Running,    // 正在运行
    Paused,     // 已暂停
    Completed,  // 正常执行完成
    Canceled    // 已被外部或异常取消
}
```


### 2.5 `CustomYield` 静态工厂

池化自定义指令的工厂类。所有通过工厂获取的指令实例均来自对象池，**在泛型 API 下实现零 GC 分配**。

```csharp
public static T Yield<T>() where T : CustomYieldInstructionBase, IPoolable, new()
```
获取无参数池化指令实例。`T` 必须实现 `IPoolable` 接口并提供无参构造函数。

```csharp
public static T Yield<T>(object arg) where T : CustomYieldInstructionBase, IPoolable, new()
```
获取带参数池化指令。**参数为 `object` 类型，值类型参数会发生装箱**，产生 1 byte GC 分配。不推荐热路径使用。

```csharp
public static T1 Yield<T1, T2>(T2 arg) where T1 : CustomYieldInstructionBase, IPoolable, new()
```
**⭐ 推荐 API**：获取带参数的池化指令，强类型参数，**零装箱、零分配**。

```csharp
public static void Release(CustomYieldInstructionBase instruction)
```
将指令实例回收到池中。**框架内部自动回收** `yield return` 使用的指令，一般情况下无需手动调用。
```

### 2.6 内置自定义指令

所有指令继承自 `CustomYieldInstructionBase`，**原生支持暂停/取消感知**。

| 指令 | 说明 | 参数类型 |
|------|------|----------|
| `WaitForSecondsControlled` | 等待指定秒数（受 `Time.timeScale` 影响） | `float` / `int` |
| `WaitForRealtimeSecondsControlled` | 等待指定秒数（忽略 `Time.timeScale`） | `float` / `int` |
| `WaitForFramesControlled` | 等待指定帧数 | `int` |
| `WaitForAsyncOperationControlled` | 等待 `AsyncOperation` 完成 | `AsyncOperation` |
| `WaitUntilControlled` | 等待条件变为 `true` | `Func<bool>` |
| `WaitWhileControlled` | 等待条件变为 `false` | `Func<bool>` |


### 2.7 扩展自定义指令

继承 `CustomYieldInstructionBase` 并实现 `KeepWaiting()` 方法，同时实现 `IPoolableYieldInstruction<T>` 或 `IPoolableYieldInstruction` 接口以接入池化。

```csharp
public sealed class WaitForCustomControlled : CustomYieldInstructionBase,
    IPoolableYieldInstruction<int>
{
    private int counter;

    public void Reset(int count) => counter = count;

    protected override bool KeepWaiting()
    {
        counter--;
        return counter > 0;
    }
}
```

使用方式：
```csharp
yield return CustomYield.Yield<WaitForCustomControlled, int>(10);
```


## 三、使用示例

### 3.1 基本用法

```csharp
using CoroutineRunner;
using UnityEngine;

public class Example : MonoBehaviour
{
    private IEnumerator MyRoutine()
    {
        Debug.Log("开始执行");
        yield return new WaitForSeconds(1f);
        Debug.Log("1秒后");
    }

    private void Start()
    {
        // 启动可控协程
        var token = GlobalCoroutineRunner.Run(MyRoutine());
        
        // 检查状态
        Debug.Log($"协程状态: {token.GetState()}");
    }
}
```

### 3.2 暂停与恢复

```csharp
private CoroutineHandleToken token;

private void Start()
{
    token = GlobalCoroutineRunner.Run(LongRoutine());
}

private void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        if (token.GetState() == CoroutineState.Running)
            token.Pause();
        else if (token.GetState() == CoroutineState.Paused)
            token.Resume();
    }
}

private IEnumerator LongRoutine()
{
    while (true)
    {
        Debug.Log("运行中...");
        yield return CustomYield.Yield<WaitForSecondsControlled, float>(0.5f);
    }
}
```

### 3.3 通道排队（并发限流）

```csharp
// 配置通道：最多同时运行 3 个协程
GlobalCoroutineRunner.ConfigureChannel("download", 3);

// 批量启动，自动排队
for (int i = 0; i < 20; i++)
{
    int id = i;
    var token = GlobalCoroutineRunner.RunQueued(DownloadRoutine(id), "download");
}

private IEnumerator DownloadRoutine(int id)
{
    Debug.Log($"开始下载 {id}");
    yield return CustomYield.Yield<WaitForSecondsControlled, float>(Random.Range(1f, 3f));
    Debug.Log($"下载完成 {id}");
}
```

### 3.4 使用 `await` 等待协程完成

```csharp
private async void Start()
{
    var token = GlobalCoroutineRunner.Run(MyRoutine());
    
    // 等待协程完成（非阻塞）
    await token;
    
    Debug.Log("协程已结束，继续执行后续逻辑");
}
```

### 3.5 使用池化 CustomYield（零分配）

```csharp
private IEnumerator OptimizedRoutine()
{
    // ✅ 推荐：泛型 API，零分配
    yield return CustomYield.Yield<WaitForSecondsControlled, float>(2.5f);
    yield return CustomYield.Yield<WaitForFramesControlled, int>(5);
    yield return CustomYield.Yield<WaitUntilControlled, System.Func<bool>>(() => someCondition);
    
    // ⚠️ 非泛型 API：值类型参数会装箱（1 byte GC）
    yield return CustomYield.Yield(typeof(WaitForSecondsControlled), 1.5f);
}
```


## 四、性能建议

| 建议 | 说明 |
|------|------|
| **优先使用泛型 `Yield<T1, T2>`** | 热路径零分配，速度是非泛型版本的约 2.2 倍 |
| **控制操作可高频调用** | `Pause`/`Resume`/`Cancel` 均为零分配，亚微秒级 |
| **合理配置通道并发数** | 根据资源限制设置 `maxConcurrent`，防止资源耗尽 |
| **通道键类型一致** | 使用相同类型作为通道键（如统一用 `string`），避免类型隔离导致的容器重复创建 |
| **避免非泛型 `Yield`** | 值类型参数会产生装箱分配 |