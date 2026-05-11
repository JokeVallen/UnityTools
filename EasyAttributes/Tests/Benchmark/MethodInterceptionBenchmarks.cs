using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EasyAttributes.Core;
using System.Reflection;

namespace EasyAttributes.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net70, warmupCount: 5, iterationCount: 10)]
    public class MethodInterceptionBenchmarks
    {
        private IExecutor _executorTransient;
        private IExecutor _executorSingleton;
        private IExecutor _executorTransientWithFeatures;
        private MethodInfo _method;
        private object _target;
        private object[] _args;
        private TestLogAttribute _attr;

        [GlobalSetup]
        public void Setup()
        {
            _target = new TestService();
            _method = typeof(TestService).GetMethod(nameof(TestService.DoWork))!;
            _args = new object[] { 42, "benchmark" };
            _attr = new TestLogAttribute { Level = "Info" };

            // 构造不同配置的执行器
            var builderTransient = DefaultExecutorBuilder.Create()
                .UseProcessor<TestLogAttribute, LogProcessor>()
                .UseProcessor<TestLogAttribute, AnotherLogProcessor>();
            _executorTransient = builderTransient.Build();

            var builderSingleton = DefaultExecutorBuilder.Create()
                .UseProcessor<TestLogAttribute, LogProcessor>()
                .UseProcessor<TestLogAttribute, AnotherLogProcessor>()
                .UseFactory(SingletonProcessorFactory.Default);
            _executorSingleton = builderSingleton.Build();

            var builderFeature = DefaultExecutorBuilder.Create()
                .UseProcessor<TestLogAttribute, LogProcessor>()
                .UseFeature<IFakeLogger>(new FakeLogger());
            _executorTransientWithFeatures = builderFeature.Build();

            // 预热工厂
            TransientProcessorFactory.Default.Create(typeof(LogProcessor));
        }

        [Benchmark(Baseline = true)]
        public int DirectCall()
        {
            return ((TestService)_target).DoWork(42, "benchmark");
        }

        [Benchmark]
        public int ReflectionCall()
        {
            return (int)_method.Invoke(_target, _args);
        }

        [Benchmark]
        public IProcessorHandle Intercept_2Processors_Transient()
        {
            var ctx = ContextFactory.CreateMethodContext(_attr, _method, _target, _args);
            return _executorTransient.Execute(ctx);
        }

        [Benchmark]
        public IProcessorHandle Intercept_2Processors_Singleton()
        {
            var ctx = ContextFactory.CreateMethodContext(_attr, _method, _target, _args);
            return _executorSingleton.Execute(ctx);
        }

        [Benchmark]
        public IProcessorHandle Intercept_WithGlobalFeature()
        {
            var ctx = ContextFactory.CreateMethodContext(_attr, _method, _target, _args);
            return _executorTransientWithFeatures.Execute(ctx);
        }
    }
}
