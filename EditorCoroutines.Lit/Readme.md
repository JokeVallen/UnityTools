> 项目由 AI 和作者共同设计和开发，已进行基本的单元测试和功能测试，具体测试请查看 `Tests` 下相关文件。

# Unity 编辑器协程库 (Editor Coroutines)

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2020.3+-blue)](https://unity.com/)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Unity Test Framework](https://img.shields.io/badge/Unity%20Test%20Framework-passing-brightgreen)]()

一个轻量级、零依赖的 Unity 编辑器协程库，让你在编辑器中也能像运行时一样使用协程。支持嵌套协程、取消令牌、等待扩展、泛型返回值等功能，非常适合编辑器工具开发、资源导入流程、批处理任务等场景。

---

## ✨ 特性

- **纯编辑器实现**：仅作用于 `UNITY_EDITOR` 环境下，不影响运行时。
- **轻量简洁**：无外部依赖，核心代码不到 200 行。
- **嵌套协程**：自动展开嵌套的 `IEnumerator`，按顺序执行。
- **取消令牌**：通过 `EditorCoroutineCancelToken` 随时取消正在运行的协程。
- **丰富的等待扩展**：提供等待秒/毫秒、等待帧、等待条件（可超时）、延迟执行等扩展方法。
- **泛型返回值**：`EditorCoroutine<T>` 支持协程执行完毕后返回一个结果。
- **异常安全**：内置异常捕获，并提供 `onException` 回调。

---

## 📦 安装

### 注入源码

将 `EditorCoroutine.cs`、`EditorCoroutineCancelToken.cs`、`EditorCoroutineExtensions.Wait.cs`、`EditorCoroutineHelper.cs`、`EditorCoroutineWithResult.cs` 五个文件复制到你的 Unity 项目的 `Editor` 文件夹或任意符合 `UNITY_EDITOR` 条件的脚本文件夹中。

### 注入 DLL 文件

将 `EditorCoroutines.Lit.dll` 和 `EditorCoroutines.Lit.xml` 放入 Unity 项目的 Plugins 目录中。

---

## 🚀 快速开始

### 基本使用

```csharp
using EditorCoroutines.Lit;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class MyEditorTool
{
    [MenuItem("Tools/Start Editor Coroutine")]
    static void StartDemo()
    {
        EditorCoroutine.StartCoroutine(DemoRoutine());
    }

    static IEnumerator DemoRoutine()
    {
        Debug.Log("协程开始");
        yield return EditorCoroutineExtensions.WaitSeconds(2);
        Debug.Log("2秒后");
        yield return EditorCoroutineExtensions.WaitFrame();
        Debug.Log("下一帧");
    }
}
```

### 带取消令牌

```csharp
static EditorCoroutineCancelToken cancelToken;

[MenuItem("Tools/Start Cancellable")]
static void StartCancellable()
{
    cancelToken = new EditorCoroutineCancelToken();
    EditorCoroutine.StartCoroutine(LongRoutine(cancelToken));
}

[MenuItem("Tools/Cancel")]
static void Cancel()
{
    cancelToken?.Cancel();
}

static IEnumerator LongRoutine(EditorCoroutineCancelToken token)
{
    for (int i = 0; i < 100; i++)
    {
        if (token.IsCancelled) yield break;
        Debug.Log("Step " + i);
        yield return EditorCoroutineExtensions.WaitSeconds(0.5f, token);
    }
}
```

### 带返回值的协程

```csharp
[MenuItem("Tools/Coroutine With Result")]
static void TestWithResult()
{
    EditorCoroutine<int>.StartCoroutine(ComputeResult(), result =>
    {
        Debug.Log("计算结果: " + result);
    });
}

static IEnumerator ComputeResult()
{
    yield return EditorCoroutineExtensions.WaitSeconds(1);
    yield return 42; // 返回 int 类型结果
}
```

### 使用示例

将根目录下的 `example.unitypackage` 导入你的示例项目以便查看具体的应用

---

## 📚 API 文档

### `EditorCoroutine`

| 方法 / 属性 | 说明 |
|------------|------|
| `static StartCoroutine(IEnumerator, onComplete, onException)` | 启动一个编辑器协程。 |
| `Start()` | 启动（适用于延迟启动场景）。 |
| `Stop()` | 停止协程。 |
| `Dispose()` | 释放资源并停止协程。 |
| `IsRunning` | 是否正在运行。 |
| `IsCompleted` | 是否已完成。 |
| `Exception` | 捕获到的异常（如果有）。 |

### `EditorCoroutine<T>`

| 方法 / 属性 | 说明 |
|------------|------|
| `static StartCoroutine(IEnumerator, onComplete, onException)` | 启动一个带返回值的编辑器协程。 |
| `Result` | 执行结果（完成后有效）。 |
| 其他同 `EditorCoroutine`。 |

### `EditorCoroutineCancelToken`

| 方法 / 属性 | 说明 |
|------------|------|
| `Cancel()` | 发起取消请求。 |
| `IsCancelled` | 是否已取消。 |

### 扩展方法 (静态类 `EditorCoroutineExtensions`)

| 方法 | 说明 |
|------|------|
| `WaitSeconds(float, token)` | 等待指定秒数。 |
| `WaitMilliseconds(float, token)` | 等待指定毫秒数。 |
| `WaitFrame(token)` | 等待一帧。 |
| `WaitUntil(Func<bool> condition, token)` | 等待条件为真。 |
| `WaitUntil(Func<bool> condition, float timeoutSeconds, token)` | 等待条件为真，超时后自动结束。 |
| `Delay(Action action, float seconds, token)` | 延迟执行一个操作。 |

### `EditorCoroutineHelper`（内部类）

内部辅助类，提供 `WrapRoutine` 方法实现嵌套协程自动展开。无需手动调用。

---

## 📄 许可证

本项目采用 MIT 许可证。详情请参见 [LICENSE](LICENSE) 文件。

---

## 🤝 贡献

欢迎提交 Issue 或 Pull Request！如果你有好的建议或发现了 bug，请随时反馈。