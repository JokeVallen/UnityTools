> 内容由 AI 根据核心代码生成，已通过人工审核。

## PoolKit API 文档

PoolKit 是一个高性能的对象池工具库，支持集合类型池化（List、Dictionary、Queue、Stack、HashSet、Array）和 Unity 对象池化（GameObject、Component、自定义 Class）。

---

## 命名空间

| 命名空间 | 用途 |
|---------|-----|
| `PoolKit` | 基础对象池（ClassPool、CollectionPool、BasePool） |
| `PoolKit.Collections` | 集合对象池（ListPool、DictionaryPool、QueuePool、StackPool、HashSetPool、ArrayPool） |
| `PoolKit.Unity` | Unity 专用对象池（GameObjectPool、ComponentPool、UnityObjectPool、UnityObjectPoolSettings） |

---

## 公共 API

### 一、PoolKit 命名空间

#### 1. `BasePool<T>`

对象池抽象基类。

| 成员 | 类型 | 说明 |
|-----|-----|-----|
| `TotalCount` | `int` (get) | 池已创建的对象总数 |
| `FreeCount` | `int` (get) | 当前空闲对象数量 |
| `IsFixed` | `bool` (get) | 是否固定容量 |
| `Capacity` | `int` (get) | 池容量 |
| `OverrideCreate` | `Func<T>` | 自定义对象创建逻辑 |
| `OverrideReset` | `Action<T>` | 自定义对象重置逻辑 |
| `OverrideDestroy` | `Action<T>` | 自定义对象销毁逻辑 |
| `Get()` | `abstract T` | 获取一个对象 |
| `Release(T item)` | `abstract void` | 释放对象回池 |
| `Clear()` | `abstract void` | 清空池 |

---

#### 2. `ClassPool<T>`

Class 类型对象池，要求 `T : class, new()`。

| 构造函数 | 说明 |
|---------|-----|
| `ClassPool()` | 默认容量 100，非固定 |
| `ClassPool(int capacity)` | 指定容量，非固定 |
| `ClassPool(int capacity, bool isFixed)` | 指定容量和是否固定 |

---

#### 3. `CollectionPool<TElement, TCollection>`

集合对象池，要求 `TCollection : class, IEnumerable<TElement>, new()`。

| 构造函数 | 说明 |
|---------|-----|
| `CollectionPool()` | 默认容量 100 |
| `CollectionPool(int capacity)` | 指定容量 |
| `CollectionPool(int capacity, bool isFixed)` | 指定容量和是否固定 |

---

### 二、PoolKit.Collections 命名空间

#### 4. `ListPool`

`List<T>` 对象池（静态类）。

| 方法 | 签名 | 说明 |
|-----|-----|-----|
| `Rent<T>()` | `static Stack<T> Rent<T>()` | 租借一个 List 实例 |
| `RentWithScope<T>()` | `static PoolScope<T> RentWithScope<T>()` | 租借 List 并返回作用域 |
| `Return<T>` | `static void Return<T>(List<T> collection)` | 归还 List 实例 |
| `Clear()` | `static void Clear()` | 清空所有 List 池 |
| `Dispose()` | `static void Dispose()` | 释放所有 List 池 |

**PoolScope 结构：**

| 成员 | 说明 |
|-----|-----|
| `List` | 获取内部的 List 实例 |
| `Dispose()` | 自动归还 List 到池 |

---

#### 5. `DictionaryPool`

`Dictionary<TKey, TValue>` 对象池（静态类）。

| 方法 | 签名 | 说明 |
|-----|-----|-----|
| `Rent<TKey,TValue>()` | `static Dictionary<TKey,TValue> Rent<TKey,TValue>()` | 租借字典 |
| `RentWithScope<TKey,TValue>()` | `static PoolScope<TKey,TValue> RentWithScope<TKey,TValue>()` | 租借并返回作用域 |
| `Return<TKey,TValue>` | `static void Return<TKey,TValue>(Dictionary<TKey,TValue> collection)` | 归还字典 |
| `Clear()` | `static void Clear()` | 清空所有字典池 |
| `Dispose()` | `static void Dispose()` | 释放所有字典池 |

---

#### 6. `QueuePool`

`Queue<T>` 对象池（静态类）。

| 方法 | 签名 | 说明 |
|-----|-----|-----|
| `Rent<T>()` | `static Queue<T> Rent<T>()` | 租借队列 |
| `RentWithScope<T>()` | `static PoolScope<T> RentWithScope<T>()` | 租借并返回作用域 |
| `Return<T>` | `static void Return<T>(Queue<T> collection)` | 归还队列 |
| `Clear()` | `static void Clear()` | 清空所有队列池 |
| `Dispose()` | `static void Dispose()` | 释放所有队列池 |

---

#### 7. `StackPool`

`Stack<T>` 对象池（静态类）。

| 方法 | 签名 | 说明 |
|-----|-----|-----|
| `Rent<T>()` | `static Stack<T> Rent<T>()` | 租借栈 |
| `RentWithScope<T>()` | `static PoolScope<T> RentWithScope<T>()` | 租借并返回作用域 |
| `Return<T>` | `static void Return<T>(Stack<T> collection)` | 归还栈 |
| `Clear()` | `static void Clear()` | 清空所有栈池 |
| `Dispose()` | `static void Dispose()` | 释放所有栈池 |

---

#### 8. `HashSetPool`

`HashSet<T>` 对象池（静态类）。

| 方法 | 签名 | 说明 |
|-----|-----|-----|
| `Rent<T>()` | `static HashSet<T> Rent<T>()` | 租借哈希集合 |
| `RentWithScope<T>()` | `static PoolScope<T> RentWithScope<T>()` | 租借并返回作用域 |
| `Return<T>` | `static void Return<T>(HashSet<T> collection)` | 归还哈希集合 |
| `Clear()` | `static void Clear()` | 清空所有哈希集合池 |
| `Dispose()` | `static void Dispose()` | 释放所有哈希集合池 |

---

#### 9. `ArrayPool`

数组对象池（静态类）。支持按大小分桶缓存。

| 方法 | 签名 | 说明 |
|-----|-----|-----|
| `Configure(int maxArraySizeMB, int maxArraysPerBucket)` | `static void Configure(...)` | 全局配置（首次使用前调用） |
| `WarmUp<T>(int minimumLength, int count)` | `static void WarmUp<T>(...)` | 预热指定大小的数组池 |
| `Rent<T>(int minimumLength)` | `static T[] Rent<T>(int minimumLength)` | 租借数组 |
| `RentWithScope<T>(int minimumLength)` | `static PoolScope<T> RentWithScope<T>(int minimumLength)` | 租借并返回作用域 |
| `Return<T>(T[] array, bool clearArray)` | `static void Return<T>(T[] array, bool clearArray)` | 归还数组 |
| `Clear()` | `static void Clear()` | 清空所有数组池 |
| `Dispose()` | `static void Dispose()` | 释放所有数组池 |

**注意事项：**
- `minimumLength = 0` 时返回 `Array.Empty<T>()`，不会从池中获取
- 数组大小会被向上取整到 2 的幂（16, 32, 64, ...）
- `Configure()` 必须在首次 `Rent` 调用前执行

---

### 三、PoolKit.Unity 命名空间

#### 10. `UnityObjectPoolSettings<T>`

Unity 对象池配置类。

| 字段 | 类型 | 默认值 | 说明 |
|-----|-----|-------|-----|
| `capacity` | `int` | 100 | 池容量 |
| `isPersistant` | `bool` | true | 是否持久化（`DontDestroyOnLoad`） |
| `isFixed` | `bool` | false | 是否固定容量 |
| `container` | `GameObject` | null | 池容器对象 |
| `original` | `T` | null | 对象原型 |
| `defaultName` | `string` | `""` | 对象默认名称 |
| `activeWhenGet` | `bool` | true | 获取时是否激活 |

---

#### 11. `UnityObjectPool<T>`

Unity 对象池基类，`T : UnityEngine.Object`。

| 成员 | 类型 | 说明 |
|-----|-----|-----|
| `container` | `protected GameObject` | 容器对象 |
| `original` | `protected T` | 对象原型 |
| `defaultName` | `protected string` | 默认名称 |
| `activeWhenGet` | `protected bool` | 获取时是否激活 |
| `Get()` | `public override T` | 获取对象 |
| `Release(T item)` | `public override void` | 释放对象 |
| `Clear()` | `public override void` | 清空池 |

---

#### 12. `GameObjectPool`

`GameObject` 专用对象池。

| 构造函数 | 说明 |
|---------|-----|
| `GameObjectPool()` | 默认容量 100 |
| `GameObjectPool(int capacity)` | 指定容量 |
| `GameObjectPool(int capacity, bool isFixed)` | 指定容量和是否固定 |
| `GameObjectPool(UnityObjectPoolSettings<GameObject> settings)` | 使用配置对象 |

**行为：**
- `Get()` 时自动 `SetActive(true)`
- `Release()` 时自动 `SetActive(false)` 并设为容器子级

---

#### 13. `ComponentPool<T>`

Component 专用对象池，`T : Component`。

| 构造函数 | 说明 |
|---------|-----|
| `ComponentPool()` | 默认容量 100 |
| `ComponentPool(int capacity)` | 指定容量 |
| `ComponentPool(int capacity, bool isFixed)` | 指定容量和是否固定 |
| `ComponentPool(UnityObjectPoolSettings<T> settings)` | 使用配置对象 |

**行为：**
- `Get()` 时自动启用 Component（如为 `Behaviour` 类型）
- `Release()` 时自动禁用 Component（如为 `Behaviour` 类型）

---

## 使用示例

### 示例 1：ListPool 基本使用

```csharp
using PoolKit.Collections;

// 方式一：手动 Rent/Return
var list = ListPool.Rent<int>();
try
{
    list.Add(1);
    list.Add(2);
    list.Add(3);
    // 使用 list...
}
finally
{
    ListPool.Return(list);
}

// 方式二：using 作用域（推荐）
using (var scope = ListPool.RentWithScope<int>())
{
    scope.List.Add(1);
    scope.List.Add(2);
    // scope.Dispose() 自动归还
}
```

### 示例 2：DictionaryPool 使用

```csharp
using PoolKit.Collections;

using (var scope = DictionaryPool.RentWithScope<string, int>())
{
    scope.Dictionary["key1"] = 100;
    scope.Dictionary["key2"] = 200;
    // 自动归还
}
```

### 示例 3：ArrayPool 使用

```csharp
using PoolKit.Collections;

// 配置（应用启动时执行一次）
ArrayPool.Configure(maxArraySizeMB: 4, maxArraysPerBucket: 32);

// 预热（可选）
ArrayPool.WarmUp<int>(minimumLength: 64, count: 10);

// 租借数组
int[] array = ArrayPool.Rent<int>(minimumLength: 50);
try
{
    array[0] = 42;
    // array 实际长度为 64（向上取整到 2 的幂）
}
finally
{
    ArrayPool.Return(array, clearArray: true);
}
```

### 示例 4：ClassPool 使用

```csharp
using PoolKit;

public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

var pool = new ClassPool<MyClass>(capacity: 50, isFixed: false);

// 自定义创建和重置逻辑
pool.OverrideCreate = () => new MyClass();
pool.OverrideReset = (obj) => { obj.Id = 0; obj.Name = null; };

var obj = pool.Get();
obj.Id = 123;
obj.Name = "Test";
pool.Release(obj);
```

### 示例 5：GameObjectPool 使用

```csharp
using PoolKit.Unity;
using UnityEngine;

// 方式一：简单创建
var goPool = new GameObjectPool(capacity: 100);

// 方式二：使用配置
var settings = new UnityObjectPoolSettings<GameObject>
{
    capacity = 200,
    isPersistant = true,
    isFixed = false,
    container = new GameObject("MyPoolContainer"),
    original = prefab,
    defaultName = "Enemy",
    activeWhenGet = true
};
var goPool2 = new GameObjectPool(settings);

// 获取和释放
GameObject go = goPool.Get();
go.transform.position = Vector3.zero;
goPool.Release(go);
```

### 示例 6：ComponentPool 使用

```csharp
using PoolKit.Unity;
using UnityEngine;

public class MyComponent : MonoBehaviour
{
    public int Health { get; set; }
}

var compPool = new ComponentPool<MyComponent>(capacity: 50);
var comp = compPool.Get();
comp.Health = 100;
// 使用 comp...
compPool.Release(comp);
```

### 示例 7：CollectionPool 使用

```csharp
using PoolKit;

// 池化 List<int>
var listPool = new CollectionPool<int, List<int>>(capacity: 20);

var list = listPool.Get();
list.AddRange(new[] { 1, 2, 3, 4, 5 });
listPool.Release(list);
```

---