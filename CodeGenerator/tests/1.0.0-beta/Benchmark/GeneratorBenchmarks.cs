using NUnit.Framework;
using Unity.PerformanceTesting;
using CodeGenerator;
using System;

namespace Tests.Benchmark
{
    public class GeneratorBenchmarks
    {
        private RepeatGenerator generator;
        private string smallTemplate;
        private string largeTemplate;

        [SetUp]
        public void SetUp()
        {
            generator = new RepeatGenerator();
            smallTemplate = "Hello";
            largeTemplate = new string('X', 1000);
        }

        [Test, Performance]
        public void Generate_SmallTemplate_Once()
        {
            Measure.Method(() => generator.Generate(smallTemplate))
                .WarmupCount(5)
                .MeasurementCount(20)
                .IterationsPerMeasurement(100)
                .GC()
                .Run();
        }

        [Test, Performance]
        public void Generate_LargeTemplate_Repeat10()
        {
            generator.RepeatCount = 10;
            Measure.Method(() => generator.Generate(largeTemplate))
                .WarmupCount(5)
                .MeasurementCount(20)
                .IterationsPerMeasurement(20)
                .GC()
                .Run();
        }

        [Test, Performance]
        public void Generate_StressTest_Repeat100()
        {
            generator.RepeatCount = 100;
            Measure.Method(() => generator.Generate(smallTemplate))
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(5)
                .GC()
                .Run();
        }
    }

    public class AsyncGeneratorBenchmarks
    {
        private AsyncRepeatGenerator asyncGen;
        private string template;

        [SetUp]
        public void SetUp()
        {
            asyncGen = new AsyncRepeatGenerator { RepeatCount = 10 };
            template = new string('A', 500);
        }

        [Test, Performance]
        public void GenerateAsync_WithoutCancellation()
        {
            Measure.Method(async () => await asyncGen.GenerateAsync(template))
                .WarmupCount(5)
                .MeasurementCount(20)
                .IterationsPerMeasurement(10)
                .GC()
                .Run();
        }
    }

    public class TemplateProviderBenchmarks
    {
        private SizedTemplateProvider provider;

        [SetUp]
        public void SetUp()
        {
            provider = new SizedTemplateProvider();
        }

        [Test, Performance]
        public void GetTemplate_SmallSize()
        {
            provider.TemplateSize = 100;
            Measure.Method(() => provider.GetTemplate("anyPath"))
                .WarmupCount(5)
                .MeasurementCount(20)
                .IterationsPerMeasurement(100)
                .GC()
                .Run();
        }

        [Test, Performance]
        public void GetTemplate_LargeSize()
        {
            provider.TemplateSize = 100000;
            Measure.Method(() => provider.GetTemplate("path"))
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(5)
                .GC()
                .Run();
        }
    }

    public class WriterBenchmarks
    {
        private NullWriter writer;
        private string content;

        [SetUp]
        public void SetUp()
        {
            writer = new NullWriter();
            content = new string('B', 10000);
        }

        [Test, Performance]
        public void Write_LargeContent()
        {
            Measure.Method(() => writer.Write("path", content))
                .WarmupCount(5)
                .MeasurementCount(20)
                .IterationsPerMeasurement(50)
                .GC()
                .Run();
        }
    }

    // 模拟一个简单的自定义中介者，用于测试 RunAll 管道性能
    public class MediatorPerformanceTests
    {
        private class MockMediator : BaseGeneratorMediator<IGenerator<string, string>>
        {
            public void ManualAdd(Type t, string templatePath, string outputPath, IGenerator<string, string> gen)
            {
                generators[t] = new MetaData(templatePath, outputPath, gen);
            }

            // 实际执行生成+写入的流水线，模拟 RunAll
            public void ExecuteAll()
            {
                foreach (var kvp in generators)
                {
                    var meta = kvp.Value;
                    var template = new string('T', 200); // 模拟模板提供者读取
                    var result = meta.Generator.Generate(template);
                    // 模拟写入
                    // (此处只为性能测量，不实际操作文件)
                }
            }

            public override void Rescan() { }
            public override void Run<T>() { }
            public override void RunAll() { ExecuteAll(); }
        }

        [Test, Performance]
        public void RunAll_FiveGenerators()
        {
            var mediator = new MockMediator();
            for (int i = 0; i < 5; i++)
            {
                var gen = new RepeatGenerator { RepeatCount = 3 };
                mediator.ManualAdd(typeof(RepeatGenerator), "t.txt", "o.txt", gen);
            }

            Measure.Method(() => mediator.ExecuteAll())
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(5)
                .GC()
                .Run();
        }

        [Test, Performance]
        public void RunAll_TwentyGenerators_Stress()
        {
            var mediator = new MockMediator();
            for (int i = 0; i < 20; i++)
            {
                var gen = new RepeatGenerator { RepeatCount = 2 };
                mediator.ManualAdd(typeof(RepeatGenerator), $"t{i}.txt", $"o{i}.txt", gen);
            }

            Measure.Method(() => mediator.ExecuteAll())
                .WarmupCount(2)
                .MeasurementCount(5)
                .IterationsPerMeasurement(2)
                .GC()
                .Run();
        }
    }
}