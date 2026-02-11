# Unity Editor Coroutines 编辑器协程工具库

一个轻量级、高效的 Unity 编辑器协程解决方案，让你能够在编辑器模式下优雅地处理异步操作，避免界面卡顿。

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity Version](https://img.shields.io/badge/unity-2020.3%2B-brightgreen.svg)](https://unity.com/)

## 📋 目录

- [功能特性](#功能特性)
- [快速开始](#快速开始)
- [安装](#安装)
- [API 文档](#api-文档)
- [使用示例](#使用示例)
- [最佳实践](#最佳实践)

## ✨ 功能特性

### 核心功能

- ✅ **编辑器协程执行** - 在 EditorApplication.update 中运行协程，完全支持 yield return
- ✅ **泛型返回值** - 通过 `EditorCoroutine<T>` 获取协程执行结果
- ✅ **取消令牌支持** - 使用 `EditorCoroutineCancelToken` 优雅地中止协程
- ✅ **嵌套协程** - 自动处理嵌套迭代器，支持复杂的协程链
- ✅ **异常捕获** - 内置异常处理机制，不会导致编辑器崩溃
- ✅ **完整的等待方法** - WaitFrame、WaitSeconds、WaitUntil、Delay 等
- ✅ **生命周期管理** - Start/Stop/Dispose 完整的资源管理

### 无依赖

- 不依赖任何第三方库
- 仅使用 Unity 内置的 Editor 和 Coroutine API
- 最小化代码体积

---

## 🚀 快速开始

### 基础用法

```csharp
// 简单的编辑器协程
EditorCoroutine.StartCoroutine(MyCoroutine());

private static IEnumerator MyCoroutine()
{
    Debug.Log("开始");
    yield return EditorCoroutineExtensions.WaitSeconds(1f);
    Debug.Log("1秒后");
}
```

### 带回调的协程

```csharp
EditorCoroutine.StartCoroutine(
    MyCoroutine(),
    onComplete: () => Debug.Log("完成"),
    onException: (ex) => Debug.LogError($"出错：{ex.Message}")
);
```

### 获取返回值

```csharp
var coroutine = EditorCoroutine<int>.StartCoroutine(
    CalculateAsync(),
    onComplete: (result) => Debug.Log($"结果：{result}")
);

private static IEnumerator CalculateAsync()
{
    yield return EditorCoroutineExtensions.WaitSeconds(1f);
    yield return (Func<int>)(() => 42);
}
```

### 取消协程

```csharp
var token = new EditorCoroutineCancelToken();
EditorCoroutine.StartCoroutine(LongRunningTask(token));

// 某处取消
token.Cancel();

private static IEnumerator LongRunningTask(EditorCoroutineCancelToken token)
{
    for (int i = 0; i < 100; i++)
    {
        if (token.IsCancelled)
            yield break;
        
        yield return EditorCoroutineExtensions.WaitFrame();
    }
}
```

---

## 📦 安装

### 方式一：直接复制源文件

将 `Core` 目录下的所有 `*.cs` 文件复制到你的项目中：

```
Assets/
└── Plugins/
    └── EditorCoroutines/
        ├── EditorCoroutine.cs
        ├── EditorCoroutine.cs
        ├── EditorCoroutineCancelToken.cs
        ├── EditorCoroutineExtensions.Wait.cs
        └── EditorCoroutineHelper.cs
```

### 方式二：导入dll文件

在 `Assets/Plugins/Editor/` 目录下放置编译好的 `EditorCoroutines.Lit.dll` 和 `EditorCoroutines.Lit.xml` 文件。

---

## 📚 API 文档

### EditorCoroutine

基础协程类，用于执行无返回值的异步操作。

```csharp
// 启动协程
public static EditorCoroutine StartCoroutine(
    IEnumerator routine,
    Action onComplete = null,
    Action<Exception> onException = null
)

// 属性
public bool IsRunning { get; }        // 是否正在运行
public bool IsCompleted { get; }      // 是否已完成
public Exception Exception { get; }   // 异常信息

// 方法
public void Start()    // 开始执行
public void Stop()     // 停止执行
public void Dispose()  // 释放资源
```

### EditorCoroutine\<T\>

泛型协程类，用于执行有返回值的异步操作。

```csharp
// 启动协程
public static EditorCoroutine<T> StartCoroutine(
    IEnumerator routine,
    Action<T> onComplete = null,
    Action<Exception> onException = null
)

// 属性
public T Result { get; }              // 执行结果
public bool IsRunning { get; }        // 是否正在运行
public bool IsCompleted { get; }      // 是否已完成
public Exception Exception { get; }   // 异常信息
```

### EditorCoroutineCancelToken

协程取消令牌，用于优雅地中止正在执行的协程。

```csharp
// 属性
public bool IsCancelled { get; }      // 是否已取消

// 方法
public void Cancel()                  // 设置取消标志
```

### EditorCoroutineExtensions

静态扩展方法，提供各种等待操作。

```csharp
// 等待一帧
public static IEnumerator WaitFrame(EditorCoroutineCancelToken token = null)

// 等待指定秒数
public static IEnumerator WaitSeconds(float seconds, EditorCoroutineCancelToken token = null)

// 等待指定毫秒数
public static IEnumerator WaitMilliseconds(float milliseconds, EditorCoroutineCancelToken token = null)

// 等待条件为真
public static IEnumerator WaitUntil(
    Func<bool> condition,
    EditorCoroutineCancelToken token = null
)

// 等待条件为真（带超时）
public static IEnumerator WaitUntil(
    Func<bool> condition,
    float timeoutSeconds,
    EditorCoroutineCancelToken token = null
)

// 延迟执行操作
public static IEnumerator Delay(
    Action action,
    float seconds,
    EditorCoroutineCancelToken token = null
)
```

---

## 💡 使用示例

### 示例 1：资源导入处理（避免编辑器卡顿）

```csharp
[MenuItem("Tools/Process All Textures")]
public static void ProcessAllTextures()
{
    EditorCoroutine.StartCoroutine(
        ProcessTexturesAsync(),
        onComplete: () => EditorUtility.DisplayDialog("完成", "所有纹理处理完毕"),
        onException: (ex) => EditorUtility.DisplayDialog("错误", ex.Message)
    );
}

private static IEnumerator ProcessTexturesAsync()
{
    var guids = AssetDatabase.FindAssets("t:Texture2D");
    
    foreach (var guid in guids)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        
        // 处理纹理
        ProcessTexture(texture);
        
        yield return EditorCoroutineExtensions.WaitFrame();
    }
}

private static void ProcessTexture(Texture2D texture)
{
    // 你的处理逻辑
}
```

### 示例 2：编辑器窗口的后台任务

```csharp
public class MyEditorWindow : EditorWindow
{
    private EditorCoroutine analysisCoroutine;

    private void OnGUI()
    {
        if (GUILayout.Button("开始分析", GUILayout.Height(30)))
        {
            analysisCoroutine = EditorCoroutine.StartCoroutine(
                AnalyzeProjectAsync(),
                onComplete: () => Debug.Log("分析完成")
            );
        }
    }

    private IEnumerator AnalyzeProjectAsync()
    {
        var assets = AssetDatabase.FindAssets("");
        var count = 0;

        foreach (var guid in assets)
        {
            count++;
            
            if (count % 100 == 0)
            {
                EditorUtility.DisplayProgressBar("分析中", $"已处理 {count} 个资源", 
                    (float)count / assets.Length);
                yield return EditorCoroutineExtensions.WaitFrame();
            }
        }

        EditorUtility.ClearProgressBar();
    }

    private void OnDestroy()
    {
        analysisCoroutine?.Dispose();
    }
}
```

### 示例 3：条件等待

```csharp
[MenuItem("Tools/Wait For Selection")]
public static void WaitForUserSelection()
{
    EditorCoroutine.StartCoroutine(
        WaitAndProcessSelection(),
        onComplete: () => Debug.Log("选择已处理")
    );
}

private static IEnumerator WaitAndProcessSelection()
{
    Debug.Log("等待用户选择...");
    
    // 等待用户选择某个 GameObject
    yield return EditorCoroutineExtensions.WaitUntil(
        () => Selection.activeGameObject != null,
        timeoutSeconds: 30f  // 30秒超时
    );

    if (Selection.activeGameObject != null)
    {
        Debug.Log($"用户选择了：{Selection.activeGameObject.name}");
    }
    else
    {
        Debug.Log("选择超时");
    }
}
```

### 示例 4：批量操作与取消

```csharp
public class BatchOperationWindow : EditorWindow
{
    private EditorCoroutine batchCoroutine;
    private EditorCoroutineCancelToken cancelToken;

    private void OnGUI()
    {
        if (GUILayout.Button("开始批量重命名"))
        {
            cancelToken = new EditorCoroutineCancelToken();
            batchCoroutine = EditorCoroutine.StartCoroutine(
                BatchRenameAsync(cancelToken)
            );
        }

        if (GUILayout.Button("取消操作"))
        {
            cancelToken?.Cancel();
        }
    }

    private IEnumerator BatchRenameAsync(EditorCoroutineCancelToken token)
    {
        var materials = Resources.FindObjectsOfTypeAll<Material>();
        
        for (int i = 0; i < materials.Length; i++)
        {
            if (token.IsCancelled)
            {
                Debug.Log("批量操作已取消");
                yield break;
            }

            materials[i].name = $"Material_{i:D4}";

            if (i % 10 == 0)
            {
                EditorUtility.DisplayProgressBar("重命名中", 
                    $"{i}/{materials.Length}", 
                    (float)i / materials.Length);
                    
                yield return EditorCoroutineExtensions.WaitFrame();
            }
        }

        EditorUtility.ClearProgressBar();
        Debug.Log("批量操作完成");
    }
}
```

### 示例 5：获取返回值

```csharp
[MenuItem("Tools/Calculate Asset Statistics")]
public static void CalculateAssetStats()
{
    EditorCoroutine<int>.StartCoroutine(
        CountAssetsAsync(),
        onComplete: (count) => 
        {
            EditorUtility.DisplayDialog("统计结果", $"项目包含 {count} 个资源");
        }
    );
}

private static IEnumerator CountAssetsAsync()
{
    yield return EditorCoroutineExtensions.WaitSeconds(0.5f);
    
    var guids = AssetDatabase.FindAssets("");
    yield return (Func<int>)(() => guids.Length);
}
```

---

## 🎯 最佳实践

### 1. 记住协程引用以便管理生命周期

```csharp
private EditorCoroutine myCoroutine;

private void OnGUI()
{
    if (GUILayout.Button("启动"))
    {
        myCoroutine = EditorCoroutine.StartCoroutine(SlowTask());
    }
}

private void OnDestroy()
{
    myCoroutine?.Dispose();  // 清理资源
}

private IEnumerator SlowTask()
{
    // ...
}
```

### 2. 在长时间操作中定期 yield

```csharp
// ✅ 好的做法：定期让出CPU时间
private IEnumerator ProcessManyItems(Item[] items)
{
    foreach (var item in items)
    {
        ProcessItem(item);
        yield return EditorCoroutineExtensions.WaitFrame();  // 每项一帧
    }
}

// ❌ 不好的做法：一次处理所有，编辑器会卡
private IEnumerator ProcessManyItemsBad(Item[] items)
{
    foreach (var item in items)
    {
        ProcessItem(item);  // 没有yield，会导致卡顿
    }
    yield return null;
}
```

### 3. 始终处理异常

```csharp
EditorCoroutine.StartCoroutine(
    RiskyOperation(),
    onException: (ex) => 
    {
        Debug.LogError($"操作失败: {ex.Message}");
        // 清理资源、显示用户提示等
    }
);
```

### 4. 使用取消令牌支持用户中断

```csharp
private EditorCoroutineCancelToken cancelToken;

private void OnGUI()
{
    if (GUILayout.Button("取消"))
    {
        cancelToken?.Cancel();
    }
}

private void StartLongTask()
{
    cancelToken = new EditorCoroutineCancelToken();
    EditorCoroutine.StartCoroutine(LongTask(cancelToken));
}

private IEnumerator LongTask(EditorCoroutineCancelToken token)
{
    for (int i = 0; i < 1000; i++)
    {
        if (token.IsCancelled)
            yield break;
        
        yield return EditorCoroutineExtensions.WaitFrame();
    }
}
```

### 5. 嵌套协程自动处理

```csharp
// ✅ 嵌套协程会自动等待完成
private IEnumerator Main()
{
    yield return Helper1();
    yield return Helper2();
}

private IEnumerator Helper1()
{
    yield return EditorCoroutineExtensions.WaitSeconds(1f);
}

private IEnumerator Helper2()
{
    yield return EditorCoroutineExtensions.WaitSeconds(1f);
}
```

---

## 📝 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📞 支持

如有问题，请：
1. 查看文档和示例
2. 查阅测试代码
3. 提交 Issue 描述问题

---

**Happy Coding! 🚀**
