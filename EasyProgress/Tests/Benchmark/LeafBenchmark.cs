using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class LeafBenchmark
    {
        private DefaultLeaf leaf;

        [GlobalSetup]
        public void Setup() => leaf = new DefaultLeaf();

        [Benchmark]
        public void Report() => leaf.Report(0.5);

        [Benchmark]
        public void Complete() => leaf.Complete();
    }
}