# CodeGenerator Framework – API 文档（v1.0.1-beta）

> 内容由 AI 根据核心代码生成，已通过人工审核。


## 一、概述

**CodeGenerator Framework** 是一个轻量级、高度可扩展的代码生成抽象框架。它通过定义清晰的接口分层，将**模板读取 → 内容生成 → 结果写入**三个环节解耦，并提供了同步/异步、带上下文/不带上下文、中介者批量调度等多种编程模型，适用于构建各类代码生成工具链。

### 版本信息

- **当前版本**：1.0.1-beta
- **目标框架**：.NET Standard 2.0
- **C# 兼容性**：7.0 及以上


## 二、架构概览

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     CodeGenerator Framework 三层架构                    │
├─────────────────────────────────────────────────────────────────────────┤
│                          中介者管理层                                   │
│         (IMediator / IAsyncMediator / IMediatorWithContext)            │
│    ┌─────────────────────────────────────────────────────────────┐      │
│    │  Rescan() → Run<T>() → RunAll()                            │      │
│    └─────────────────────────────────────────────────────────────┘      │
├─────────────────────────────────────────────────────────────────────────┤
│  模板提供 ──────────────> 内容生成 ──────────────> 结果写入              │
│  ┌──────────────┐    ┌──────────────────┐    ┌──────────────┐           │
│  │ ITemplate    │ → │ IGenerator /     │ → │ IWriter      │           │
│  │ Provider     │    │ IGeneratorWith  │    │              │           │
│  │ (同步/异步)   │    │ Context         │    │ (同步/异步)   │           │
│  └──────────────┘    │ (同步/异步)      │    └──────────────┘           │
│                       └──────────────────┘                              │
├─────────────────────────────────────────────────────────────────────────┤
│                          基础能力层                                     │
│    ┌────────────────────┐    ┌────────────────────┐                    │
│    │ TypedContext       │    │ Optional<T>        │                    │
│    │ (强类型键值存储)    │    │ (可选值包装器)     │                    │
│    └────────────────────┘    └────────────────────┘                    │
└─────────────────────────────────────────────────────────────────────────┘
```


## 三、公共 API 参考

### 3.1 基础能力

#### `Optional<T>` 结构体

可选值包装器，用于表示可能存在或不存在的值，避免使用 `null` 带来的歧义。

```csharp
public readonly struct Optional<T>
```

| 成员 | 说明 |
|------|------|
| `bool HasValue { get; }` | 是否包含有效值 |
| `T Value { get; }` | 获取值，无值时抛出 `InvalidOperationException` |
| `static Optional<T> None { get; }` | 空值单例 |
| `static implicit operator Optional<T>(T value)` | 从 `T` 隐式转换 |
| `static explicit operator T(Optional<T> optional)` | 显式转换为 `T` |
| `bool Equals(Optional<T> other)` | 值相等比较 |
| `bool operator == / !=` | 相等/不等运算符 |

**使用示例**：
```csharp
Optional<int> result = GetValue();
if (result.HasValue)
    Console.WriteLine($"值是: {result.Value}");
else
    Console.WriteLine("无值");

// 隐式转换
Optional<string> name = "Unity";
```

---

#### `ITypedContext` 接口

强类型上下文，提供类型安全的键值存储能力，键和值可以分别是任意类型。

```csharp
public interface ITypedContext
{
    void Set<TKey, TValue>(TKey key, TValue value);
    Optional<TValue> Get<TKey, TValue>(TKey key);
    bool Remove<TKey, TValue>(TKey key);
    bool ContainsKey<TKey, TValue>(TKey key);
    void Clear();
}
```

| 方法 | 说明 |
|------|------|
| `Set<TKey, TValue>(key, value)` | 存储键值对，键和值类型均可任意指定 |
| `Get<TKey, TValue>(key)` | 获取值，返回 `Optional<TValue>` |
| `Remove<TKey, TValue>(key)` | 移除指定键值对 |
| `ContainsKey<TKey, TValue>(key)` | 判断是否包含指定键 |
| `Clear()` | 清空所有存储 |

> **设计说明**：每个 `(TKey, TValue)` 组合独立存储，互不干扰。例如 `Set<string, int>("count", 10)` 和 `Set<string, string>("count", "ten")` 可以共存。

---

#### `TypedContext` 类

`ITypedContext` 的默认实现。

```csharp
public sealed class TypedContext : ITypedContext, IResettable
```

| 成员 | 说明 |
|------|------|
| `void Set<TKey, TValue>(TKey key, TValue value)` | 存储值 |
| `Optional<TValue> Get<TKey, TValue>(TKey key)` | 获取值 |
| `bool Remove<TKey, TValue>(TKey key)` | 移除值 |
| `bool ContainsKey<TKey, TValue>(TKey key)` | 判断包含 |
| `void Clear()` | 清空所有存储（保留存储容器） |
| `void Reset()` | 重置整个上下文（释放存储容器） |

**使用示例**：
```csharp
var context = new TypedContext();
context.Set("playerName", "Hero");
context.Set("level", 42);

var name = context.Get<string, int>("level"); // Optional<int> 包含 42
```

---

#### `IResettable` 接口

可重置能力扩展接口，用于需要完整重置状态的类型。

```csharp
public interface IResettable
{
    void Reset();
}
```


### 3.2 生成器接口

#### `IGenerator`（标记接口）

所有生成器的基础标记接口，不包含任何成员，仅用于类型约束和中介者泛型约束。

```csharp
public interface IGenerator { }
```

---

#### `ISyncGenerator<TTemplate, TContent>`

同步代码生成器接口。

```csharp
public interface ISyncGenerator<TTemplate, TContent> : IGenerator
{
    TContent Generate(TTemplate template);
}
```

| 参数 | 说明 |
|------|------|
| `TTemplate` | 模板内容类型（如 `string`、`byte[]`） |
| `TContent` | 生成结果类型（如 `string`、`byte[]`） |

---

#### `ISyncGeneratorWithContext<TTemplate, TContent>`

带上下文的同步代码生成器接口。

```csharp
public interface ISyncGeneratorWithContext<TTemplate, TContent> : IGenerator
{
    TContent Generate(TTemplate template, ITypedContext context);
}
```

> 适用于生成逻辑需要访问外部状态或配置的场景。

---

#### `IAsyncGenerator<TTemplate, TContent>`

异步代码生成器接口，支持取消操作。

```csharp
public interface IAsyncGenerator<TTemplate, TContent> : IGenerator
{
    Task<TContent> GenerateAsync(TTemplate template, CancellationToken cancellationToken = default);
}
```

---

#### `IAsyncGeneratorWithContext<TTemplate, TContent>`

带上下文的异步代码生成器接口。

```csharp
public interface IAsyncGeneratorWithContext<TTemplate, TContent> : IGenerator
{
    Task<TContent> GenerateAsync(TTemplate template, ITypedContext context, CancellationToken cancellationToken = default);
}
```


### 3.3 模板提供者接口

#### `ITemplateProvider<TTemplate>`

同步模板提供者。

```csharp
public interface ITemplateProvider<TTemplate>
{
    TTemplate GetTemplate(string templatePath);
}
```

---

#### `IAsyncTemplateProvider<TTemplate>`

异步模板提供者。

```csharp
public interface IAsyncTemplateProvider<TTemplate>
{
    Task<TTemplate> GetTemplateAsync(string templatePath, CancellationToken cancellationToken = default);
}
```


### 3.4 写入器接口

#### `IWriter<TContent>`

同步写入器。

```csharp
public interface IWriter<TContent>
{
    void Write(string outputPath, TContent content);
}
```

---

#### `IAsyncWriter<TContent>`

异步写入器。

```csharp
public interface IAsyncWriter<TContent>
{
    Task WriteAsync(string outputPath, TContent content, CancellationToken cancellationToken = default);
}
```


### 3.5 中介者接口

#### `IMediator<TGenerator>`

同步中介者接口。

```csharp
public interface IMediator<TGenerator> where TGenerator : IGenerator
{
    void Rescan();   // 扫描并注册生成器
    void Clear();    // 清理所有生成器
    void RunAll();   // 执行所有生成器
    void Run<T>() where T : TGenerator;  // 执行指定生成器
}
```

---

#### `IMediatorWithContext<TGenerator>`

带上下文的同步中介者接口。

```csharp
public interface IMediatorWithContext<TGenerator> where TGenerator : IGenerator
{
    void RunAll(ITypedContext context);
    void Run<T>(ITypedContext context) where T : TGenerator;
}
```

---

#### `IAsyncMediator<TGenerator>`

异步中介者接口。

```csharp
public interface IAsyncMediator<TGenerator> where TGenerator : IGenerator
{
    Task RescanAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task RunAllAsync(CancellationToken cancellationToken = default);
    Task RunAsync<T>(CancellationToken cancellationToken = default) where T : TGenerator;
}
```

---

#### `IAsyncMediatorWithContext<TGenerator>`

带上下文的异步中介者接口。

```csharp
public interface IAsyncMediatorWithContext<TGenerator> where TGenerator : IGenerator
{
    Task RunAllAsync(ITypedContext context, CancellationToken cancellationToken = default);
    Task RunAsync<T>(ITypedContext context, CancellationToken cancellationToken = default) where T : TGenerator;
}
```


### 3.6 抽象基类

#### `BaseMediator<TGenerator>`

生成器中介者抽象基类，提供反射扫描、索引访问和资源释放的通用逻辑。

```csharp
public abstract class BaseMediator<TGenerator> : IMediator<TGenerator>,
    IEnumerable<KeyValuePair<Type, BaseMediator<TGenerator>.MetaData>>,
    IReadOnlyDictionary<Type, BaseMediator<TGenerator>.MetaData>
    where TGenerator : class, IGenerator
```

**公开成员**：

| 成员 | 说明 |
|------|------|
| `MetaData this[Type key]` | 通过生成器类型获取元数据 |
| `IEnumerable<Type> Keys` | 所有已注册的生成器类型 |
| `IEnumerable<MetaData> Values` | 所有元数据 |
| `int Count` | 已注册数量 |
| `bool ContainsKey(Type key)` | 判断是否已注册某类型 |
| `bool TryGetValue(Type key, out MetaData value)` | 尝试获取元数据 |
| `IEnumerator<...> GetEnumerator()` | 遍历所有条目 |
| `virtual void Clear()` | 清理所有生成器，并释放 `IDisposable` 实例 |
| `abstract void Rescan()` | **子类必须实现**，扫描并填充 `generators` 字典 |
| `abstract void Run<T>() where T : TGenerator` | **子类必须实现**，执行指定生成器 |
| `abstract void RunAll()` | **子类必须实现**，执行所有生成器 |

**嵌套结构体 `MetaData`**：

| 成员 | 说明 |
|------|------|
| `string TemplatePath` | 模板路径 |
| `string OutputPath` | 输出路径 |
| `TGenerator Generator` | 生成器实例 |

**受保护成员**：

| 成员 | 说明 |
|------|------|
| `Dictionary<Type, MetaData> generators` | 存储生成器的字典，子类在 `Rescan` 中填充 |
| `static void DisposeInstance(object instance)` | 若实例为 `IDisposable` 则调用 `Dispose()` |


### 3.7 特性（Attributes）

#### `GeneratorAttribute`

代码生成器标记特性，用于自动扫描标识。

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GeneratorAttribute : Attribute { }
```

#### `GeneratorConfigAttribute`

代码生成器配置特性，声明模板路径与输出路径。

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class GeneratorConfigAttribute : Attribute
{
    public string TemplatePath { get; }
    public string OutputPath { get; }

    public GeneratorConfigAttribute(string templatePath, string outputPath);
}
```


## 四、使用示例

### 4.1 定义生成器

```csharp
using CodeGenerator;

// 同步生成器：将模板转换为大写
[GeneratorConfig("Templates/Input.txt", "Output/Result.txt")]
[Generator]
public class UpperCaseGenerator : ISyncGenerator<string, string>
{
    public string Generate(string template)
    {
        return template.ToUpper();
    }
}

// 带上下文的生成器：使用上下文中的前缀
[GeneratorConfig("Templates/Message.txt", "Output/Message.txt")]
[Generator]
public class PrefixedGenerator : ISyncGeneratorWithContext<string, string>
{
    public string Generate(string template, ITypedContext context)
    {
        var prefix = context.Get<string, string>("prefix").ValueOrDefault("[PREFIX]");
        return $"{prefix}: {template}";
    }
}

// 异步生成器
[GeneratorConfig("Templates/Large.txt", "Output/Large.txt")]
[Generator]
public class AsyncGenerator : IAsyncGenerator<string, string>
{
    public async Task<string> GenerateAsync(string template, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);  // 模拟异步处理
        return template.ToUpper();
    }
}
```

### 4.2 实现自定义中介者

```csharp
public class MyMediator : BaseMediator<IGenerator>
{
    public override void Rescan()
    {
        // 实际项目应使用反射扫描带有 [Generator] + [GeneratorConfig] 的类
        // 这里手动添加作为示例
        generators[typeof(UpperCaseGenerator)] = new MetaData(
            "Templates/Input.txt",
            "Output/Result.txt",
            new UpperCaseGenerator()
        );
    }

    public override void Run<T>()
    {
        if (generators.TryGetValue(typeof(T), out var meta))
        {
            var generator = (ISyncGenerator<string, string>)meta.Generator;
            string template = "hello world"; // 实际应通过 ITemplateProvider 读取
            string result = generator.Generate(template);
            System.IO.File.WriteAllText(meta.OutputPath, result);
        }
    }

    public override void RunAll()
    {
        foreach (var key in generators.Keys)
        {
            // 通过反射调用 Run<T>()
            typeof(MyMediator).GetMethod(nameof(Run))
                .MakeGenericMethod(key)
                .Invoke(this, null);
        }
    }
}
```

### 4.3 使用中介者

```csharp
var mediator = new MyMediator();
mediator.Rescan();
mediator.Run<UpperCaseGenerator>();
// Output/Result.txt 内容变为 "HELLO WORLD"

// 遍历已注册的生成器
foreach (var kvp in mediator)
{
    Console.WriteLine($"生成器: {kvp.Key.Name}, 输出: {kvp.Value.OutputPath}");
}
```

### 4.4 使用 TypedContext

```csharp
var context = new TypedContext();
context.Set("prefix", "[CUSTOM]");
context.Set("author", "CoroutineRunner Team");
context.Set("version", 1);

var prefixed = context.Get<string, string>("prefix"); // Optional<string> 包含 "[CUSTOM]"
var author = context.Get<string, string>("author");   // Optional<string> 包含 "CoroutineRunner Team"
var version = context.Get<string, int>("version");    // Optional<int> 包含 1
```