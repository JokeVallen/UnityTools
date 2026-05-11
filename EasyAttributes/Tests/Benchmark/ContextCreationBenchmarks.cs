using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EasyAttributes.Core;
using System.Reflection;

namespace EasyAttributes.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net70, warmupCount: 5, iterationCount: 10)]
    public class ContextCreationBenchmarks
    {
        private MethodInfo _method;
        private PropertyInfo _property;
        private TestLogAttribute _attr;
        private object _target;

        [GlobalSetup]
        public void Setup()
        {
            _target = new TestService();
            _method = typeof(TestService).GetMethod(nameof(TestService.DoWork))!;
            _property = typeof(TestService).GetProperty(nameof(TestService.Name))!;
            _attr = new TestLogAttribute();
        }

        [Benchmark]
        public IMethodContext CreateMethodContext()
        {
            return ContextFactory.CreateMethodContext(_attr, _method, _target, null);
        }

        [Benchmark]
        public IPropertyContext CreatePropertyContext()
        {
            return ContextFactory.CreatePropertyContext(_attr, _property, PropertyAccessor.Get, _target, null);
        }
    }
}
