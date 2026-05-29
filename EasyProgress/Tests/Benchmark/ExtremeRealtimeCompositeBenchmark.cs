using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class ExtremeRealtimeCompositeBenchmark
    {
        private RealtimeComposite composite;
        private DefaultLeaf[] leaves;

        [Params(1_000, 10_000)]
        public int ChildCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var rule = WeightedAverageRule.Create();
            composite = new RealtimeComposite(rule);
            leaves = new DefaultLeaf[ChildCount];
            for (int i = 0; i < ChildCount; i++)
            {
                leaves[i] = new DefaultLeaf();
                composite.AddChild(leaves[i]);
            }
        }

        [Benchmark]
        public void UpdateOneLeaf() => leaves[0].Report(0.5);

        [Benchmark]
        public void UpdateAllLeaves()
        {
            for (int i = 0; i < leaves.Length; i++)
                leaves[i].Report(0.5);
        }
    }
}
