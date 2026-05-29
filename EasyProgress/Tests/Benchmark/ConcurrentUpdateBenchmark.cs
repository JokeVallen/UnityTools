using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class ConcurrentUpdateBenchmark
    {
        private RealtimeComposite composite;
        private DefaultLeaf[] leaves;

        [Params(1, 4, 16)]
        public int ConcurrencyLevel { get; set; }

        [Params(1_000, 5_000)]
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
        public void ConcurrentUpdates()
        {
            var tasks = new Task[ConcurrencyLevel];
            for (int t = 0; t < ConcurrencyLevel; t++)
            {
                int idx = t % leaves.Length;
                tasks[t] = Task.Run(() => leaves[idx].Report(0.5));
            }
            Task.WaitAll(tasks);
        }
    }
}
