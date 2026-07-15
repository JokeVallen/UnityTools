本框架是一个面向高性能 RPG 游戏设计的底层基石资产。整体设计严格遵循**高内聚低耦合**、**面向对象与函数式编程结合**、**零装箱/拆箱内存优化**以及**极致克制的架构留白**哲学。本框架不绑定任何游戏引擎（如 Unity），具备完全的服务器/客户端双端同源复用能力与 100% 的单元测试友好度。

---

## 🛠 核心架构哲学与设计思想

在传统的游戏战斗与属性系统开发中，开发者往往面临三个痛点：数据类型不安全导致的运行时频繁装箱（如 `Dictionary<string, object>`）、组件遍历更新时修改导致的死锁或异常、以及底层逻辑与策划业务数据高度耦合导致的难以重构。

本框架通过精妙的类型隔离、契约裁剪与数据擦除技术，完美地实现了底层负责机制（Mechanism），业务层负责策略（Policy）的现代架构意图。

---

## 💎 五大硬核底座机制解析

### 1. 动态双层容器与零装箱隔离机制（Type-Safe Storage Split）

框架在 `AttributeCollection` 与 `TypedContext` 的底层设计中，摒弃了传统的 `object` 动态容器方案，采用独创的**双层强类型仓库隔离技术**：

* **外层视图**：使用一个非泛型的 `Dictionary<Type, IStorage>` 作为分发中心，这里的 `Type` 实际上是内层泛型仓库的元数据类型。
* **内层仓库**：定义私有内部类 `Storage<TKey, TValue> : IStorage`，其内部包裹纯粹的强类型字典 `Dictionary<TKey, Attribute<TValue>>`。
* **技术价值**：当外部存取属性时，底层首先通过反射获取对应的强类型仓库，随后在**百分之百强类型**的内层字典内进行高速查阅。**数值类型在进入容器到被取出的整个生命周期中，绝对不发生任何装箱与拆箱行为**，这为海量实体的高频数值战斗计算提供了极其稳定的帧率保障，将 GC 压力压榨至 0。

### 2. 精确差异化变更通知与判等优化（Equality Comparer Optimisation）

在 `INotifiableAttributeCollection` 的实现中，属性的变动不仅触发事件，更在底层引入了显式注入的 `IEqualityComparer<TValue>` 相等性比较器：

* 底层在 `Set` 写入新值时，首先调用对应类型的比较器进行强类型判等。
* 只有当新旧数值**真正不相等**时，才会更新底层只读结构体并安全地向下游（如 UI 或战斗 Log 监听者）抛出 `OnChanged` 事件。
* **技术价值**：完美阻断了因 Buff 每帧刷新相同数值而导致的伪变更通知，极大降低了表现层与 UI 层的无谓重写开销。

### 3. 防重入的帧末延迟命令队列（Command Deferred Buffer）

在 Buff 系统的 `Tick` 推进过程中，最忌讳在遍历中途动态修改集合（例如 Buff 造成伤害导致目标死亡，触发被动移除了其他 Buff），这会抛出灾难性的 `InvalidOperationException` 集合修改异常。

* `BuffComponent` 引入了 `isUpdating` 运行期状态保护开关。
* 在 `Tick` 推进期间，外界所有的 `Add`、`Remove`、`Clear` 请求都会被**原子化包裹**为 `PendingCommand` 结构体，安全推入待处理队列。
* 直到当前帧遍历完全结束、状态重置后，系统才会在帧末安全消费命令队列。
* **技术价值**：从根本上杜绝了重入死锁与集合遍历冲突，构建了无懈可击的 Buff 状态机生命周期。

### 4. 显式只读视图隔离与编译期防御（Interface Segregation）

框架严格遵循**接口隔离原则（ISP）**，将属性系统拆分为 `IReadOnlyAttributeCollection`（只读视图）、`IAttributeCollection`（读写视图）与 `INotifiableAttributeCollection`（事件视图）。

* **技术价值**：当属性集合被传递至伤害公式（`IAttributeFormula`）或外部读取器（`AttributeReader`）时，系统在**编译期就将其锁死在只读视图**中。从语言层面掐断了任何人在编写计算公式时误改角色原始属性的温床。

### 5. 纯防御性的值类型包装器（Struct Wrapper Pattern）

无论是属性值 `Attribute<T>` 还是可选组件包装 `Optional<T>`，框架坚决杜绝 `null` 的隐患，全面采用 `readonly struct` 进行防御：

* **技术价值**：完全分配在栈上，不带来任何堆内存开销。同时利用 `.HasValue` 机制在类型系统上**强制约束**编写业务层的开发人员处理“空值/不存在”的边界情况，从源头上消灭了游戏运行时最难以排查的 `NullReferenceException`。

---

## 🎨 灵魂扩展设计：Monad 链式 API 与数据擦除

除了扎实的底层底座，框架在上层业务编写的流水线体验与自由度上，展现了极高的技术审美。

### 1. `Attribute<T>` 声明式单子链式计算（Monadic Chaining）

通过 `Extensions.Attribute.cs`，框架为只读结构体注入了函数式编程的单子（Monad）流式调用能力。它允许将原本冗长、多分支的判空与边界限制代码，重构为极致优雅的“计算流水线”：

```csharp
// 零 GC 消耗、空值安全传播的流式数值计算
int finalAtk = attributes.Get<string, int>("BaseAtk") // 获取原始属性包装
    .Add(buffAtk)                                    // 链式加算 Buff
    .Subtract(debuffAtk)                             // 链式减算 Debuff
    .Clamp(0, 9999)                                  // 强类型边界裁剪
    .Coalesce(0);                                    // 终结操作：若全链条无值则安全降级为 0

```

* **空值安全传播**：如果链条的源头属性不存在，整个链条不会崩溃，`None` 状态会安全地向后传递，直至终结操作降级。
* **极致性能**：整条长长的链式操作完全在栈上创建新的 struct，**产生的 GC 分配为绝对的 0**。

### 2. `ISkillInfoSearcher` 的数据擦除与“防御性留白”

这是全框架最具灵气的设计。`ISkillInfoSearcher` 作为一个**纯粹的空标记接口**，代表了高超的依赖倒置（Dependency Inversion）艺术：

* **破局解法**：底层框架只负责运送这个凭证（如包裹在 `BuffContext` 与 `EffectContext` 中），它不需要知道凭证里有什么，从而实现了**与具体 RPG 业务数据的 100% 解耦**。
* **静态扩展桥接**：具体的游戏项目只需让策划的配置表结构实现该接口，并编写专属于该项目的**强类型静态扩展方法**：

```csharp
// 业务层自定义的扩展桥接
public static class SkillConfigExtensions
{
    public static float GetDamageParam(this ISkillInfoSearcher searcher, int level)
    {
        // 只有业务层知道 ISkillInfoSearcher 的真实数据结构
        if (searcher is MyProjectSkillConfig config)
        {
            return config.BaseDamage + level * config.GrowthRate;
        }
        return 0f;
    }
}

// 具体的 Buff 业务层使用体验：
// 既保持了底层的 100% 通用，又享受到了 100% 的强类型 IDE 代码提示
float damage = context.SkillInfoSearcher.GetDamageParam(currentLevel);

```

---

## 🎭 声明式契约与池化策略

### 1. 基于接口的能力剪裁（Interface-Based Capabilities）

`IBuff` 被设计为高度纯净的行为接口，通过 `INoExtraContextBuff`（不需要自定义上下文）、`INoGlobalBroadcastBuff`（不需要全局广播）以及 `IStackableBuff`（可堆叠能力）等标记接口动态裁剪行为。

* 底层通过 `buff.IsNoGlobalBroadcast()` 等扩展方法进行极速的编译期类型匹配，**免去了在基类中维护大量 `bool` 变量的内存开销**，组件表现极度轻量化。

### 2. `IResettable` 天然支持对象池模式

框架内的所有核心容器类（`AttributeCollection`、`TypedContext`、`BuffComponent`）均显式实现了 `IResettable` 接口。

* **池化契约**：内部的 `Reset()` 实现了极其干净的逆向清理，确保组件在复用时**绝不会发生“上一个宿主的残留数据污染下一个宿主”的幽灵 Bug**。
* **策略隔离**：底层的职责是保证自己“能够被安全地重置”。至于上层业务是选择跑完压测就扔，还是使用 `Stack<T>` 做缓存池，完全由业务层根据项目规模自主掌控。

---

## 🧱 业务层工程落地指引

由于该框架偏向高性能底层代码驱动，为了降低团队其他成员的学习门槛，建议在引入项目时，在业务层包裹以下两层“糖衣”：

### 1. 业务强类型降维封装（Extension Wrapper）

避免让业务层频繁手写双泛型的 `Get<TKey, TValue>`，建议基于项目具体的属性枚举编写专属快捷扩展：

```csharp
public enum RoleAttr { Hp, MaxHp, Atk, Def }

public static class AtkCollectionExtensions
{
    public static int GetHp(this IReadOnlyAttributeCollection col) => col.GetValueOrDefault<RoleAttr, int>(RoleAttr.Hp);
    public static void SetHp(this IAttributeCollection col, int value) => col.Set<RoleAttr, int>(RoleAttr.Hp, value);
}

```

### 2. 可视化配表桥接器（Unity Inspector Bridge）

针对 Unity 无法原生序列化泛型字典的硬伤，建议编写一个 MonoBehavior 组件作为配置转换桥接器，在 `Awake` 时将可视化配置遍历注入到底层集合中：

```csharp
public class AttributeConfigBridge : MonoBehaviour
{
    [System.Serializable]
    public struct AttrPair { public RoleAttr attr; public int value; }
    public List<AttrPair> initialAttributes;

    public void InitTo(IAttributeCollection runtimeCollection)
    {
        foreach(var pair in initialAttributes)
        {
            runtimeCollection.Set<RoleAttr, int>(pair.attr, pair.value);
        }
    }
}

```

---

## 📝 总结

本框架是一套**骨架极其扎实、对内存与性能追求到极致**的组件化战斗数值底座。它将复杂而易错的强类型安全存储、防爆指令队列与池化重置契约在底层完美闭环，同时将多变的业务与配置查询以最高的优雅度留白给上层。是一套极具商业项目基石价值的优秀架构。