using System;
using System.Threading;
using System.Threading.Tasks;
using CodeGenerator;

// 同步生成器，实现简单的字符串重复转换，用于模拟不同长度的工作
public class RepeatGenerator : IGenerator<string, string>
{
    // 将模板重复一定次数，可配置
    public int RepeatCount { get; set; } = 1;

    public string Generate(string template)
    {
        string result = string.Empty;
        for (int i = 0; i < RepeatCount; i++)
        {
            result += template; // 故意使用字符串拼接以产生分配
        }
        return result;
    }
}

// 异步生成器
public class AsyncRepeatGenerator : IGeneratorAsync<string, string>
{
    public int RepeatCount { get; set; } = 1;

    public async Task<string> GenerateAsync(string template, CancellationToken cancellationToken = default)
    {
        // 模拟轻度异步操作
        await Task.Yield();
        string result = string.Empty;
        for (int i = 0; i < RepeatCount; i++)
        {
            result += template;
        }
        return result;
    }
}

// 模板提供者：简单返回字符串，但通过参数控制大小
public class SizedTemplateProvider : ITemplateProvider<string>
{
    public int TemplateSize { get; set; } = 100;

    public string GetTemplate(string templatePath)
    {
        return new string('A', TemplateSize);
    }
}

// 写入器：写入到丢弃流（/dev/null），模拟最小开销
public class NullWriter : IWriter<string>
{
    public void Write(string outputPath, string content) { }
}