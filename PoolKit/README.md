> 内容由 AI 根据核心代码生成，已通过人工审核。

# PoolKit

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2020.3+-black.svg?logo=unity)](https://unity.com/)
[![Test Framework](https://img.shields.io/badge/Test%20Framework-NUnit%203.0.1%20%7C%20Unity.PerformanceTesting%203.0.3-green.svg)](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

**PoolKit** 是一个轻量级、高性能的 C# 对象池工具库，专为 .NET 和 Unity 环境设计。它提供了丰富的集合类型池化（List、Dictionary、Queue、Stack、HashSet、Array）以及 Unity 对象池化（GameObject、Component）支持，旨在显著减少内存分配和 GC 压力，提升应用程序性能。

---

## 工具库简介

在游戏开发和服务器应用中，频繁创建和销毁对象会导致：

- **内存碎片**：大量短生命周期对象增加 GC 负担
- **性能下降**：`new` 操作和 Unity 的 `Instantiate`/`AddComponent` 开销巨大
- **卡顿**：GC 触发时可能导致帧率抖动

**PoolKit 通过对象复用机制解决以上问题：**

- ✅ **集合池**：`List<T>`、`Dictionary<TKey,TValue>`、`Queue<T>`、`Stack<T>`、`HashSet<T>`、`T[]`
- ✅ **Unity 对象池**：`GameObject`、`Component` 及任意 `UnityEngine.Object` 子类
- ✅ **通用对象池**：任意 `class` 类型和集合类型
- ✅ **零 GC 分配**：池化操作几乎不产生托管内存分配
- ✅ **线程安全**：使用 `ConcurrentStack` 和 `ConcurrentDictionary` 实现
- ✅ **作用域管理**：支持 `using` 语句自动归还

根据基准测试，PoolKit 在典型场景下可实现 **3~2700 倍** 的性能提升。

---

## 环境要求

| 组件 | 版本要求 |
|-----|---------|
| **.NET** | 4.x / .NET Standard 2.0+ |
| **Unity** | 2020.3 或更高版本（使用 Unity 专用模块时） |
| **C#** | 7.3+ |

---

## 安装方式

### 方式一：源码导入

1. 将 `PoolKit` 源码目录复制到 Unity 项目的 `Assets` 文件夹或任意 `Scripts` 目录
2. 确保脚本编译通过（无需额外依赖）

### 方式二：DLL 文件导入

1. 将 DLL 放入 Unity 项目的 `Assets/Plugins` 目录
2. 在需要使用的脚本中添加 `using PoolKit;` 和 `using PoolKit.Collections;` 等命名空间引用

---

## 设计理念

PoolKit 的设计遵循以下核心原则：

1. **简单易用**：API 设计直观，支持 `using` 作用域模式，减少手动管理错误
2. **高性能优先**：使用 `ConcurrentStack` 作为底层存储，无锁操作保证高并发性能
3. **灵活可扩展**：通过 `OverrideCreate`、`OverrideReset`、`OverrideDestroy` 支持自定义行为
4. **零侵入**：无需修改现有类型即可使用池化功能
5. **内存可控**：支持固定容量限制，防止内存无限增长

---

## 具体功能说明

### 1. 集合池 (Collection Pools)

为常用 .NET 集合类型提供池化支持：

| 池类型 | 对应集合 | 适用场景 |
|-------|---------|---------|
| `ListPool` | `List<T>` | 临时数据存储、数据转换 |
| `DictionaryPool` | `Dictionary<TKey,TValue>` | 键值对缓存、查找表 |
| `QueuePool` | `Queue<T>` | 任务队列、消息缓冲 |
| `StackPool` | `Stack<T>` | 深度优先遍历、撤销操作 |
| `HashSetPool` | `HashSet<T>` | 去重操作、集合运算 |
| `ArrayPool` | `T[]` | 大数组复用、缓冲区管理 |

**特点：**
- 静态 API，无需手动创建池实例
- 支持 `using` 作用域自动归还
- 每个类型自动维护独立的池

### 2. 通用对象池 (ClassPool)

适用于任意 `class` 类型的对象复用：

- 支持自定义创建、重置、销毁逻辑
- 可控制池容量和是否固定
- 适合 MonoBehaviour 之外的纯 C# 对象

### 3. Unity 对象池 (UnityObjectPool)

专为 Unity 游戏对象设计的池化方案：

- **GameObjectPool**：管理 `GameObject` 实例，自动处理 `SetActive`
- **ComponentPool**：管理 `Component` 实例，自动启用/禁用
- 支持 `DontDestroyOnLoad` 持久化
- 可指定容器对象和原型预制体
- 支持获取时自动激活

### 4. 集合池泛型封装 (CollectionPool)

将任意 `IEnumerable<T>` 集合类型池化：

```csharp
var pool = new CollectionPool<int, List<int>>();
var list = pool.Get();
// 使用 list...
pool.Release(list);
```

---

## 许可证

本项目采用 [MIT](../LICENSE) 许可证。

---

## 相关文档

- 📖 [API 参考文档](./source/1.0.0-beta/DOCUMENT.md)
- 📊 [测试报告](./tests/1.0.0-beta/TEST_REPORT.md)