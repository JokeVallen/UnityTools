using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(RunStrategy.Monitoring, iterationCount: 10, warmupCount: 2, invocationCount: 1_000_000)]
    [MemoryDiagnoser]
    public class LongRunningBenchmark
    {
        private DefaultLeaf leaf;

        [GlobalSetup]
        public void Setup() => leaf = new DefaultLeaf();

        [Benchmark]
        public void ReportOneMillionTimes() => leaf.Report(0.5);
    }
}
