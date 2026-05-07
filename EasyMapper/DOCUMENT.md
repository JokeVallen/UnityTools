> 内容由 AI 根据核心代码生成，已通过人工审核。

# EasyMapper API 文档

本文档只涵盖命名空间 `EasyMapper` 和 `EasyMapper.Runtime` 中的 **public** 类型与成员，注意命名空间 `EasyMapper` 包含的是框架层，而 `EasyMapper.Runtime` 是可选的默认实现，如果你有更好的方案完全可以自行基于框架层进行定制。

---

## 框架接口（EasyMapper）

### `IBlueprint<TSource, TToken>`
**数据源到令牌的转换蓝图**  
- `TToken Refine(TSource source)` – 将数据源提炼为令牌。  
- `TSource Restore(TToken token)` – 通过令牌还原数据源（可选实现，可能抛出异常）。

### `IPipeline<TSource, TToken>` where TToken : struct
**映射流水线**  
- `TToken Import(TSource source)` – 导入数据源并返回令牌。  
- `TSource Export(TToken token)` – 从令牌导出数据源。

### `IPackage<TToken>`
**令牌序列化器**  
- `byte[] Wrap(TToken token)` – 序列化令牌为字节数组。  
- `TToken Unwrap(byte[] bytes)` – 从字节数组反序列化令牌。

### `IFeature`
**蓝图特征标记**  
- `bool IsTraceable { get; }` – 令牌是否可通过算法直接还原数据源。

### `IIdentity<TToken> : IEquatable<TToken>` where TToken : struct
**令牌类型标识** – 实现此接口的结构体可作为令牌使用。

---

## 运行时核心（EasyMapper.Runtime）

### 令牌类型
- **`LongToken`** – 64位长整型令牌，可隐式转换为 `long`/从 `long` 转换。
- **`GuidToken`** – 128位 GUID 令牌，可隐式转换为 `Guid`/从 `Guid` 转换。
- `TokenWrapper<TToken>` (internal) – 值类型令牌的引用包装，供内部使用。

### 蓝图
| 类 | 说明 |
|----|------|
| `Char10PackingBlueprint` | 短字符串可逆编码蓝图（≤10字符，[a-z0-9_-]）。 |
| `InterningBlueprint` | 通用字符串驻留蓝图，用自增 ID 保证唯一性。 |
| `SmartDistributor` | 智能分发器，根据字符串特征自动选择快速/回退蓝图。 |
| `ObjectNamingBlueprint` | 提取 `UnityEngine.Object.name` 并委托字符串蓝图生成令牌。 |

### 流水线
| 类 | 说明 |
|----|------|
| `StandardPipeline<TSource, TToken>` | 标准强引用字典流水线。 |
| `UnityWeakPipeline<TSource, TToken>` | Unity 对象弱引用流水线，防内存泄漏。 |
| `ThreadSafePipeline<TSource, TToken>` | 线程安全装饰器，为任意流水线加锁。 |
| `CappedPipeline<TSource, TToken>` | LRU 容量限制独立流水线。 |
| `CacheFirstPipeline<TSource, TToken>` | 缓存优先装饰器，避免重复调用 Import。 |
| `IdempotentPipeline<TSource, TToken>` | 幂等装饰器，保证同一源多次导入返回相同令牌。 |
| `DiagnosticPipeline<TSource, TToken>` | 诊断装饰器，统计次数并提供事件。 |
| `GuardedPipeline<TSource, TToken>` | 参数校验装饰器，对 null/默认值抛出异常。 |

### 序列化器
| 类 | 说明 |
|----|------|
| `BinaryIdentityPackage` | `LongToken` 与 8 字节数组互转。 |
| `GuidBinaryPackage` | `GuidToken` 与 16 字节数组互转。 |

### 可维护接口
- **`IMaintainable`** – 提供 `Count` 属性和 `Cleanup()` 方法，用于查询条目数和清理映射。

### 静态入口
- **`IDMap`** – 全局静态门面，提供对字符串和 `UnityEngine.Object` 的快捷映射方法。  
  可通过 `IDMap.Current` 替换内部 `IDMapInstance`，实现自定义行为。  
  - `long Assign(string name)` / `string Locate(long id)`  
  - `long Assign(Object obj)` / `T Locate<T>(long id)`  
  - `byte[] Pack(long id)` / `long Unpack(byte[] bytes)`  
  - `void Cleanup()` / `bool ContainsString(long)` / `bool ContainsObject(long)`  

- **`IDMapInstance`** – 可配置的映射实例，嵌套 `Builder` 构建器类，可注入自定义流水线和序列化器。

---

## 使用示例

### 1. 默认字符串映射
```csharp
long playerId = IDMap.Assign("player");
string name = IDMap.Locate(playerId); // "player"
```

### 2. Unity 对象映射（弱引用）
```csharp
GameObject enemy = ...;
long token = IDMap.Assign(enemy);
GameObject restored = IDMap.Locate<GameObject>(token);
// enemy 销毁后 restored 为 null
```

### 3. 自定义流水线（启用容量限制和诊断）
```csharp
var instance = IDMapInstance.Builder.Create()
    .UseStringPipeline(
        new GuardedPipeline<string, LongToken>(
            new CappedPipeline<string, LongToken>(
                new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint()),
                new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint()),
                maxEntries: 5000)))
    .Build();

IDMap.Current = instance;
long id = IDMap.Assign("my_key");
```

### 4. 为自定义类型构建映射（不依赖 IDMap）
```csharp
var intBlueprint = new IntIdentityBlueprint(); // 用户实现 IBlueprint<int, LongToken>
var pipeline = new StandardPipeline<int, LongToken>(intBlueprint, intBlueprint);
long token = pipeline.Import(42);
int value = pipeline.Export(token); // 42
```

### 5. 序列化令牌到网络流
```csharp
byte[] data = IDMap.Pack(token);
// 网络发送...
long received = IDMap.Unpack(data);
```