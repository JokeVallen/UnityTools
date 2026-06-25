> 内容由 AI 根据核心代码生成，已通过人工审核。

---

# ComparerUtility API 文档

**版本**：1.0.1-beta

---

## 概述

`ComparerUtility` 是一个比较器策略工具类，提供统一的比较器注册、获取和缓存机制。其核心定位是：

- **策略注册表**：允许为任意类型 `T` 和业务键 `TKey` 注册自定义比较器实例。
- **运行时装配**：支持在运行时根据类型和业务键动态获取比较器。
- **零 GC 分配**：所有热路径操作不产生堆内存分配，适合 Unity 等对 GC 敏感的环境。

**服务对象**：无状态自定义比较器、不可变状态自定义比较器。

---

## 公共 API

### 一、相等性比较器（IEqualityComparer）

#### 泛型路径

---

##### `GetEqualityComparer<T, TKey>(TKey key)`

获取指定类型的相等性比较器。若未注册则返回 `null`。

- **类型参数**：
  - `T`：数据类型
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
- **返回**：`IEqualityComparer<T>`，若未注册则返回 `null`
- **异常**：`ArgumentNullException`（`key` 为 `null`）

```csharp
var comparer = ComparerUtility.GetEqualityComparer<string, int>(123);
if (comparer != null) { /* 使用 comparer */ }
```

---

##### `TryGetEqualityComparer<T, TKey>(TKey key, out IEqualityComparer<T> comparer)`

尝试获取指定类型的相等性比较器。

- **类型参数**：
  - `T`：数据类型
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
  - `comparer`：输出参数，获取成功时返回比较器
- **返回**：`bool`，成功返回 `true`，否则返回 `false`

```csharp
if (ComparerUtility.TryGetEqualityComparer<string, int>(123, out var comparer))
{
    // 使用 comparer
}
```

---

##### `GetEqualityComparerOrDefault<T, TKey>(TKey key)`

获取指定类型的相等性比较器。若 `key` 为 `null` 或未注册，则返回默认比较器 `EqualityComparer<T>.Default`。

- **类型参数**：
  - `T`：数据类型
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
- **返回**：`IEqualityComparer<T>`，始终非空

```csharp
var comparer = ComparerUtility.GetEqualityComparerOrDefault<string, int>(123);
// comparer 不为 null
```

---

##### `SetEqualityComparer<T, TKey>(TKey key, IEqualityComparer<T> comparer)`

注册指定类型的相等性比较器。若该比较器同时实现了非泛型 `IEqualityComparer` 接口，会自动同步到非泛型存储。

- **类型参数**：
  - `T`：数据类型
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
  - `comparer`：相等性比较器
- **异常**：`ArgumentNullException`（`key` 或 `comparer` 为 `null`）

```csharp
var comparer = StringComparer.OrdinalIgnoreCase;
ComparerUtility.SetEqualityComparer<string, string>("IgnoreCase", comparer);
```

---

##### `RemoveEqualityComparer<T, TKey>(TKey key)`

移除指定类型的相等性比较器。同时移除泛型和非泛型存储中的对应项。

- **类型参数**：
  - `T`：数据类型
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
- **返回**：`bool`，若任一存储中移除成功则返回 `true`
- **异常**：`ArgumentNullException`（`key` 为 `null`）

```csharp
bool removed = ComparerUtility.RemoveEqualityComparer<string, string>("IgnoreCase");
```

---

#### 非泛型路径

---

##### `GetEqualityComparer<TKey>(TKey key)`

获取指定的相等性比较器（无类型校验）。若未注册则返回 `null`。

- **类型参数**：
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
- **返回**：`IEqualityComparer`，若未注册则返回 `null`
- **异常**：`ArgumentNullException`（`key` 为 `null`）

```csharp
var comparer = ComparerUtility.GetEqualityComparer<string>("IgnoreCase");
```

---

##### `TryGetEqualityComparer<TKey>(TKey key, out IEqualityComparer comparer)`

尝试获取指定的相等性比较器（无类型校验）。

- **类型参数**：
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
  - `comparer`：输出参数
- **返回**：`bool`，成功返回 `true`

```csharp
if (ComparerUtility.TryGetEqualityComparer<string>("IgnoreCase", out var comparer))
{
    // 使用 comparer
}
```

---

##### `GetEqualityComparer<TKey>(TKey key, Type equalityComparerType)`

获取指定的相等性比较器，并进行类型校验。若实际类型与期望类型不一致，抛出 `InvalidCastException`。

- **类型参数**：
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
  - `equalityComparerType`：期望的比较器类型
- **返回**：`IEqualityComparer`，若未注册则返回 `null`
- **异常**：`ArgumentNullException`（`key` 或 `equalityComparerType` 为 `null`）；`InvalidCastException`（类型不匹配）

```csharp
var comparer = ComparerUtility.GetEqualityComparer<string>("IgnoreCase", typeof(OrdinalIgnoreCaseComparer));
```

---

##### `TryGetEqualityComparer<TKey>(TKey key, Type equalityComparerType, out IEqualityComparer comparer)`

尝试获取指定的相等性比较器，并进行类型校验。类型不匹配时返回 `false`。

- **类型参数**：
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
  - `equalityComparerType`：期望的比较器类型
  - `comparer`：输出参数
- **返回**：`bool`，成功返回 `true`

```csharp
if (ComparerUtility.TryGetEqualityComparer<string>("IgnoreCase", typeof(OrdinalIgnoreCaseComparer), out var comparer))
{
    // 使用 comparer
}
```

---

##### `GetEqualityComparerOrDefault<TKey>(TKey key, Type type)`

获取指定的相等性比较器（无类型校验）。若 `key` 为 `null` 或未注册，则返回默认比较器（通过反射获取并缓存）。

- **类型参数**：
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
  - `type`：数据类型（用于获取默认比较器）
- **返回**：`IEqualityComparer`，始终非空
- **异常**：`ArgumentNullException`（`type` 为 `null`）

```csharp
var comparer = ComparerUtility.GetEqualityComparerOrDefault<string>("IgnoreCase", typeof(string));
```

---

##### `GetEqualityComparerOrDefault<TKey>(TKey key, Type type, Type equalityComparerType)`

获取指定的相等性比较器，并进行类型校验。若 `key` 为 `null`、未注册或类型不匹配，则返回默认比较器。

- **类型参数**：
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
  - `type`：数据类型（用于获取默认比较器）
  - `equalityComparerType`：期望的比较器类型
- **返回**：`IEqualityComparer`，始终非空
- **异常**：`ArgumentNullException`（`type` 为 `null`）

```csharp
var comparer = ComparerUtility.GetEqualityComparerOrDefault<string>("IgnoreCase", typeof(string), typeof(OrdinalIgnoreCaseComparer));
```

---

##### `SetEqualityComparer<TKey>(TKey key, IEqualityComparer comparer)`

注册非泛型相等性比较器。

- **类型参数**：
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
  - `comparer`：相等性比较器
- **异常**：`ArgumentNullException`（`key` 或 `comparer` 为 `null`）

```csharp
ComparerUtility.SetEqualityComparer<string>("NonGeneric", new MyNonGenericComparer());
```

---

##### `RemoveEqualityComparer<TKey>(TKey key)`

移除指定的非泛型相等性比较器（仅移除非泛型存储）。

- **类型参数**：
  - `TKey`：比较器实例唯一标识类型
- **参数**：
  - `key`：比较器实例唯一标识
- **返回**：`bool`
- **异常**：`ArgumentNullException`（`key` 为 `null`）

```csharp
bool removed = ComparerUtility.RemoveEqualityComparer<string>("NonGeneric");
```

---

### 二、排序比较器（IComparer）

排序比较器的 API 与相等性比较器完全对称，以下仅列出方法签名，不再重复说明。

#### 泛型路径

| 方法 | 说明 |
|------|------|
| `GetComparer<T, TKey>(TKey key)` | 获取比较器，未注册返回 `null` |
| `TryGetComparer<T, TKey>(TKey key, out IComparer<T> comparer)` | 尝试获取 |
| `GetComparerOrDefault<T, TKey>(TKey key)` | 获取或返回 `Comparer<T>.Default` |
| `SetComparer<T, TKey>(TKey key, IComparer<T> comparer)` | 注册泛型比较器（自动同步到非泛型） |
| `RemoveComparer<T, TKey>(TKey key)` | 移除比较器（同时移除泛型和非泛型） |

#### 非泛型路径

| 方法 | 说明 |
|------|------|
| `GetComparer<TKey>(TKey key)` | 无类型校验 Get |
| `TryGetComparer<TKey>(TKey key, out IComparer comparer)` | 无类型校验 TryGet |
| `GetComparer<TKey>(TKey key, Type comparerType)` | 带类型校验 Get |
| `TryGetComparer<TKey>(TKey key, Type comparerType, out IComparer comparer)` | 带类型校验 TryGet |
| `GetComparerOrDefault<TKey>(TKey key, Type type)` | 无类型校验 OrDefault |
| `GetComparerOrDefault<TKey>(TKey key, Type type, Type comparerType)` | 带类型校验 OrDefault |
| `SetComparer<TKey>(TKey key, IComparer comparer)` | 注册非泛型比较器 |
| `RemoveComparer<TKey>(TKey key)` | 移除非泛型比较器 |

---

### 三、全局管理

---

##### `ClearAll()`

清空所有已注册的比较器（包括泛型和非泛型存储）以及默认值缓存。主要用于测试环境重置。

```csharp
// 在测试 SetUp 中调用
[SetUp]
public void SetUp()
{
    ComparerUtility.ClearAll();
}
```

---

## 使用示例

### 示例 1：注册和使用泛型比较器

```csharp
public class MyDataComparer : IEqualityComparer<MyData>, IComparer<MyData>
{
    public bool Equals(MyData x, MyData y) => x.Id == y.Id;
    public int GetHashCode(MyData obj) => obj.Id.GetHashCode();
    public int Compare(MyData x, MyData y) => x.Id.CompareTo(y.Id);
}

// 注册
ComparerUtility.SetEqualityComparer<MyData, string>("Default", new MyDataComparer());

// 使用
var comparer = ComparerUtility.GetEqualityComparer<MyData, string>("Default");
var set = new HashSet<MyData>(comparer);
```

---

### 示例 2：使用枚举作为业务键

```csharp
public enum ComparisonKind { Default, IgnoreCase, CurrentCulture }

// 注册
ComparerUtility.SetEqualityComparer<string, ComparisonKind>(
    ComparisonKind.IgnoreCase, 
    StringComparer.OrdinalIgnoreCase);

// 使用
var comparer = ComparerUtility.GetEqualityComparer<string, ComparisonKind>(ComparisonKind.IgnoreCase);
bool equal = comparer.Equals("Hello", "HELLO"); // true
```

---

### 示例 3：非泛型反射场景

```csharp
Type elementType = typeof(string);
object key = "IgnoreCase";
var comparerType = typeof(OrdinalIgnoreCaseComparer); // 假设存在

// 运行时获取比较器
var comparer = ComparerUtility.GetEqualityComparer<string>(key, comparerType);
if (comparer != null)
{
    // 使用比较器进行反射驱动的操作
}
```

---

### 示例 4：使用 TryGet 安全获取

```csharp
if (ComparerUtility.TryGetEqualityComparer<int, string>("MyKey", out var comparer))
{
    // 注册存在，使用 comparer
    bool result = comparer.Equals(1, 1);
}
else
{
    // 注册不存在，执行降级逻辑
}
```

---

### 示例 5：多租户场景

```csharp
public enum Tenant { TenantA, TenantB }

// 启动时注册
ComparerUtility.SetEqualityComparer<string, Tenant>(
    Tenant.TenantA, StringComparer.OrdinalIgnoreCase);
ComparerUtility.SetEqualityComparer<string, Tenant>(
    Tenant.TenantB, StringComparer.CurrentCulture);

// 请求处理中
IEqualityComparer<string> comparer = ComparerUtility.GetEqualityComparer<string, Tenant>(currentTenant);
var result = list.Distinct(comparer).ToList();
```

---

### 示例 6：注册非泛型比较器

```csharp
public class MyNonGenericComparer : IEqualityComparer
{
    public new bool Equals(object x, object y) => x?.Equals(y) ?? y == null;
    public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
}

ComparerUtility.SetEqualityComparer<string>("NonGeneric", new MyNonGenericComparer());
var comparer = ComparerUtility.GetEqualityComparer<string>("NonGeneric");
```

---

## 注意事项

1. **类型安全性**：非泛型 `Get` 方法（带类型校验）要求传入的 `Type` 必须与比较器的实际类型**完全一致**，不支持继承关系。
2. **有状态比较器**：本工具类仅适用于**无状态**或**构造后不可变状态**的比较器。禁止注册带有可变字段（如计数器、`IResettable`）的比较器，否则会导致线程安全问题。
3. **默认值缓存**：非泛型 `OrDefault` 方法通过反射获取默认比较器，结果会缓存，首次调用有微小开销。
4. **并发安全**：所有公共方法均为线程安全，可在多线程环境中使用。
5. **测试重置**：在单元测试中，建议在 `[SetUp]` 方法中调用 `ClearAll()` 以避免测试间相互干扰。