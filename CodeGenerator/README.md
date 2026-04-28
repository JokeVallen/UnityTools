# 🔧 CodeGenerator 轻量级代码生成框架

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Unity Test Framework](https://img.shields.io/badge/Unity%20Test%20Framework-passing-brightgreen)]()  

一个基于 **中介者模式** 与 **特性标记** 的轻量级代码生成框架。通过清晰的职责划分（模板提供、生成、写入），你可以快速搭建可扩展、可维护的代码生成管线。支持同步与异步操作，适用于工具链开发、定制化代码生成等场景。

---

## ✨ 特性

- **职责分离**：模板读取、代码生成、结果写入各自独立，方便替换和扩展。
- **中介者托管**：通过 `GeneratorMediator` 自动扫描并管理所有标记的生成器。
- **特性驱动**：使用 `[GeneratorConfig]` 特性声明模板路径和输出路径，无需手动注册。
- **同步与异步支持**：提供完整的异步接口，支持 `CancellationToken` 取消操作。
- **泛型可定制**：可为不同模板和输出类型编写对应的生成器，框架不限制具体实现。
- **轻量零依赖**：仅依赖 .NET Standard 2.0，无额外第三方库。

---

## 📦 安装

### 注入源码

将整个源码文件夹复制到你的项目中，修改相应的命名空间（如有需要），即可直接使用。

### 使用 DLL

可将 `.dll` 和 `.xml` 直接导入项目进行使用。

---

## 🚀 快速开始

### 1. 定义你的生成器接口（可选）

```csharp
// 例如：将字符串模板转换为字符串代码
public interface IStringGenerator : IGenerator<string, string> { }
```

或者直接让你的生成器实现 `IGenerator<T1, T2>` 接口。

### 2. 实现生成器并标记特性

```csharp
[GeneratorConfig("Templates/MyClass.txt", "Output/MyClass.cs")]
public class MyClassGenerator : IStringGenerator
{
    public string Generate(string template)
    {
        // 用模板生成具体代码，这里仅作演示，你可以使用正则表达式高效搜索需要替换内容的标识
        return template.Replace("#NAME#", "MyClass");
    }
}
```

### 3. 实现模板提供者和写入器（可选）

```csharp
public class FileTemplateProvider : ITemplateProvider<string>
{
    public string GetTemplate(string templatePath)
        => File.ReadAllText(templatePath);
}

public class FileWriter : IWriter<string>
{
    public void Write(string outputPath, string content)
        => File.WriteAllText(outputPath, content);
}
```

模板提供者和写入器是解耦规范，并非必须实现的接口，当你存在多种模板读取方式或最终生成内容的写入途径时往往需要用到它们。

### 4. 创建中介者并运行

```csharp
public class MyMediator : BaseGeneratorMediator<IStringGenerator>
{
    private readonly ITemplateProvider<string> templateProvider = new FileTemplateProvider();
    private readonly IWriter<string> writer = new FileWriter();

    public override void Rescan()
    {
        Clear();
        // 使用反射扫描所有带 [GeneratorConfig] 的 IStringGenerator 实现
        var generatorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IStringGenerator).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var type in generatorTypes)
        {
            var attr = type.GetCustomAttribute<GeneratorConfigAttribute>();
            if (attr == null) continue;
            var generator = (IStringGenerator)Activator.CreateInstance(type);
            generators[type] = new MetaData(attr.TemplatePath, attr.OutputPath, generator);
        }
    }

    public override void Run<T>()
    {
        if (!generators.TryGetValue(typeof(T), out var meta))
            throw new InvalidOperationException("Generator not found.");
        
        string template = templateProvider.GetTemplate(meta.TemplatePath);
        string result = meta.Generator.Generate(template);
        writer.Write(meta.OutputPath, result);
    }

    public override void RunAll()
    {
        foreach (var type in generators.Keys)
        {
            RunByType(type);
        }
    }

    private void RunByType(Type type)
    {
        var method = typeof(MyMediator).GetMethod(nameof(Run), Type.EmptyTypes);
        var genericMethod = method.MakeGenericMethod(type);
        genericMethod.Invoke(this, null);
    }
}
```

中介者负责调度各个组件，避免组件之间显式引用，进一步解耦各个组件的协同工作。

### 5. 运行生成器

```csharp
var mediator = new MyMediator();
mediator.Rescan();
mediator.RunAll();
```

> 你也可以实现异步版本，只需使用 `ITemplateProviderAsync<TTemplate>` 和 `IWriterAsync<TContent>`，并覆写 `RunAsync<T>` 等方法。

---

## 📚 API 文档

### `IGenerator` / `IGenerator<TTemplate, TContent>` / `IGeneratorAsync<TTemplate, TContent>`

生成器核心接口，负责将模板内容 `TTemplate` 转换为输出内容 `TContent`。

| 方法 | 说明 |
|------|------|
| `TContent Generate(TTemplate template)` | 同步生成 |
| `Task<TContent> GenerateAsync(TTemplate, CancellationToken)` | 异步生成（带取消支持） |

### `GeneratorConfigAttribute`

标记一个生成器类，并指定其使用的模板路径和输出路径。

| 属性 | 说明 |
|------|------|
| `TemplatePath` | 模板文件路径 |
| `OutputPath` | 输出文件路径 |

### `ITemplateProvider<TTemplate>` / `ITemplateProviderAsync<TTemplate>`

从指定路径读取模板内容。

| 方法 | 说明 |
|------|------|
| `TTemplate GetTemplate(string path)` | 同步获取模板 |
| `Task<TTemplate> GetTemplateAsync(string, CancellationToken)` | 异步获取模板 |

### `IWriter<TContent>` / `IWriterAsync<TContent>`

将生成的内容写入输出路径。

| 方法 | 说明 |
|------|------|
| `void Write(string outputPath, TContent content)` | 同步写入 |
| `Task WriteAsync(string, TContent, CancellationToken)` | 异步写入 |

### `BaseGeneratorMediator<TGenerator>`

中介者基类，实现生成器存储、扫描和执行的公共逻辑。你可以继承此类并实现抽象方法，也可以自定义中介者类。

| 成员 | 说明 |
|------|------|
| `generators` 字典 | 存储所有已扫描生成器的元数据 |
| `MetaData` 结构体 | 包含 `TemplatePath`, `OutputPath`, `Generator` |
| `Rescan()` | 扫描并注册所有生成器（需实现） |
| `Clear()` | 清理并释放所有生成器（基类已实现 `IDisposable` 释放） |
| `Run<T>()` | 运行指定类型的生成器（需实现） |
| `RunAll()` | 运行所有已注册的生成器（需实现） |
| `DisposeInstance(object)` | 安全释放 `IDisposable` 实例 |

### 异步中介者接口 `IGeneratorMediatorAsync<TGenerator>`

| 方法 | 说明 |
|------|------|
| `RescanAsync(CancellationToken)` | 异步扫描生成器 |
| `ClearAsync(CancellationToken)` | 异步清理 |
| `RunAllAsync(CancellationToken)` | 异步运行所有生成器 |
| `RunAsync<T>(CancellationToken)` | 异步运行指定生成器 |

---

## 🧱 依赖关系图

```
┌──────────────┐
│  Generator   │ (你的实现)
└──────┬───────┘
       │ 使用
┌──────▼──────┐      ┌─────────────────┐      ┌──────────┐
│  Mediator   │ ───► │ITemplateProvider│ ───► │ IWriter  │
└─────────────┘      └─────────────────┘      └──────────┘
          扫描并管理所有生成器
```

---

## 📄 许可证

本项目采用 MIT 许可证。详情请参见 [LICENSE](LICENSE) 文件。

---

## 🤝 贡献

欢迎提交 Issue 或 Pull Request！如果你有好的建议或发现了 bug，请随时反馈。