> 内容由 AI 根据核心代码生成，已通过人工审核。

# EasyMapper

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2020.3+-black.svg)](https://unity.com/)
[![.NET](https://img.shields.io/badge/.NET-Standard_2.0-512BD4.svg)](https://dotnet.microsoft.com/en-us/platform/dotnet-standard)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)

**EasyMapper** 是一个高性能、可组合的 **运行时 ID 映射框架**，专为 Unity 和 .NET Standard 2.0 设计。  
它将任意字符串或 UnityEngine.Object 转换为紧凑的 64 位 `LongToken`，并提供了丰富的装饰器流水线，在保持极致性能的同时轻松应对百万级对象。

---

## 功能特性

- 🚀 **超低延迟**：短字符串编码路径单次操作在 100~400 ns 之间，字典查找 <100 ns。
- 🧹 **零分配快速路径**：符合条件的短标识符直接通过位运算生成 Token，不产生任何堆分配。
- 🧩 **可组合架构**：所有流水线、蓝图、序列化器都是可替换的泛型接口实现，支持装饰器叠加。
- 🧠 **智能分发**：`SmartDistributor` 根据字符串特征自动选择最优编码方案。
- 🔄 **弱引用对象映射**：`UnityWeakPipeline` 利用 `ConditionalWeakTable` 和 `WeakReference` 跟踪 Unity 对象，杜绝内存泄漏。
- 🔒 **线程安全支持**：`ThreadSafePipeline` 装饰器一键加锁。
- 💾 **紧凑序列化**：Token 可序列化为 8 字节（或 16 字节）二进制，适合网络同步与存档。
- 🛠️ **内置诊断与防护**：`DiagnosticPipeline`, `GuardedPipeline`, `IdempotentPipeline` 等开箱即用。

---

## 环境要求

- Unity 2020.3 或更高版本
- 目标 .NET Standard 2.0 或 .NET Framework 4.x
- 无其他外部依赖（仅需 Unity 自身 API）

---

## 安装方式

### 1. 导入源码
将 `EasyMapper` 文件夹复制到项目中，包含所有 `.cs` 文件即可。

### 2. 导入 DLL
将编译后的 `EasyMapper.dll` 放入 `Assets/Plugins` 目录。

> *推荐导入源码，以便自由修改和调试。*

---

## 设计理念

EasyMapper 遵循 **"没有完美的单一模块，只有可组合的完美系统"**。  
通过极简的泛型接口（`IBlueprint`, `IPipeline`, `IPackage`, `IFeature`）定义契约，所有功能都通过独立的零件组合而成。默认实现提供了经过验证的高效零件，用户可以根据需要自由替换或装饰，而不必修改框架核心。

---

## 主要功能简介

### 字符串编码
- **Char10PackingBlueprint**：将长度 ≤10、字符集为 [a-z0-9_-] 的字符串无损编码到 64 位长整形中，解码无需求助字典。
- **InterningBlueprint**：为任意长字符串分配唯一且幂等的 ID，使用原子自增计数器。
- **SmartDistributor**：根据输入字符串特征自动选择上述两种路径，并用 bit63 标记，还原时准确路由。

### Unity 对象映射
- **ObjectNamingBlueprint**：提取对象名称，交由字符串蓝图生成 Token。
- **UnityWeakPipeline**：维护“Token → 弱引用对象”映射，当对象被销毁时自动返回 null，并可调用 `Cleanup()` 清理死引用。

### 流水线装饰器
- `CappedPipeline`：LRU 容量限制，防止字典无限增长。
- `CacheFirstPipeline`：基于 `ConditionalWeakTable` 缓存已导入的对象，避免重复调用蓝图。
- `ThreadSafePipeline`：透明加锁，使任意流水线线程安全。
- `DiagnosticPipeline`：记录 Import/Export 次数并触发事件，便于监控。
- `IdempotentPipeline`：强制幂等，相同源多次导入返回同一 Token。
- `GuardedPipeline`：参数校验，拒绝 null 或默认值。

### 序列化
- `BinaryIdentityPackage`：Token ↔ 8 字节数组，适合服务器通信。
- `GuidBinaryPackage`：支持 128 位 GUID Token 的二进制转换。

### 全局入口与服务替换
- 静态类 `IDMap` 提供最简 API（`Assign`, `Locate`, `Pack`, `Unpack`, `Cleanup`）。
- 静态属性 `IDMap.Current` 可由 `IDMapInstance.Builder` 构建的自定义实例替换，实现全项目范围的策略切换。

---

## 快速上手

```csharp
// 分配字符串 Token
long token = IDMap.Assign("PlayerHealth");
string result = IDMap.Locate(token); // "PlayerHealth"

// 分配对象 Token（GameObject）
GameObject npc = ...;
long npcId = IDMap.Assign(npc);
GameObject restored = IDMap.Locate<GameObject>(npcId);

// 网络传输
byte[] data = IDMap.Pack(token);
long deserialized = IDMap.Unpack(data);
```

---

## 文档导航

- 📘 [API 文档](./DOCUMENT.md) – 所有公开类型及方法的详细说明。
- 🧪 [测试报告](./TEST_REPORT.md) – 基准测试与单元测试结果。

---

## 许可证

本项目采用 [MIT](../LICENSE) 许可证。