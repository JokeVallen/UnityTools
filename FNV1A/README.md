> 文档内容由 AI 基于代码生成，已通过人工审阅。

# FNV1AUtility

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2020.3+-blue)](https://unity.com/)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Unity Test Framework](https://img.shields.io/badge/Unity%20Test%20Framework-passing-brightgreen)]()

FNV1AUtility 是一个专为 Unity 和 .NET 设计的高性能、零分配、跨平台确定性哈希工具库。它基于 **FNV-1a 64 位算法**，支持多种基础类型、Unity 类型及集合类型的哈希组合，并提供灵活的扩展机制。通过泛型静态缓存与内联优化，该库在保持易用性的同时，将抽象开销降至最低。

---

## 目录

1. [核心特性](#核心特性)
2. [快速上手](#快速上手)
3. [内置类型清单](#内置类型清单)
   - [.NET 基础类型](#net-基础类型)
   - [Unity 特有类型](#unity-特有类型)
   - [集合类型](#集合类型)
4. [自定义哈希](#自定义哈希)
   - [实现 IFNVHashable 接口](#实现-ifnvhashable-接口)
   - [注册自定义哈希器](#注册自定义哈希器)
5. [高级用法](#高级用法)
   - [启用 Unsafe 优化](#启用-unsafe-优化)
   - [Unsafe API 使用指南](#unsafe-api-使用指南)
   - [集合处理最佳实践](#集合处理最佳实践)
   - [泛型 API 与非泛型 API](#泛型-api-与非泛型-api)
   - [缓存预热](#缓存预热)
6. [实际应用示例](#实际应用示例)
7. [注意事项与常见问题](#注意事项与常见问题)

---

## 核心特性

| 特性 | 说明 |
|------|------|
| **确定性** | 相同输入始终产生相同哈希，不受进程、平台、.NET 版本影响 |
| **零堆分配** | 除一次性缓存初始化和安全版 Guid 外，所有操作均无 GC 分配 |
| **高性能** | 单次 int 哈希约 4 ns，集合封装无额外开销 |
| **广泛类型支持** | 内置支持 .NET 基础类型、Unity 数学类型、数组、列表等 |
| **可扩展** | 可通过接口或注册委托自定义任意类型的哈希行为 |
| **条件编译优化** | 支持 `ENABLE_UNSAFE` 宏，为 Guid 等类型提供指针级加速 |
| **线程安全** | 静态缓存只读，并发调用安全（注册新哈希器需外部同步） |

---

## 快速上手

### 1. 获取初始哈希值

所有哈希计算都从一个初始值开始：

```csharp
ulong hash = FNV1AUtility.Start();
```

### 2. 逐项追加数据

根据值的类型，调用对应的 `Append` 方法。每次调用需要显式传入 `hash` 并接收新的哈希值。

```csharp
// .NET 基础类型使用 AppendForNET<T> 或具体方法
hash = FNV1AUtility.AppendForNET(hash, 42);                 // int
hash = FNV1AUtility.AppendForNET(hash, "Hello");            // string

// Unity 类型必须使用 AppendForUnity<T> 或具体方法
hash = FNV1AUtility.AppendForUnity(hash, new Vector3(1,2,3)); // Unity Vector3
```

### 3. 获取最终哈希

每次调用返回的 `ulong` 即为更新后的累积哈希，最终值即为完整的 64 位 FNV-1a 哈希。

---

## 内置类型清单

库通过静态缓存为大量类型预先配置了高效哈希器。以下列出所有内置支持的类型，开发者可直接使用，无需任何额外代码。

### .NET 基础类型

**使用方式**：`FNV1AUtility.AppendForNET<T>(hash, value)` 或专用方法如 `AppendInt32`。

| 类型 | 专用方法示例 | 备注 |
|------|-------------|------|
| `byte` | `AppendByte(hash, value)` | 极速 |
| `int` | `AppendInt32(hash, value)` | 极速 |
| `long` | `AppendInt64(hash, value)` | 极速 |
| `float` | `AppendFloat(hash, value)` | 注意 ±0.0f 产生不同哈希 |
| `double` | `AppendDouble(hash, value)` | - |
| `bool` | `AppendBool(hash, value)` | - |
| `string` | `AppendString(hash, value)` | 无分配，`null` 视为 0 |
| `enum` | `AppendEnum(hash, value)` | 转为 `int` 追加 |
| `DateTime` | `AppendDateTime(hash, value)` | 使用 `Ticks` |
| `Guid` | `AppendGuid(hash, value)` | 安全版本，有分配；高性能版本见 Unsafe 模块 |
| `IFNVHashable` | 自动调用 `AppendHash` | 推荐的自定义类型方式 |

> **回退行为**：对于未内置的类型，若未实现 `IFNVHashable` 也未注册自定义委托，库会调用 `value.GetHashCode()` 并将其结果作为 `int` 追加。**此方式不具备跨平台确定性**，请谨慎用于持久化或网络同步场景。

### Unity 特有类型

**使用方式**：**必须**使用 `FNV1AUtility.AppendForUnity<T>(hash, value)` 或对应的专用方法，**不能**使用 `AppendForNET`。

| 类型 | 专用方法示例 | 备注 |
|------|-------------|------|
| `UnityEngine.Object` | `AppendUnityObject(hash, obj)` | 使用 `GetInstanceID()` |
| `Vector2` | `AppendVector2(hash, value)` | - |
| `Vector3` | `AppendVector3(hash, value)` | - |
| `Vector4` | `AppendVector4(hash, value)` | - |
| `Quaternion` | `AppendQuaternion(hash, value)` | - |
| `Color` | `AppendColor(hash, value)` | - |
| `Rect` | `AppendRect(hash, value)` | - |

```csharp
ulong hash = FNV1AUtility.Start();
hash = FNV1AUtility.AppendForUnity(hash, new Vector3(1, 2, 3));   // 泛型
hash = FNV1AUtility.AppendVector3(hash, new Vector3(4, 5, 6));    // 专用方法
```

### 集合类型

**使用方式**：通过 `AppendForCollection<TCollection, TElement>` 或专用方法（如 `AppendArray`），需要传入**元素哈希器**。

| 类型 | 专用方法示例 |
|------|-------------|
| `T[]` | `AppendArray(hash, array, elementHasher)` |
| `List<T>` | `AppendList(hash, list, elementHasher)` |
| `IList<T>` | `AppendIListGeneric(hash, list, elementHasher)` |
| `IList`（非泛型） | `AppendIList(hash, list, elementHasher)` |

```csharp
int[] numbers = { 1, 2, 3 };
hash = FNV1AUtility.AppendArray(hash, numbers, FNV1AUtility.AppendInt32);
```

---

## 自定义哈希

### 实现 IFNVHashable 接口

```csharp
public class PlayerData : IFNVHashable
{
    public int Id;
    public string Name;
    public Vector3 Position;

    public ulong AppendHash(ulong hash)
    {
        hash = FNV1AUtility.AppendInt32(hash, Id);
        hash = FNV1AUtility.AppendString(hash, Name);
        hash = FNV1AUtility.AppendForUnity(hash, Position); // Unity 类型用 AppendForUnity
        return hash;
    }
}
```

使用：

```csharp
PlayerData data = ...;
ulong hash = FNV1AUtility.Start();
hash = FNV1AUtility.AppendForNET(hash, data); // 自动调用 AppendHash
```

### 注册自定义哈希器

```csharp
// 在游戏启动时注册
FNV1AUtility.RegisterHasherForNET<DateTime>((hash, dt) =>
{
    return FNV1AUtility.AppendInt32(hash, dt.Year * 10000 + dt.Month * 100 + dt.Day);
});

FNV1AUtility.RegisterHasherForUnity<Vector3>((hash, v) =>
{
    // 自定义逻辑
    return hash;
});
```

---

## 高级用法

### 启用 Unsafe 优化

1. Unity 中勾选 **Player Settings → Allow 'unsafe' Code**。
2. 在 **Scripting Define Symbols** 中添加 `ENABLE_UNSAFE`。
3. 重新编译。

完成上述配置后，`FNV1AUtility.Unsafe.cs` 文件中的高性能 API 将变为可用。

### Unsafe API 使用指南

当启用了 `ENABLE_UNSAFE` 后，库提供独立的 unsafe 入口 `AppendForUnsafe<T>` 和一系列以 `Unsafe` 后缀结尾的方法，专为性能极致场景设计。

**核心 API：**

| API | 说明 |
|-----|------|
| `AppendForUnsafe<T>(hash, value)` | 泛型入口，通过 `UnsafeFNVHasherCache<T>` 调用对应的 unsafe 优化委托 |
| `AppendForUnsafe(hash, object)` | 非泛型入口，目前特化处理 `Guid` 和 `IFNVHashable` |
| `AppendGuidUnsafe(hash, guid)` | 使用指针逐字节追加，零分配，约 20 ns |
| `AppendGuidFastUnsafe(hash, guid)` | 将 Guid 视为两个 `ulong`，仅 2 次迭代，约 2.5 ns |

**Guid 哈希的两种 Unsafe 实现对比：**

| 方法 | 性能 | 迭代方式 | 适用场景 |
|------|------|----------|----------|
| `AppendGuidUnsafe` | ~20 ns | 逐字节循环 16 次 | 需要与安全版本行为完全一致的调试/验证场景 |
| `AppendGuidFastUnsafe` | ~2.5 ns | 两次 ulong 迭代 | **推荐**，追求极致性能的生产环境 |

**使用示例：**

```csharp
Guid guid = Guid.NewGuid();
ulong hash = FNV1AUtility.Start();

// 推荐：通过泛型入口自动选择最快实现
hash = FNV1AUtility.AppendForUnsafe(hash, guid);

// 或直接调用极速版本
hash = FNV1AUtility.AppendGuidFastUnsafe(hash, guid);

// 若需要与安全版本逻辑严格一致
hash = FNV1AUtility.AppendGuidUnsafe(hash, guid);
```

> **注意**：`AppendForNET<Guid>` 在启用 `ENABLE_UNSAFE` 后**仍然使用安全版本**，不会自动切换到 unsafe 实现。如需加速，请显式调用 `AppendForUnsafe` 系列方法。这样设计是为了让开发者清晰地把控性能与安全性边界。AppendGuidFastUnsafe 产生的哈希值与 AppendGuidUnsafe 及安全版本 AppendGuid 不同。虽然它们都是确定性哈希，但因迭代方式不同，结果不可互换。请在整个项目中统一使用同一种 Guid 哈希方法，否则哈希校验将失败。

### 集合处理最佳实践

```csharp
// 推荐：使用泛型集合 API
hash = FNV1AUtility.AppendForCollection<int[], int>(hash, numbers, FNV1AUtility.AppendInt32);
```

### 泛型 API 与非泛型 API

| API | 特点 |
|-----|------|
| `AppendForNET<T>(hash, value)` | 零装箱，首选（安全版本） |
| `AppendForUnity<T>(hash, value)` | 零装箱，专用于 Unity 类型 |
| `AppendForUnsafe<T>(hash, value)` | 零装箱，启用 unsafe 后的高性能路径 |
| `AppendForNET(hash, object)` | 内部 `switch`，值类型会装箱，尽量避免 |

### 缓存预热

在 `Awake()` 中预热常用类型，避免首次调用时的微小延迟：

```csharp
void Awake()
{
    FNV1AUtility.AppendForNET(0, 0);
    FNV1AUtility.AppendForUnity(0, Vector3.zero);
    FNV1AUtility.AppendForCollection<int[], int>(0, null, null);
#if ENABLE_UNSAFE
    FNV1AUtility.AppendForUnsafe(0, Guid.Empty); // 预热 unsafe 缓存
#endif
}
```

---

## 实际应用示例

### 1. 存档完整性校验

```csharp
SaveData data = LoadSaveData();
ulong hash = FNV1AUtility.Start();
hash = data.AppendHash(hash); // data 实现 IFNVHashable
PlayerPrefs.SetString("SaveHash", hash.ToString());
```

### 2. 缓存键生成

```csharp
ulong key = FNV1AUtility.Start();
key = FNV1AUtility.AppendForNET(key, path);
key = FNV1AUtility.AppendForNET(key, width);
key = FNV1AUtility.AppendForUnity(key, tint); // Color 是 Unity 类型
```

### 3. 网络状态同步

```csharp
ulong serverHash = FNV1AUtility.Start();
serverHash = FNV1AUtility.AppendForUnity(serverHash, serverState.position);
serverHash = FNV1AUtility.AppendForUnity(serverHash, serverState.rotation);
serverHash = FNV1AUtility.AppendForNET(serverHash, serverState.health);
```

---

## 注意事项与常见问题

### ❗ Unity 类型必须使用 `AppendForUnity`

若对 Unity 类型误用 `AppendForNET`，将回退到 `GetHashCode()`，导致确定性丢失且可能碰撞。请务必使用 `AppendForUnity` 或相应的专用方法。

### ❗ 浮点数 ±0 问题

`+0.0f` 与 `-0.0f` 产生不同哈希。若需视作相等，请在外部规范化。

### ❗ 集合顺序敏感

数组、列表等有序集合保留元素顺序。如需无序哈希，请自行组合元素哈希值（如 XOR）。

### ❗ Unsafe API 需显式调用

即使启用了 `ENABLE_UNSAFE`，`AppendForNET<Guid>` 仍使用安全版本。请使用 `AppendForUnsafe<Guid>` 或直接调用 `AppendGuidFastUnsafe` 以获得加速。

---

## 总结

FNV1AUtility 提供了简洁、高效的确定性哈希方案。使用时要牢记：

- **.NET 类型用 `AppendForNET`，Unity 类型用 `AppendForUnity`。**
- **需要极致性能时，启用 `ENABLE_UNSAFE` 并调用 `AppendForUnsafe` 系列。**
- **所有调用需显式传递并接收 `hash`，不支持链式。**
- 自定义类型优先实现 `IFNVHashable`。

遵循以上要点，您可以在项目中轻松集成并获得最佳性能。

FNV1AUtility 库遵循以下原则：

- 单一职责：仅提供 FNV-1a 哈希计算的原子操作和类型缓存。
- 机制而非策略：提供 Append 机制，不规定调用风格（链式或传统）、不强制并发模型。
- 零开销抽象：不引入不必要的包装层，将性能优化空间完全留给开发者。

因此，开发者完全可以根据项目需要，以扩展方法或包装类的形式自由添加链式调用、线程安全累加器、Fluent Builder 等上层设施，而不会与库的核心设计冲突。