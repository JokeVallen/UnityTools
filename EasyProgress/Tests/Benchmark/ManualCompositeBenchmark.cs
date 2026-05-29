using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class ManualCompositeBenchmark
    {
        private ManualComposite composite;
        private DefaultLeaf[] leaves;

        [Params(1, 10, 100)]
        public int ChildCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var rule = WeightedAverageRule.Create();
            composite = new ManualComposite(rule);
            leaves = new DefaultLeaf[ChildCount];
            for (int i = 0; i < ChildCount; i++)
            {
                leaves[i] = new DefaultLeaf();
                composite.AddChild(leaves[i]);
            }
        }

        [Benchmark]
        public void UpdateOneChild_NoRefresh() => leaves[0].Report(0.5);

        [Benchmark]
        public void UpdateOneChild_ThenRefresh()
        {
            leaves[0].Report(0.5);
            composite.Refresh();
        }
    }
}