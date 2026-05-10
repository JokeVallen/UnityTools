> 内容由 AI 根据核心代码生成，已通过人工审核。

# EditorCoroutines.Lit

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity](https://img.shields.io/badge/Unity-2019.4+-black?logo=unity)](https://unity.com/)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)

轻量级 Unity 编辑器协程库，提供与运行时代程类似的异步体验，支持嵌套协程、取消令牌、超时等待与返回值。

## 📖 简介

`EditorCoroutines.Lit` 在 Unity Editor 环境下基于 `EditorApplication.update` 驱动协程，让你可以在编辑器脚本、检查器或工具窗口中使用熟悉的 `yield return` 模式编写异步逻辑。无需依赖 `MonoBehaviour`，所有操作均在编辑器中完成，完美适配自定义编辑器工具、资源导入处理、自动化任务等场景。

## 🛠 安装环境要求

- Unity 2019.4 或更高版本
- 仅支持 **Unity Editor** 平台（脚本使用 `#if UNITY_EDITOR` 包裹）
- 无需额外依赖

## 📥 安装方式

### 方式一：通过源码导入
1. 将仓库中所有 `.cs` 文件复制到你的 Unity 项目的任意 `Editor` 文件夹下（例如 `Assets/Editor/EditorCoroutines/`）。
2. 确认所有文件位于命名空间 `EditorCoroutines.Lit` 内，脚本自动生效。

### 方式二：通过 DLL 导入
1. 在 Release 页面下载预编译的 `EditorCoroutines.Lit.dll`。
2. 将 DLL 放入 `Assets/Editor` 文件夹中。
3. 确保 Unity 编辑器已加载该程序集。

## 🎯 设计理念

- **零依赖**：纯 C# 实现，仅依靠 Unity Editor API。
- **最小可用**：模仿 `MonoBehaviour` 协程的使用习惯，降低学习成本。
- **职责分离**：
  - `EditorCoroutine` / `EditorCoroutine<T>` 负责协程生命周期（启动、停止、释放、异常处理）。
  - `EditorCoroutineCancelToken` 提供轻量级取消信号，由用户或扩展方法检查。
  - `EditorCoroutineExtensions` 提供常用的等待原语（秒、帧、条件、延迟等）。
- **嵌套支持**：自动展平嵌套 `IEnumerator`，让组合异步逻辑更自然。
- **安全释放**：支持 `IDisposable` 模式，可安全地停止协程并清空回调，防止内存泄漏。

## ⚙️ 具体功能

### 1. 编辑器协程生命周期管理
- 通过 `StartCoroutine` 启动协程，提供完成与异常回调。
- `Stop()` 可随时终止协程，取消 `EditorApplication.update` 注册。
- `Dispose()` 彻底释放资源，可多次调用而不会报错。

### 2. 带返回值的协程
- `EditorCoroutine<T>` 允许协程在结束时产出一个结果（通过 `yield return` 值或 `Func<T>`）。
- 适用于需要异步计算并返回数据的编辑器操作，如文件处理进度、网络请求结果等。

### 3. 取消令牌与超时控制
- `EditorCoroutineCancelToken` 为外部提供一个布尔开关，配合扩展方法可提前结束当前等待。
- 超时版本的 `WaitUntil` 确保协程不会无限期挂起。

### 4. 丰富的时间/条件等待
- **`WaitSeconds` / `WaitMilliseconds`**：基于 `EditorApplication.timeSinceStartup` 的精确等待。
- **`WaitFrame`**：等待下一编辑器帧。
- **`WaitUntil`**：条件等待，支持超时。
- **`Delay`**：延迟后执行 Action，自动处理取消逻辑。

### 5. 嵌套协程自动展平
- 你可以在协程中 `yield return` 另一个 `IEnumerator`，协程引擎会自动等待嵌套协程执行完毕，无需额外处理。

### 6. 异常安全
- 所有异常通过 `onException` 回调捕获，不会中断编辑器主循环，并正常结束协程。

## ❓ 常见问题

**Q：取消令牌为什么不能直接停止整个协程？**  
A：令牌只负责通知等待方法提前结束。要彻底终止，请直接调用协程对象的 `Stop()` 或 `Dispose()`。或者在每次等待后检查 `token.IsCancelled` 并手动 `yield break`。

**Q：可以在协程运行时访问 `Result` 吗？**  
A：可以，但只有在协程完全结束后才能获得最终结果。建议在 `onComplete` 回调中使用 `Result`。

**Q：是否支持在播放模式下使用？**  
A：所有代码均被 `#if UNITY_EDITOR` 限制，仅用于编辑器。播放模式下请使用 `MonoBehaviour.StartCoroutine`。

## 📚 其它文档

- [API 详细文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)
- [更新日志](./RELEASE.md)

## 📄 许可证

本项目采用 [MIT 许可证](https://opensource.org/licenses/MIT)。你可以自由使用、修改和分发。