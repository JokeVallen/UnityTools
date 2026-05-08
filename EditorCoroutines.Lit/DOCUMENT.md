> 内容由 AI 根据核心代码生成，已通过人工审核。

# EditorCoroutines.Lit API 文档

## 公共 API

### 1. `EditorCoroutine` 类（无返回值协程）

| 成员 | 签名 | 说明 |
|------|------|------|
| 属性 | `bool IsRunning { get; }` | 协程是否正在运行 |
| 属性 | `bool IsCompleted { get; }` | 协程是否已完成（正常结束或异常终止） |
| 属性 | `Exception Exception { get; }` | 协程执行中抛出的异常，若无异常则为 `null` |
| 静态方法 | `static EditorCoroutine StartCoroutine(IEnumerator routine, Action onComplete = null, Action<Exception> onException = null)` | 创建并启动一个编辑器协程。`routine` 为迭代器；`onComplete` 在正常完成时调用；`onException` 在发生异常时调用 |
| 实例方法 | `void Start()` | 手动启动协程（若已释放则抛出异常，已运行则忽略） |
| 实例方法 | `void Stop()` | 停止正在运行的协程，取消 `EditorApplication.update` 注册 |
| 实例方法 | `void Dispose()` | 释放协程资源，停止运行并清空回调。可多次调用 |

---

### 2. `EditorCoroutine<T>` 类（带返回值协程）

| 成员 | 签名 | 说明 |
|------|------|------|
| 属性 | `bool IsRunning { get; }` | 协程是否正在运行 |
| 属性 | `bool IsCompleted { get; }` | 协程是否已完成 |
| 属性 | `T Result { get; }` | 协程执行结果。通过 `yield return` 返回 `T` 类型值或 `Func<T>` 委托获取 |
| 属性 | `Exception Exception { get; }` | 协程执行中抛出的异常 |
| 静态方法 | `static EditorCoroutine<T> StartCoroutine(IEnumerator routine, Action<T> onComplete = null, Action<Exception> onException = null)` | 创建并启动带结果的编辑器协程。`onComplete` 接受结果值 |
| 实例方法 | `void Start()`, `void Stop()`, `void Dispose()` | 生命周期控制，行为同无返回值版本 |

> **获取返回值的方式**：在协程中 `yield return` 一个类型为 `T` 的值，或一个返回 `T` 的 `Func<T>` 委托，`Result` 属性会自动记录最近一次产生的值。建议在完成回调中读取 `Result`。

---

### 3. `EditorCoroutineCancelToken` 类

| 成员 | 签名 | 说明 |
|------|------|------|
| 属性 | `bool IsCancelled { get; }` | 指示是否已请求取消 |
| 方法 | `void Cancel()` | 设置取消标志为 `true`。多次调用安全 |

> 注意：该令牌仅用于在 `EditorCoroutineExtensions` 的等待方法中提前结束等待。**不会自动终止整个协程**，需要在协程中自行检查并 `yield break`，或直接调用 `EditorCoroutine.Stop()` 终止。

---

### 4. `EditorCoroutineExtensions` 静态扩展方法

| 方法 | 签名 | 说明 |
|------|------|------|
| `WaitSeconds` | `static IEnumerator WaitSeconds(float seconds, EditorCoroutineCancelToken token = null)` | 等待指定秒数（基于 `EditorApplication.timeSinceStartup`）。若提供令牌且已取消，则立即退出 |
| `WaitMilliseconds` | `static IEnumerator WaitMilliseconds(float milliseconds, EditorCoroutineCancelToken token = null)` | 等待指定毫秒数，内部调用 `WaitSeconds` |
| `WaitFrame` | `static IEnumerator WaitFrame(EditorCoroutineCancelToken token = null)` | 等待下一帧（`yield return null`）。支持取消令牌 |
| `WaitUntil` | `static IEnumerator WaitUntil(Func<bool> condition, EditorCoroutineCancelToken token = null)` | 等待条件为 `true`。每帧检查条件，支持取消 |
| `WaitUntil` (重载) | `static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds, EditorCoroutineCancelToken token = null)` | 带超时的条件等待。超时或取消时退出 |
| `Delay` | `static IEnumerator Delay(Action action, float seconds, EditorCoroutineCancelToken token = null)` | 延迟指定秒数后执行操作。如果令牌已取消则不执行 |

---

## 使用示例

### 启动一个简单协程

```csharp
EditorCoroutine.StartCoroutine(MyRoutine(), onComplete: () => {
    Debug.Log("完成");
});

IEnumerator MyRoutine() {
    yield return EditorCoroutineExtensions.WaitSeconds(1.0f);
    Debug.Log("1 秒后");
}
```

### 带返回值的协程

```csharp
EditorCoroutine<int>.StartCoroutine(GenerateResult(), onComplete: (result) => {
    Debug.Log("结果为：" + result);
});

IEnumerator GenerateResult() {
    yield return EditorCoroutineExtensions.WaitSeconds(0.5f);
    yield return 42; // 直接返回 int
    // 或者 yield return new Func<int>(() => 42);
}
```

### 嵌套协程

```csharp
IEnumerator Outer() {
    Debug.Log("Outer start");
    yield return Inner();
    Debug.Log("Outer end");
}

IEnumerator Inner() {
    yield return EditorCoroutineExtensions.WaitFrame();
    Debug.Log("Inner step");
}
```

### 使用取消令牌配合手动停止

```csharp
var token = new EditorCoroutineCancelToken();
var ec = EditorCoroutine.StartCoroutine(WaitAndCheck(token));

// 外部随时取消
token.Cancel();

IEnumerator WaitAndCheck(EditorCoroutineCancelToken token) {
    yield return EditorCoroutineExtensions.WaitSeconds(2.0f, token);
    if (token.IsCancelled) yield break; // 必须显式检查
    Debug.Log("继续执行");
}
```

### 直接调用 Stop() 彻底终止

```csharp
var ec = EditorCoroutine.StartCoroutine(LongTask());
// 用 Stop() 彻底移除协程，无需令牌
ec.Stop();
```