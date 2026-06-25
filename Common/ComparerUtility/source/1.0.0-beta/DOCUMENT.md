> 内容由 AI 根据核心代码生成，已通过人工审核。

# API 文档 - ComparerUtility

`ComparerUtility` 是一个静态工具类，提供相等性比较器与排序比较器的全局缓存、设置、移除和清空功能，支持泛型/非泛型 API 以及仅实现单一接口的比较器适配。

## 公共 API

### 相等性比较器 (EqualityComparer)

#### 获取

```csharp
public static IEqualityComparer<T> GetEqualityComparer<T>()
```
获取类型 `T` 的相等性比较器。优先返回自定义缓存中的实例；未命中则返回 `EqualityComparer<T>.Default`。

```csharp
public static IEqualityComparer GetEqualityComparer(Type type)
```
获取指定 `Type` 的相等性比较器。优先返回自定义缓存；未命中则通过反射获取默认比较器并缓存结果。  
**异常**：`type` 为 `null` 时抛出 `ArgumentNullException`；类型不支持时抛出 `InvalidOperationException`。

#### 设置

```csharp
public static void SetEqualityComparer<T>(IEqualityComparer<T> comparer)
```
为类型 `T` 设置自定义相等性比较器。若该比较器同时实现了 `IEqualityComparer`，直接缓存原对象；否则内部自动包装为适配器。

```csharp
public static void SetEqualityComparer(Type type, IEqualityComparer comparer)
```
为指定类型设置非泛型相等性比较器。  
**异常**：任一参数为 `null` 时抛出 `ArgumentNullException`。

#### 移除

```csharp
public static bool TryRemoveEqualityComparer<T>()
public static bool TryRemoveEqualityComparer(Type type)
```
尝试移除指定类型的相等性比较器（包括自定义缓存和默认反射缓存）。只要任一处存在缓存并移除即返回 `true`；否则返回 `false`。  
**异常**：`type` 为 `null` 时抛出 `ArgumentNullException`。

#### 清空

```csharp
public static void ClearEqualityComparers()
```
清空所有相等性比较器的自定义缓存和反射缓存。

---

### 排序比较器 (Comparer)

#### 获取

```csharp
public static IComparer<T> GetComparer<T>()
```
获取类型 `T` 的排序比较器。优先返回自定义缓存；未命中时返回 `Comparer<T>.Default`。

```csharp
public static IComparer GetComparer(Type type)
```
获取指定 `Type` 的排序比较器。优先返回自定义缓存；未命中时通过反射获取默认比较器并缓存。  
**异常**：`type` 为 `null` 抛出 `ArgumentNullException`；类型不支持时抛出 `InvalidOperationException`。

#### 设置

```csharp
public static void SetComparer<T>(IComparer<T> comparer)
```
为类型 `T` 设置自定义排序比较器。若同时实现了 `IComparer`，直接缓存原对象；否则自动包装。

```csharp
public static void SetComparer(Type type, IComparer comparer)
```
为指定类型设置非泛型排序比较器。  
**异常**：任一参数为 `null` 抛出 `ArgumentNullException`。

#### 移除

```csharp
public static bool TryRemoveComparer<T>()
public static bool TryRemoveComparer(Type type)
```
尝试移除指定类型的排序比较器缓存（自定义和反射缓存）。移除成功返回 `true`。  
**异常**：`type` 为 `null` 抛出 `ArgumentNullException`。

#### 清空

```csharp
public static void ClearComparers()
```
清空所有排序比较器的自定义缓存和反射缓存。

---

## 使用示例

### 1. 全局替换字符串相等比较（忽略大小写）
```csharp
ComparerUtility.SetEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);
var cmp = ComparerUtility.GetEqualityComparer<string>();
bool equal = cmp.Equals("Hello", "hello"); // true
```

### 2. 自定义实体排序（按时间降序）
```csharp
public class DescTimeComparer : IComparer<MyEntity>
{
    public int Compare(MyEntity x, MyEntity y) => y.Time.CompareTo(x.Time);
}
ComparerUtility.SetComparer<MyEntity>(new DescTimeComparer());
list.Sort(ComparerUtility.GetComparer<MyEntity>());
```

### 3. 仅非泛型接口的遗留比较器
```csharp
var legacy = new LegacyEqualityComparer(); // 只实现 IEqualityComparer
ComparerUtility.SetEqualityComparer(typeof(string), legacy);
// 泛型获取仍正常工作（自动适配）
var cmp = ComparerUtility.GetEqualityComparer<string>();
bool eq = cmp.Equals("a", "a"); // true
```

### 4. 动态类型支持
```csharp
Type t = typeof(MyEntity);
IEqualityComparer ec = ComparerUtility.GetEqualityComparer(t);
int hash = ec.GetHashCode(myObj);
```

### 5. 移除与清空缓存
```csharp
ComparerUtility.TryRemoveEqualityComparer<string>();
ComparerUtility.ClearComparers();
```