> 内容由 AI 根据核心代码生成，已通过人工审核。

# CodeGenerator Framework – API 文档

本文档列出所有公开（`public`）API 的签名与简要说明，并提供基础使用示例。

## 结构一览

``` text
┌─────────────────────────────────────────────────────────┐
│         CodeGenerator Framework 三层架构                 │
├─────────────────────────────────────────────────────────┤
│                    中介者管理层                          │
│        (IGeneratorMediator / IGeneratorMediatorAsync)   │
│  - Rescan() : 扫描并注册生成器                           │
│  - RunAll() : 执行所有生成器                             │
│  - Run<T>() : 执行指定生成器                             │
├─────────────────────────────────────────────────────────┤
│  模板读取 ──────> 内容生成 ──────> 结果写入               │
│ ┌──────────┐  ┌──────────┐  ┌─────────┐                 │
│ │ITemplate │→ │IGenerator│→ │IWriter  │                 │
│ │Provider  │  │          │  │         │                 │
│ └──────────┘  └──────────┘  └─────────┘                 │
│ (同步/异步)    (同步/异步)    (同步/异步)                 │
└─────────────────────────────────────────────────────────┘
```

## 公共 API 一览

### 接口

#### `IGenerator`（标记接口）
```csharp
public interface IGenerator { }
```
所有生成器的基础标记接口，不包含任何成员，仅用于类型约束。

#### `IGenerator<TTemplate, TContent> : IGenerator`
```csharp
public interface IGenerator<TTemplate, TContent> : IGenerator
{
    TContent Generate(TTemplate template);
}
```
- **描述**：同步代码生成器。接收模板实例，返回生成内容。
- **类型参数**：`TTemplate` – 模板数据类型；`TContent` – 生成结果数据类型。

#### `IGeneratorAsync<TTemplate, TContent> : IGenerator`
```csharp
public interface IGeneratorAsync<TTemplate, TContent> : IGenerator
{
    Task<TContent> GenerateAsync(TTemplate template, CancellationToken cancellationToken = default);
}
```
- **描述**：异步代码生成器。支持取消操作，适用于耗时生成场景。

#### `ITemplateProvider<TTemplate>`
```csharp
public interface ITemplateProvider<TTemplate>
{
    TTemplate GetTemplate(string templatePath);
}
```
- **描述**：同步模板提供者，根据模板路径返回模板内容。

#### `ITemplateProviderAsync<TTemplate>`
```csharp
public interface ITemplateProviderAsync<TTemplate>
{
    Task<TTemplate> GetTemplateAsync(string templatePath, CancellationToken cancellationToken = default);
}
```
- **描述**：异步模板提供者。

#### `IWriter<TContent>`
```csharp
public interface IWriter<TContent>
{
    void Write(string outputPath, TContent content);
}
```
- **描述**：同步写入器，将内容写入指定输出路径。

#### `IWriterAsync<TContent>`
```csharp
public interface IWriterAsync<TContent>
{
    Task WriteAsync(string outputPath, TContent content, CancellationToken cancellationToken = default);
}
```
- **描述**：异步写入器。

#### `IGeneratorMediator<TGenerator> where TGenerator : IGenerator`
```csharp
public interface IGeneratorMediator<TGenerator>
{
    void Rescan();
    void Clear();
    void RunAll();
    void Run<T>() where T : TGenerator;
}
```
- **描述**：同步中介者，负责扫描、清理和执行生成器。

#### `IGeneratorMediatorAsync<TGenerator> where TGenerator : IGenerator`
```csharp
public interface IGeneratorMediatorAsync<TGenerator>
{
    Task RescanAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task RunAllAsync(CancellationToken cancellationToken = default);
    Task RunAsync<T>(CancellationToken cancellationToken = default) where T : TGenerator;
}
```
- **描述**：异步中介者，所有操作返回 `Task` 并支持取消令牌。

---

### 类

#### `GeneratorConfigAttribute : Attribute`
```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class GeneratorConfigAttribute : Attribute
{
    public string TemplatePath { get; }
    public string OutputPath { get; }

    public GeneratorConfigAttribute(string templatePath, string outputPath);
}
```
- **描述**：用于标记生成器类，声明其使用的模板路径与输出路径。必须作用于直接实现 `IGenerator` 的类上，且不可继承、不可重复。
- **构造函数参数**：`templatePath` – 模板路径；`outputPath` – 输出路径。

#### `BaseGeneratorMediator<TGenerator> : IGeneratorMediator<TGenerator>, IEnumerable<...>, IReadOnlyDictionary<...>`
```csharp
public abstract class BaseGeneratorMediator<TGenerator> : IGeneratorMediator<TGenerator>, 
    IEnumerable<KeyValuePair<Type, BaseGeneratorMediator<TGenerator>.MetaData>>,
    IReadOnlyDictionary<Type, BaseGeneratorMediator<TGenerator>.MetaData>
    where TGenerator : class, IGenerator
```
- **描述**：生成器中介者抽象基类。提供反射扫描、索引访问和释放资源的通用逻辑，具体扫描策略由子类实现。

**公开成员**  
- `MetaData this[Type key]` – 通过生成器类型获取元数据。
- `IEnumerable<Type> Keys` – 所有已注册的生成器类型。
- `IEnumerable<MetaData> Values` – 所有元数据。
- `int Count` – 已注册数量。
- `bool ContainsKey(Type key)` – 判断是否已注册某类型。
- `bool TryGetValue(Type key, out MetaData value)` – 尝试获取元数据。
- `IEnumerator<KeyValuePair<Type, MetaData>> GetEnumerator()` – 遍历所有条目。
- `virtual void Clear()` – 清理所有生成器，并释放实现了 `IDisposable` 的实例。
- `abstract void Rescan()` – 子类必须实现，用于扫描程序集并填充 `generators` 字典。
- `abstract void Run<T>() where T : TGenerator` – 执行指定生成器的完整流程（读模板→生成→写入）。
- `abstract void RunAll()` – 执行所有已注册生成器。

**嵌套结构体**  
`public readonly struct MetaData`  
- `string TemplatePath` – 模板路径。
- `string OutputPath` – 输出路径。
- `TGenerator Generator` – 生成器实例。
- 构造函数：`MetaData(string templatePath, string outputPath, TGenerator generator)`

**受保护成员**  
- `Dictionary<Type, MetaData> generators` – 存储生成器的字典，子类在 `Rescan` 中填充。
- `static void DisposeInstance(object instance)` – 如果实例是 `IDisposable` 则调用 `Dispose()`。

---

## 使用示例

以下示例演示如何实现一个将模板字符串转为大写并写入文件的简单生成器，以及如何通过自定义中介者运行。

### 1. 定义生成器
```csharp
using CodeGenerator;
using System.Threading;
using System.Threading.Tasks;

[GeneratorConfig("Templates/MyTemplate.txt", "Output/MyOutput.txt")]
public class UpperCaseGenerator : IGenerator<string, string>
{
    public string Generate(string template)
    {
        return template.ToUpper();
    }
}

// 也可实现异步版本（同时实现两个接口不是必须的，这里仅为演示）
public class UpperCaseGeneratorAsync : IGeneratorAsync<string, string>
{
    public Task<string> GenerateAsync(string template, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(template.ToUpper());
    }
}
```

### 2. 实现中介者（简化扫描逻辑）
```csharp
public class MyMediator : BaseGeneratorMediator<IGenerator<string, string>>
{
    public override void Rescan()
    {
        // 示例：手动添加，实际项目中可通过反射查找带 [GeneratorConfig] 的类
        var generator = new UpperCaseGenerator();
        generators[typeof(UpperCaseGenerator)] = new MetaData(
            "Templates/MyTemplate.txt",
            "Output/MyOutput.txt",
            generator
        );
    }

    public override void Run<T>()
    {
        if (generators.TryGetValue(typeof(T), out var meta))
        {
            // 实际项目应注入模板提供者和写入器，这里仅示意
            string template = "hello world"; // 模拟读取模板
            string result = meta.Generator.Generate(template);
            System.IO.File.WriteAllText(meta.OutputPath, result);
        }
    }

    public override void RunAll()
    {
        foreach (var key in generators.Keys)
        {
            // 使用反射调用 Run<T> 的对应方法，此简化示例略
        }
    }
}
```

### 3. 使用中介者
```csharp
var mediator = new MyMediator();
mediator.Rescan();
mediator.Run<UpperCaseGenerator>();
// 此时 Output/MyOutput.txt 内容变为 "HELLO WORLD"
```

### 4. 异步版本示例（假设有异步提供者和写入器）
```csharp
public class AsyncMediator : BaseGeneratorMediator<IGenerator<string, string>>
{
    private ITemplateProviderAsync<string> templateProvider;
    private IWriterAsync<string> writer;

    // 注入依赖...
    public override async Task Run<T>(CancellationToken cancellationToken)
    {
        var meta = generators[typeof(T)];
        string template = await templateProvider.GetTemplateAsync(meta.TemplatePath, cancellationToken);
        
        // 如果生成器实现了 IGeneratorAsync，则调用异步版本
        string result;
        if (meta.Generator is IGeneratorAsync<string, string> asyncGen)
            result = await asyncGen.GenerateAsync(template, cancellationToken);
        else
            result = ((IGenerator<string, string>)meta.Generator).Generate(template);

        await writer.WriteAsync(meta.OutputPath, result, cancellationToken);
    }
    // ... 其他抽象方法实现
}
```

> **注意**：完整可运行的项目还应提供 `ITemplateProvider` 和 `IWriter` 的具体实现（如文件读取器、文件写入器），并结合反射自动扫描带有 `[GeneratorConfig]` 特性的生成器类。上述简例仅为说明接口的协作方式。