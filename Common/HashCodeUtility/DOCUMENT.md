> 内容由 AI 根据核心代码生成，已通过人工审核。

# HashCodeUtility API 文档

## 公共 API

### `HashCodeUtility.GetHashCode<T>`

**签名**
```csharp
public static int GetHashCode<T>(T obj1)
```

**作用**  
计算单个对象的哈希码。  
使用 `EqualityComparer<T>.Default`，null 对象视为 0。

---

### `HashCodeUtility.Combine<T1, T2>`

**签名**
```csharp
public static int Combine<T1, T2>(T1 obj1, T2 obj2)
```

**作用**  
合并两个对象的哈希码，顺序敏感。

---

### `HashCodeUtility.Combine<T1, T2, T3>`

**签名**
```csharp
public static int Combine<T1, T2, T3>(T1 obj1, T2 obj2, T3 obj3)
```

**作用**  
合并三个对象的哈希码，顺序敏感。

---

### `HashCodeUtility.Combine<T1, T2, T3, T4>`

**签名**
```csharp
public static int Combine<T1, T2, T3, T4>(T1 obj1, T2 obj2, T3 obj3, T4 obj4)
```

**作用**  
合并四个对象的哈希码，顺序敏感。

---

### `HashCodeUtility.Combine<T1, T2, T3, T4, T5>`

**签名**
```csharp
public static int Combine<T1, T2, T3, T4, T5>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5)
```

**作用**  
合并五个对象的哈希码，顺序敏感。  
超过 5 个参数建议使用 `CombineAll<T>`。

---

### `HashCodeUtility.CombineAll<T>`

**签名**
```csharp
public static int CombineAll<T>(params T[] objects)
```

**作用**  
合并任意数量同类型对象的哈希码，顺序敏感。  
null 数组返回 0，空数组返回种子值 17。  
使用 `EqualityComparer<T>.Default` 获取各元素哈希。

---

### `HashCodeUtility.CombineAll`

**签名**
```csharp
public static int CombineAll(params object[] objects)
```

**作用**  
合并任意数量 `object` 的哈希码，顺序敏感。  
通过 `object.GetHashCode()` 获取哈希（值类型会被装箱）。  
注意：值类型哈希可能与泛型版本不同，取决于运行时实现。

---

### `HashCodeUtility.GetOrderDependentHashCode<T> (T[])`

**签名**
```csharp
public static int GetOrderDependentHashCode<T>(T[] array)
```

**作用**  
计算数组元素的顺序依赖哈希码。  
null 数组返回 0，空数组返回 17。  
使用默认比较器。

---

### `HashCodeUtility.GetOrderDependentHashCode<T> (T[], IEqualityComparer<T>)`

**签名**
```csharp
public static int GetOrderDependentHashCode<T>(T[] array, IEqualityComparer<T> comparer)
```

**作用**  
使用指定比较器计算数组的顺序依赖哈希码。  
`comparer` 为 null 时回退到默认比较器。

---

### `HashCodeUtility.GetOrderDependentHashCode<T> (IEnumerable<T>)`

**签名**
```csharp
public static int GetOrderDependentHashCode<T>(IEnumerable<T> enumerable)
```

**作用**  
计算序列元素的顺序依赖哈希码，适用于任何 `IEnumerable<T>` 实现。  
null 序列返回 0。

---

### `HashCodeUtility.GetOrderDependentHashCode<T> (IEnumerable<T>, IEqualityComparer<T>)`

**签名**
```csharp
public static int GetOrderDependentHashCode<T>(IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
```

**作用**  
使用指定比较器计算序列的顺序依赖哈希码。  
`comparer` 为 null 时使用默认比较器。

---

## 使用示例

### 基本使用

```csharp
int hash = HashCodeUtility.GetHashCode("hello");

int combined = HashCodeUtility.Combine("foo", 42, true);

int fromArray = HashCodeUtility.CombineAll(1, 2, 3, 4, 5);
```

### 顺序敏感哈希

```csharp
int h1 = HashCodeUtility.GetOrderDependentHashCode(new[] {"a", "b", "c"});
int h2 = HashCodeUtility.GetOrderDependentHashCode(new[] {"c", "b", "a"});
// h1 != h2
```

### 自定义比较器

```csharp
var list = new List<string> { "Ab", "cd" };
int caseSensitive = HashCodeUtility.GetOrderDependentHashCode(list);
int caseInsensitive = HashCodeUtility.GetOrderDependentHashCode(
    list, StringComparer.OrdinalIgnoreCase);
```

### 注意非泛型与泛型的差异

```csharp
int genericHash = HashCodeUtility.CombineAll(1, 2, 3);
int nonGenericHash = HashCodeUtility.CombineAll((object)1, (object)2, (object)3);
// 两个哈希可能不同，因值类型装箱后使用 ValueType.GetHashCode()
```