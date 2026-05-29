using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class WeightedRealtimeCompositeBenchmark
    {
        private WeightedRealtimeComposite composite;
        private DefaultLeaf[] leaves;

        [Params(1, 10, 100)]
        public int ChildCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var rule = WeightedAverageRule.Create();
            composite = new WeightedRealtimeComposite(rule);
            leaves = new DefaultLeaf[ChildCount];
            float weight = 1f / ChildCount;
            for (int i = 0; i < ChildCount; i++)
            {
                leaves[i] = new DefaultLeaf();
                composite.AddChild(leaves[i], weight);
            }
        }

        [Benchmark]
        public void UpdateOneChild() => leaves[0].Report(0.5);
    }
}