> 内容由 AI 根据核心代码生成，已通过人工审核。

# ComparerUtility

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Unity](https://img.shields.io/badge/Unity-2020.3%2B-green.svg)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)
![](https://img.shields.io/badge/Performance-zero%20alloc-brightgreen)

一个轻量级的 C# 比较器策略注册表，支持 `IEqualityComparer`/`IEqualityComparer<T>` 和 `IComparer`/`IComparer<T>` 的统一注册、获取与管理。通过业务键（`TKey`）支持同一类型多个比较器实例，适用于需要集中管理比较策略的大型应用或框架。

## 简介

`ComparerUtility` 为你的应用提供一个全局比较器策略注册中心。你可以为任意类型 `T` 和业务键 `TKey` 注册自定义比较器，之后在任何地方通过相同的键获取该实例。这种设计支持同一类型拥有多种比较规则（如 `"IgnoreCase"`、`"CurrentCulture"`），并允许在运行时动态切换策略。

工具内部采用分层存储模型，通过 `Storage<TKey, TValue>` 实现类型组合级别的隔离，保证不同 `(TKey, TValue)` 组合互不干扰。所有操作均为线程安全，热路径零 GC 分配。

**主要功能**：
- 为同一类型注册多个命名比较器实例（通过业务键 `TKey`）
- 泛型 API（编译时类型安全）与非泛型 API（反射场景）双路径支持
- 泛型与非泛型存储自动同步（当比较器同时实现两类接口时）
- 非泛型路径支持可选的类型校验（精确匹配）
- `TryGet` 模式安全获取，区分“未注册”与“注册为 null”
- `GetOrDefault` 模式无异常回退默认值
- 默认比较器的反射结果自动缓存
- 线程安全的并发访问
- 简洁的缓存清除机制

## 安装环境要求

- .NET Standard 2.0 或更高
- Unity 2020.3 及以上版本
- C# 7.0+

## 安装方式

### 源码导入
将 `ComparerUtility.cs` 文件放入 Unity 项目的 `Assets/Scripts/Utility`（或其他脚本目录）中即可。

### DLL 文件导入
将编译好的程序集（包含 `ComparerUtility` 类）复制到 `Assets/Plugins` 目录下。

## 设计理念

在大型应用或框架中，比较规则（如字符串忽略大小写、按特定字段排序）往往需要全局统一，但同一类型也可能存在多种比较策略（如针对不同租户、不同环境）。`ComparerUtility` 通过引入 **业务键 `TKey`**，让调用者只需 `GetEqualityComparer<T, TKey>(key)` 就能获得当前策略，而具体的比较器可在启动时集中注册，实现了“比较策略”与“业务代码”的解耦。

与普通的 `static` 字段或单例缓存不同，本工具支持：
- **多实例**：同一类型可注册多个不同行为的比较器。
- **运行时装配**：策略可在应用启动后动态切换（通过重新注册覆盖）。
- **类型安全**：非泛型路径要求精确类型匹配，防止因继承导致的误用。

内部存储采用双层隔离，确保不同 `(TKey, TValue)` 组合不会产生键冲突。

## API 速览

| 方法族 | 未注册时行为 | `null` key 行为 | 类型不匹配行为 |
|--------|-------------|----------------|---------------|
| `Get...` | 返回 `null` | 抛出异常 | 抛出异常 |
| `TryGet...` | 返回 `false` | 返回 `false` | 返回 `false` |
| `Get...OrDefault` | 返回默认值 | 返回默认值 | 返回默认值 |

## 具体功能说明

### 相等性比较器（`IEqualityComparer<T>` / `IEqualityComparer`）

- **注册**：使用 `SetEqualityComparer<T, TKey>(TKey key, IEqualityComparer<T> comparer)` 注册泛型比较器；若比较器同时实现非泛型接口，会自动同步到非泛型存储。  
  也可使用 `SetEqualityComparer<TKey>(TKey key, IEqualityComparer comparer)` 直接注册非泛型比较器。
- **获取**：
  - `GetEqualityComparer<T, TKey>(TKey key)` —— 未注册返回 `null`，`key` 为 `null` 抛异常。
  - `TryGetEqualityComparer<T, TKey>(TKey key, out IEqualityComparer<T> comparer)` —— 安全获取，失败返回 `false`。
  - `GetEqualityComparerOrDefault<T, TKey>(TKey key)` —— 未注册或 `key` 为 `null` 时返回 `EqualityComparer<T>.Default`。
- **非泛型重载**：支持传入 `Type` 参数和可选的比较器类型校验，适用于反射场景。
- **移除**：`RemoveEqualityComparer<T, TKey>(TKey key)` 同时移除泛型和非泛型存储中的项。

### 排序比较器（`IComparer<T>` / `IComparer`）

所有功能与相等性比较器对称，方法名为 `SetComparer`、`GetComparer`、`TryGetComparer`、`GetComparerOrDefault`、`RemoveComparer`，默认值回退到 `Comparer<T>.Default`。

### 默认比较器智能缓存

当非泛型 `...OrDefault` 方法需要回退默认比较器时，会通过反射获取 `EqualityComparer<T>.Default` / `Comparer<T>.Default` 并将结果缓存，避免重复反射开销。

### 全局清理

`ClearAll()` 清空所有已注册的比较器（包括泛型和非泛型存储）以及默认值缓存，适用于测试环境重置。

## 使用示例

### 示例 1：注册和使用泛型比较器（字符串忽略大小写）

```csharp
// 注册
ComparerUtility.SetEqualityComparer<string, string>("IgnoreCase", StringComparer.OrdinalIgnoreCase);

// 使用
var comparer = ComparerUtility.GetEqualityComparer<string, string>("IgnoreCase");
bool equal = comparer.Equals("Hello", "HELLO"); // true
```

### 示例 2：使用枚举作为业务键

```csharp
public enum ComparisonKind { Default, IgnoreCase, CurrentCulture }

ComparerUtility.SetEqualityComparer<string, ComparisonKind>(
    ComparisonKind.IgnoreCase,
    StringComparer.OrdinalIgnoreCase
);

var comparer = ComparerUtility.GetEqualityComparer<string, ComparisonKind>(ComparisonKind.IgnoreCase);
```

### 示例 3：使用 `TryGet` 安全获取

```csharp
if (ComparerUtility.TryGetEqualityComparer<int, string>("MyKey", out var comparer))
{
    // 注册存在，使用 comparer
}
else
{
    // 注册不存在，执行降级逻辑
}
```

### 示例 4：非泛型反射场景

```csharp
Type elementType = typeof(string);
object key = "IgnoreCase";
Type comparerType = typeof(OrdinalIgnoreCaseComparer); // 假设存在

var comparer = ComparerUtility.GetEqualityComparer<string>(key, comparerType);
if (comparer != null)
{
    // 使用比较器
}
```

### 示例 5：多租户场景

```csharp
public enum Tenant { TenantA, TenantB }

// 启动时注册
ComparerUtility.SetEqualityComparer<string, Tenant>(Tenant.TenantA, StringComparer.OrdinalIgnoreCase);
ComparerUtility.SetEqualityComparer<string, Tenant>(Tenant.TenantB, StringComparer.CurrentCulture);

// 请求处理中
IEqualityComparer<string> comparer = ComparerUtility.GetEqualityComparer<string, Tenant>(currentTenant);
```

## 常见问题

**Q：为什么泛型 `Get` 方法需要两个类型参数（`T` 和 `TKey`）？**  
A：`T` 是比较器作用的元素类型，`TKey` 是业务键类型。这种设计让你可以灵活选择键类型（如 `string`、枚举、`Guid` 等），同时保证了编译时类型安全。

**Q：非泛型 `Get` 方法的类型校验是干什么用的？**  
A：当你通过 `Type` 参数获取比较器时，工具会校验取出的比较器实际类型是否与传入的 `Type` 完全一致。这防止了因继承关系导致误用基类比较器的情况，确保调用者明确知道自己拿到的是哪种比较器。

**Q：`Get...OrDefault` 在 `key` 为 `null` 时返回默认值，如果我想区分“未注册”和“键为 null”该怎么办？**  
A：使用 `TryGet` 系列方法，它在 `key` 为 `null` 时返回 `false`，且不抛异常，让你明确知道获取是否成功。

**Q：我注册了一个比较器，但非泛型 `Get` 返回 `null`，为什么？**  
A：如果你只使用了 `SetEqualityComparer<T, TKey>` 注册泛型比较器，且该比较器**未同时实现**非泛型 `IEqualityComparer` 接口，则非泛型存储中不会有该比较器。请确保你的比较器同时实现了两个接口，或使用非泛型注册方法。

**Q：工具是否支持可释放（`IDisposable`）的比较器？**  
A：当前版本不接管用户比较器的生命周期，移除缓存时不会调用 `Dispose`。如有需要，请自行管理。

## 文档导航

- [API 文档](./source/1.0.1-beta/DOCUMENT.md)
- [测试报告](./tests/1.0.1-beta/TEST_REPORT.md)
- [发布说明](./RELEASE.md)

## 许可证

本项目基于 [MIT](../../LICENSE) 许可证开源。