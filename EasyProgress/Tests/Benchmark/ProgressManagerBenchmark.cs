using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class ProgressManagerBenchmark
    {
        private DefaultProgressManager<double> manager;
        private ICompositionRule<double> rule = WeightedAverageRule.Create();

        [GlobalSetup]
        public void Setup() => manager = DefaultProgressManager.CreateDefault();

        [Benchmark]
        public IProgressLeaf<double> AcquireLeaf() => manager.AcquireLeaf();

        [Benchmark]
        public IProgressComposite<double> AcquireComposite() => manager.AcquireComposite(rule);
    }
}