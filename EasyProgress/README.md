> 内容由 AI 根据核心代码生成，已通过人工审核。

# EasyProgress

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-brightgreen.svg)](https://dotnet.microsoft.com/download)
[![Unity](https://img.shields.io/badge/Unity-Compatible-brightgreen.svg)](https://unity.com)
[![Test Framework](https://img.shields.io/badge/test-xUnit-blue.svg)](https://xunit.net)
[![Benchmark](https://img.shields.io/badge/benchmark-BenchmarkDotNet-blue.svg)](https://benchmarkdotnet.org)

**EasyProgress** 是一个轻量、高性能、线程安全的进度管理工具库，专为 .NET Standard 2.0 设计，支持 Unity 及其他 .NET 运行时。它提供叶子节点、组合节点和可插拔的组合规则，可轻松实现单一任务、加权并行、顺序串行、动态子任务及树形嵌套进度，并内置对象池与扩展方法，显著降低 GC 压力。

## 安装环境要求

- .NET Standard 2.0 或更高版本（兼容 .NET Framework 4.6.1+、.NET Core 2.0+、Unity 2018.1+）
- 支持 C# 7.0 及以上

## 安装方式

### 源码导入

将源码文件复制到您的项目中即可。

### DLL 文件导入

下载 `EasyProgress.Core.dll`，添加引用。

## 设计理念

- **通用性**：框架层只定义最基本的概念（`IProgressNode`、`IProgressLeaf`），不包含任何假设；默认实现层提供开箱即用的功能。
- **高性能**：热路径零分配（在池化稳定后），微秒级延迟；内置 `ListPool`、`DictionaryPool` 和节点池化。
- **易用性**：提供 `using` 作用域、委托模式等扩展方法，自动管理节点生命周期。
- **可扩展性**：可通过实现接口自定义节点、规则和管理器。

## 具体功能说明

### 1. 单一进度条

使用 `DefaultLeaf` 直接报告进度。

### 2. 多任务加权总进度

使用 `WeightedRealtimeComposite` + `WeightedAverageRule`，每个子任务可以设置不同权重。

### 3. 串行任务链

使用 `WeightedRealtimeComposite` + `SequentialRule`，子任务按顺序执行，前一个完成才进入下一个。

### 4. 动态未知数量的子任务

使用 `RealtimeComposite`（无权重），动态调用 `AddChild` 添加子节点，所有子节点等权平均。

### 5. 嵌套进度

组合节点可以包含其他组合节点，形成任意深度的进度树。

### 6. 手动刷新模式（适合 Unity 等需要批量更新的场景）

使用 `ManualComposite`，子节点变化只标记脏，外部每帧调用 `Refresh()` 才会计算总进度并触发事件。

### 7. 对象池与节点复用

- 使用 `Progress.CreateLeaf<T>()` 和 `Progress.CreateComposite<T>()` 自动从默认池获取节点。
- 使用完毕后调用 `Progress.ReleaseLeaf` / `Progress.ReleaseComposite` 归还池中。
- 可使用扩展方法 `ReleaseTree` 递归释放整个子树。

### 8. 快捷扩展方法

- `BeginProgress` / `BeginComposite`：`using` 作用域自动清理临时节点。
- `RunWithProgress` / `RunWithProgressAsync`：委托模式，执行任务后自动清理。
- `AddChildren`：批量添加子节点。
- `ReleaseLeafChildren` / `ReleaseTree`：清理子节点或整个子树。

### 9. 自定义组合规则

实现 `ICompositionRule<T>` 接口，即可定义任意聚合逻辑（如取最大值、自定义加权等）。

## 常见问题和回答

**Q: 如何判断一个进度节点是否已完成？**  
A: 对于 `double` 类型，可以使用扩展方法 `.IsComplete()`（需自行添加或使用容差比较）。内部使用 `Math.Abs(progress - 1.0) < 1e-9`。

**Q: 组合节点没有子节点时进度是多少？**  
A: 进度为 0。

**Q: 多个线程同时报告进度安全吗？**  
A: 所有公共 API 都是线程安全的，内部使用细粒度锁，事件在锁外触发，避免死锁。

**Q: 如何避免对象池泄漏？**  
A: 使用 `using` 作用域或 `RunWithProgress` 等扩展方法，或手动调用 `ReleaseTree` 递归释放子节点。

**Q: 能否支持 int 类型进度（0-100）？**  
A: 框架层支持泛型 `T`，您可以自己实现 `int` 版本的节点和规则，并通过 `Progress.RegisterProgressManager` 注册管理器。

## 其它文档导航

- [API 文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)

## 许可证

[MIT](LICENSE)