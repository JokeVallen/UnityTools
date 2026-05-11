using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EasyAttributes.Core;
using System.Reflection;

namespace EasyAttributes.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net70, warmupCount: 5, iterationCount: 10)]
    public class AsyncMethodInterceptionBenchmarks
    {
        private IExecutorAsync _executor;
        private MethodInfo _asyncMethod;
        private object _target;
        private object[] _args;
        private TestLogAttribute _attr;

        [GlobalSetup]
        public void Setup()
        {
            _target = new TestService();
            _asyncMethod = typeof(TestService).GetMethod(nameof(TestService.DoWorkAsync))!;
            _args = new object[] { 42, "async" };
            _attr = new TestLogAttribute { Level = "Debug" };

            _executor = DefaultExecutorBuilder.Create()
                .UseProcessor<TestLogAttribute, AsyncLogProcessor>()
                .UseProcessor<TestLogAttribute, SyncLogProcessor>()
                .BuildAsync();
        }

        [Benchmark(Baseline = true)]
        public async Task<int> DirectCallAsync()
        {
            return await ((TestService)_target).DoWorkAsync(42, "async");
        }

        [Benchmark]
        public async Task<IProcessorHandle> InterceptAsync_WithMixedProcessors()
        {
            var ctx = ContextFactory.CreateMethodContext(_attr, _asyncMethod, _target, _args);
            return await _executor.ExecuteAsync(ctx);
        }
    }
}
