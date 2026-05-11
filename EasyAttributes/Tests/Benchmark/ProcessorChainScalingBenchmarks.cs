using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EasyAttributes.Core;
using System.Reflection;

namespace EasyAttributes.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net70, warmupCount: 3, iterationCount: 7)]
    public class ProcessorChainScalingBenchmarks
    {
        private IExecutor _executor0;
        private IExecutor _executor5;
        private IExecutor _executor10;
        private MethodInfo _method;
        private object _target;
        private object[] _args;
        private TestLogAttribute _attr;

        [GlobalSetup]
        public void Setup()
        {
            _target = new TestService();
            _method = typeof(TestService).GetMethod(nameof(TestService.DoWork))!;
            _args = new object[] { 1, "s" };
            _attr = new TestLogAttribute();

            // 0 处理器（仅 Attribute 但注册表无映射）
            _executor0 = DefaultExecutorBuilder.Create().Build();

            // 5 个相同处理器
            var builder5 = DefaultExecutorBuilder.Create();
            builder5.UseProcessor<TestLogAttribute, DummyProcessor1>();
            builder5.UseProcessor<TestLogAttribute, DummyProcessor2>();
            builder5.UseProcessor<TestLogAttribute, DummyProcessor3>();
            builder5.UseProcessor<TestLogAttribute, DummyProcessor4>();
            builder5.UseProcessor<TestLogAttribute, DummyProcessor5>();
            _executor5 = builder5.Build();

            // 10 个相同处理器
            var builder10 = DefaultExecutorBuilder.Create();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor1>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor2>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor3>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor4>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor5>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor6>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor7>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor8>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor9>();
            builder10.UseProcessor<TestLogAttribute, DummyProcessor10>();
            _executor10 = builder10.Build();
        }

        [Benchmark(Baseline = true)]
        public IProcessorHandle Chain_0_Processors()
        {
            var ctx = ContextFactory.CreateMethodContext(_attr, _method, _target, _args);
            return _executor0.Execute(ctx);
        }

        [Benchmark]
        public IProcessorHandle Chain_5_Processors()
        {
            var ctx = ContextFactory.CreateMethodContext(_attr, _method, _target, _args);
            return _executor5.Execute(ctx);
        }

        [Benchmark]
        public IProcessorHandle Chain_10_Processors()
        {
            var ctx = ContextFactory.CreateMethodContext(_attr, _method, _target, _args);
            return _executor10.Execute(ctx);
        }
    }
}
