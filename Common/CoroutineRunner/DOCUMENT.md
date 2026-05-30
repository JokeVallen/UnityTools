> 内容由 AI 根据核心代码生成，已通过人工审核。

## 公共 API 简介

### `GlobalCoroutineRunner` 静态类

全局入口，所有方法委托给内部运行器。

```csharp
public static Coroutine StartCoroutine(IEnumerator routine)
```
启动原生协程，返回 `Coroutine` 对象。

```csharp
public static Coroutine StartCoroutine(string methodName, object value)
```
通过方法名启动协程（开销较大，不推荐）。

```csharp
public static void StopCoroutine(Coroutine coroutine)
public static void StopCoroutine(IEnumerator routine)
public static void StopCoroutine(string methodName)
public static void StopAllCoroutines()
```
停止协程。

```csharp
public static void ConfigureChannel(string channelName, int maxConcurrent)
```
配置并发通道的最大并发数。

```csharp
public static CoroutineHandleToken Run(IEnumerator routine)
```
启动可控协程（不排队，立即运行），返回句柄。

```csharp
public static CoroutineHandleToken RunQueued(IEnumerator routine, string channelName)
```
将可控协程送入指定通道排队执行。

```csharp
public static void Dispose()
```
释放全局运行器资源。

### `CoroutineHandleToken` 结构体

```csharp
public readonly struct CoroutineHandleToken
```
协程句柄，包含 `Id` 和 `Version`，用于安全操作。

- `bool IsValid { get; }`：句柄是否有效（Id > 0）。
- `static CoroutineHandleToken NullToken`：空句柄。

### 扩展方法（`Extensions` 类）

```csharp
public static void Pause(this CoroutineHandleToken token)
```
暂停协程。

```csharp
public static void Resume(this CoroutineHandleToken token)
```
恢复暂停的协程。

```csharp
public static void Cancel(this CoroutineHandleToken token)
```
取消协程。

```csharp
public static CoroutineState GetState(this CoroutineHandleToken token)
```
获取协程状态（无效时返回 `Completed`）。

```csharp
public static bool TryGetState(this CoroutineHandleToken token, out CoroutineState state)
```
尝试获取状态，返回 false 表示 token 无效。

```csharp
public static bool IsDone(this CoroutineHandleToken token)
```
判断协程是否已完成（包括 Completed 和 Canceled）。

```csharp
public static TaskAwaiter<bool> GetAwaiter(this CoroutineHandleToken token)
```
支持 `await token` 等待协程结束。

### `CustomYield` 静态工厂

```csharp
public static T Yield<T>() where T : CustomYieldInstructionBase, IPoolable, new()
```
获取无参数池化指令实例。

```csharp
public static T Yield<T>(object arg) where T : CustomYieldInstructionBase, IPoolable, new()
```
获取带参数池化指令（非泛型参数，可能装箱）。

```csharp
public static T1 Yield<T1, T2>(T2 arg) where T1 : CustomYieldInstructionBase, IPoolable, new()
```
获取带参数池化指令（强类型，零分配）。

```csharp
public static void Release(CustomYieldInstructionBase instruction)
```
将指令回收到池中。

### 内置自定义指令

- `WaitForSecondsControlled`：受 `Time.timeScale` 影响的秒数等待。
- `WaitForRealtimeSecondsControlled`：忽略 `timeScale` 的真实秒数等待。
- `WaitForFramesControlled`：等待指定帧数。
- `WaitForAsyncOperationControlled`：等待 `AsyncOperation` 完成。
- `WaitUntilControlled`：等待条件变为 `true`。
- `WaitWhileControlled`：等待条件变为 `false`。

所有指令均继承 `CustomYieldInstructionBase`，支持暂停/取消。

## 使用示例

```csharp
using CoroutineRunner;
using UnityEngine;

public class Example : MonoBehaviour
{
    async void Start()
    {
        // 启动可控协程
        var token = GlobalCoroutineRunner.Run(MyRoutine());
        
        // 3秒后暂停
        await System.Threading.Tasks.Task.Delay(3000);
        token.Pause();
        
        // 2秒后恢复
        await System.Threading.Tasks.Task.Delay(2000);
        token.Resume();
        
        // 等待协程结束
        await token;
        Debug.Log("协程结束");
    }
    
    IEnumerator MyRoutine()
    {
        Debug.Log("开始");
        // 池化等待（零分配）
        yield return CustomYield.Yield<WaitForSecondsControlled, float>(2f);
        Debug.Log("2秒后");
        
        // 等待下一帧
        yield return CustomYield.Yield<WaitForFramesControlled, int>(1);
        
        // 等待条件
        bool condition = false;
        yield return CustomYield.Yield<WaitUntilControlled, System.Func<bool>>(() => condition);
    }
}
```