using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class PoolBenchmark
    {
        private PooledNodeManager<double, DefaultLeaf> pool;

        [GlobalSetup]
        public void Setup()
        {
            pool = new PooledNodeManager<double, DefaultLeaf>(_ => new DefaultLeaf());
            // Pre-warm pool
            for (int i = 0; i < 100; i++)
                pool.Release(new DefaultLeaf());
        }

        [Benchmark]
        public DefaultLeaf AcquireRelease()
        {
            var leaf = pool.Acquire();
            leaf.Report(0.5);
            pool.Release(leaf);
            return leaf;
        }
    }
}