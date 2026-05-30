> 内容由 AI 根据核心代码生成，已通过人工审核。

# CoroutineRunner

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity 2020.3+](https://img.shields.io/badge/Unity-2020.3%2B-black.svg)](https://unity.com)
[![Test Framework 1.1.33](https://img.shields.io/badge/Test%20Framework-1.1.33-green.svg)](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1)
[![Performance 3.0.3](https://img.shields.io/badge/Performance-3.0.3-orange.svg)](https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.0)

**CoroutineRunner** 是一个 Unity 增强型协程管理工具库，让你拥有完整的协程控制权（暂停/恢复/取消/状态查询），同时提供对象池化的等待指令、并发通道排队以及与 async/await 的无缝集成。

- 🎮 **完全可控**：通过句柄随时暂停、恢复、取消任何协程。
- ♻️ **零 GC 分配**：内置池化工厂，热路径无内存分配。
- 🚦 **并发限制**：通道机制限制同时运行的协程数量。
- ⏳ **现代异步**：支持 `await token` 等待协程结束。
- 📦 **无 MonoBehaviour 依赖**：全局静态入口，随处可用。

## 安装环境要求

- Unity 2020.3 或更高版本
- .NET Standard 2.0 或 .NET 4.x
- 可选依赖：`com.unity.test-framework`（用于测试）

## 安装方式

### 源码导入
1. 将 `Assets/Source` 目录下的所有 `.cs` 文件复制到你的 Unity 项目任意文件夹。
2. 确保项目启用 `Unsafe Code`（不需要，但某些数值转换可能需要）。

### DLL 导入
1. 编译 `CoroutineRunner.dll`（目标 .NET Standard 2.0）。
2. 将 DLL 放入 Unity 项目的 `Assets/Plugins` 文件夹。

## 设计理念

- **分离关注点**：`CoroutineHandleToken` 只读句柄 + 内部 `CoroutineHandle` 对象池，安全且高效。
- **池化优先**：所有自定义等待指令均通过池化工厂获取，用毕自动回收。
- **通道调度**：支持无限队列、有限并发，适合资源受限的批量任务。
- **状态可观测**：提供 `TryGetState` 和 `GetState`，不丢失取消/完成语义。
- **延迟回收**：协程结束后等待一帧再回收句柄，确保同一帧内可查询最终状态。

## 具体功能说明

### 1. 启动可控协程
```csharp
var token = GlobalCoroutineRunner.Run(MyCoroutine());
```

### 2. 控制协程生命周期
```csharp
token.Pause();      // 暂停
token.Resume();     // 恢复
token.Cancel();     // 取消
var state = token.GetState(); // Running / Paused / Completed / Canceled
```

### 3. 使用池化等待指令
```csharp
yield return CustomYield.Yield<WaitForSecondsControlled, float>(1.5f);   // 零分配
yield return CustomYield.Yield<WaitForFramesControlled, int>(3);         // 等待3帧
```

### 4. 通道排队执行
```csharp
GlobalCoroutineRunner.ConfigureChannel("download", 3);
var token = GlobalCoroutineRunner.RunQueued(DownloadRoutine(), "download");
```

### 5. async/await 集成
```csharp
await GlobalCoroutineRunner.Run(MyCoroutine());
```

### 6. 扩展方法
- `Pause()`, `Resume()`, `Cancel()`
- `GetState()`, `TryGetState()`, `IsDone()`
- `GetAwaiter()` 使 `await token` 可用

## 常见问题

**Q: 为什么需要 `Yield<T1, T2>(T2 arg)` 双泛型版本？**  
A: 单泛型版本 `Yield<T>(object arg)` 会导致值类型参数装箱（产生 GC）。双泛型版本可零分配。

**Q: 协程结束后还能获取状态吗？**  
A: 协程结束后会延迟一帧回收句柄，同一帧内 `GetState()` 仍返回正确状态；下一帧后句柄失效，`TryGetState` 返回 false。

**Q: 如何扩展自己的等待指令？**  
A: 继承 `CustomYieldInstructionBase`，实现 `KeepWaiting()`，并根据需要实现 `IPoolable` 或 `IPoolableYieldInstruction<T>` 接口。然后通过 `CustomYield.Yield<YourInstruction, T>(...)` 使用。

**Q: 通道并发数设为 0 是什么意思？**  
A: 并发数 ≤ 0 表示不限制并发，所有协程立即执行。

## 其他文档

- [API 文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。