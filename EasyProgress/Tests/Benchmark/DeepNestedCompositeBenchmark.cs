using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class DeepNestedCompositeBenchmark
    {
        private const int TotalNodes = 5_000;
        private IProgressNode<double> root;
        private DefaultLeaf deepestLeaf;

        [Params(5, 10, 20)]
        public int Depth { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var rule = WeightedAverageRule.Create();
            var leaves = new List<DefaultLeaf>();
            for (int i = 0; i < TotalNodes; i++)
                leaves.Add(new DefaultLeaf());
            root = BuildBalancedTree(leaves, 0, leaves.Count, Depth, rule);
            deepestLeaf = leaves[^1]; // 最后一个叶子作为更新目标
        }

        private IProgressNode<double> BuildBalancedTree(
            List<DefaultLeaf> leaves, int start, int count, int depth, ICompositionRule<double> rule)
        {
            if (depth == 0 || count <= 1)
                return leaves[start];
            var composite = new RealtimeComposite(rule);
            int childrenPerNode = Math.Max(1, count / 2);
            int remaining = count;
            int pos = start;
            while (remaining > 0)
            {
                int part = Math.Min(childrenPerNode, remaining);
                var child = BuildBalancedTree(leaves, pos, part, depth - 1, rule);
                composite.AddChild(child);
                pos += part;
                remaining -= part;
            }
            return composite;
        }

        [Benchmark]
        public void UpdateDeepestLeaf() => deepestLeaf.Report(0.5);
    }
}
