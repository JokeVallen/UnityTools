## 版本 1.0.1-beta (2026-06-25)

> 内容由 AI 根据核心代码生成，已通过人工审核。

本次版本是一次功能性发布，将工具从简单的比较器缓存重新定义为功能完整的**比较器策略注册表**。引入了对每个类型支持多个命名实例、全新的存储架构、增强的类型安全性以及全面的测试覆盖。

---

### 🚀 自 1.0.0-beta 以来的主要变更

#### 1. 架构重构：从单例缓存到策略注册表
- **之前**：单一的 `ConcurrentDictionary<Type, IEqualityComparer>` 限制了每个类型只能有一个比较器实例。
- **现在**：采用分层存储模型（`Storage<TKey, TValue>`），通过**目标类型 `T`** 和**业务键 `TKey`** 双重维度隔离比较器。这使得同一类型可以拥有多个比较器实例（例如 `"IgnoreCase"`、`"CurrentCulture"`）。

#### 2. 全新 API 设计——双路径：泛型 + 非泛型
- 泛型方法（如 `GetEqualityComparer<T, TKey>(TKey key)`）现在要求传入强类型键，确保编译时类型安全。
- 为非泛型反射场景提供了重载（如 `GetEqualityComparer<TKey>(TKey key, Type equalityComparerType)`），并支持可选的类型校验。
- 方法明确区分为 `Get...`（未注册时返回 `null`）和 `Get...OrDefault`（回退到默认比较器），使调用意图清晰明确。

#### 3. 泛型与非泛型存储的内置同步
- 当通过 `SetEqualityComparer<T, TKey>` 注册泛型比较器时，如果该比较器同时实现了 `IEqualityComparer`（或 `IComparer`），会自动同步到非泛型存储。这确保了两套 API 家族看到相同的数据。

#### 4. 非泛型路径的类型安全保障
- `GetEqualityComparer<TKey>(TKey key, Type equalityComparerType)` 重载会校验获取的比较器实际类型是否与传入的 `Type` 参数**完全一致**。这防止了因继承关系导致的误用（例如为派生类型误用了基类的比较器）。

#### 5. 新增 `TryGet` 系列方法
- 新增 `TryGetEqualityComparer<T, TKey>` 和 `TryGetComparer<T, TKey>` 泛型方法，通过 `out` 参数返回比较器，通过 `bool` 返回值指示是否成功。
- 新增非泛型 `TryGetEqualityComparer<TKey>` 和 `TryGetComparer<TKey>` 重载，支持带/不带类型校验。
- 与 `Get` 系列方法相比，`TryGet` 在 `key` 为 `null` 或类型不匹配时返回 `false` 而非抛出异常，更适合需要区分“未注册”和“注册为 null”的场景。

#### 6. `...OrDefault` 方法对 `null` 参数的处理优化
- 泛型 `GetEqualityComparerOrDefault<T, TKey>` 和 `GetComparerOrDefault<T, TKey>` 在 `key` 为 `null` 时返回默认比较器，不再抛出 `ArgumentNullException`。
- 非泛型 `GetEqualityComparerOrDefault<TKey>` 和 `GetComparerOrDefault<TKey>` 在 `key` 为 `null` 时返回默认比较器。
- 带类型校验的 `...OrDefault` 重载在 `comparerType` 为 `null` 时同样返回默认比较器。
- 此变更使 `OrDefault` 的语义更加统一：无论 `key` 为 `null` 还是未注册，均返回默认值。

#### 7. 性能优化
- **泛型路径**：直接使用 CLR 缓存的 `EqualityComparer<T>.Default` / `Comparer<T>.Default`，无额外开销。
- **非泛型默认值获取**：现在通过 `ConcurrentDictionary<Type, IEqualityComparer>` 和 `ConcurrentDictionary<Type, IComparer>` 进行缓存，消除了重复反射。
- 内部的 `ConcurrentDictionary` 确保线程安全的无锁访问。

#### 8. 移除冗余复杂度
- 移除了之前的适配器类（`EqualityComparerAdapter<T>`、`ComparerAdapter<T>`）及其关联的内部接口。新的存储设计直接存储 `IEqualityComparer<T>` 和 `IComparer<T>`，简化了代码库并降低了维护成本。

#### 9. 移除旧的默认值缓存
- 移除了独立的 `DefaultEqualityComparerCache` 和 `DefaultComparerCache` 静态类。泛型路径的默认值现在直接从 CLR 获取，非泛型路径的默认值缓存在新的专用字典中。

#### 10. ClearAll 现在清空所有数据
- `ClearAll()` 同时清空主存储和默认值缓存，确保在测试设置中安全使用，避免测试间相互干扰。

#### 11. 全面的测试覆盖
- 新增**单元测试**，覆盖所有公共 API（泛型/非泛型的 Get/Set/Remove、TryGet、OrDefault 变体、类型校验、ClearAll）。
- 新增**性能基准测试**（使用 Unity Performance Testing 框架），覆盖多数据规模（10、100、1000、10000 个键）和并发场景，测量执行时间和 GC 分配。
- 两套测试套件均在 **EditMode** 下运行。

---

### ⚠️ 破坏性变更

- **API 重命名/移除**：旧的无键方法 `GetEqualityComparer<T>()` 已不存在。请使用新的基于键的重载。
- **移除适配器类**：任何依赖旧适配器接口（`IEqualityComparerAdapter` 等）的代码需要更新。
- **默认回退行为变更**：非泛型 `Get...OrDefault` 方法现在要求显式传入元素类型的 `Type` 参数，且返回缓存的默认比较器而非 `null`。
- **`...OrDefault` 方法不再因 `key` 为 `null` 而抛出异常**，改为返回默认值。若需要区分 `null` key 与未注册，请使用 `TryGet` 系列方法。

---

### 🔧 升级指南（从 1.0.0-beta 升级）

1. **将单实例调用替换为带键的调用**：
   ```csharp
   // 旧代码
   var cmp = ComparerUtility.GetEqualityComparer<string>();
   // 新代码
   var cmp = ComparerUtility.GetEqualityComparer<string, string>("myKey");
   ```
2. **对于非泛型反射场景**，显式传入比较器类型：
   ```csharp
   var cmp = ComparerUtility.GetEqualityComparer<string>(key, typeof(MyComparer));
   ```
3. 如果使用了 `SetEqualityComparer<T>(IEqualityComparer<T>)`，请改为 `SetEqualityComparer<T, TKey>(TKey key, IEqualityComparer<T>)`。
4. 在测试中，在 `[SetUp]` 方法中调用 `ComparerUtility.ClearAll()` 以确保测试隔离。
5. 若之前依赖 `...OrDefault` 方法对 `null` key 抛异常的行为，请改用 `Get...`（会抛异常）或调整逻辑。

---

### 📦 依赖更新

- `com.unity.test-framework` ≥ 1.1.33
- `com.unity.test-framework.performance` ≥ 3.0.3（用于性能基准测试）

---

### 📝 已知限制（Beta 版）

- 非泛型 `Get` 方法不支持基于继承关系的类型兼容性（要求精确类型匹配）。这是有意为之的设计，以避免歧义行为。
- 不支持跨域重载的注册比较器序列化或持久化。

---

### ⚠️ 兼容性

- **不兼容 1.0.0-beta 版本**

---

### 📊 API 速览表

| 方法族 | 未注册时行为 | null key 行为 | 类型不匹配行为 |
|--------|-------------|---------------|---------------|
| `Get...` | 返回 `null` | 抛出异常 | 抛出异常 |
| `TryGet...` | 返回 `false` | 返回 `false` | 返回 `false` |
| `Get...OrDefault` | 返回默认值 | 返回默认值 | 返回默认值 |

---

### 📈 性能亮点（在 Unity 2020.3.48f1 上测量）

- **泛型 Get**（10k 个键）：约 **0.0023** 毫秒，0 GC 分配。
- **非泛型 Get**（10k 个键）：约 **0.0032** 毫秒，0 GC 分配。
- **`TryGet`**（10k 个键）：约 **0.0035** 毫秒，0 GC 分配。
- **并发读取**（10 个线程，每线程 1000 次读取）：总计约 **0.8~0.9** 毫秒，0 GC 分配。

---

### 🎯 总结

本次发布将工具从简单的缓存转变为**生产就绪的比较器策略注册表**，提供：

- 通过业务键支持多实例。
- 清晰明确的 API，区分 `Get`、`TryGet` 和 `OrDefault` 语义。
- 健壮的类型安全和异常处理。
- 出色的性能，所有热路径零 GC 分配。

建议升级以获得改进的架构和灵活性。欢迎反馈和问题报告。

---

**版本** 1.0.1-beta  
**发布日期** 2026-06-25  
**维护者** JokeVallen