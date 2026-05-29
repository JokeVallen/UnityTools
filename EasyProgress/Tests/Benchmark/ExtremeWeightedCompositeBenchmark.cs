using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class ExtremeWeightedCompositeBenchmark
    {
        private WeightedRealtimeComposite composite;
        private DefaultLeaf heavyLeaf;
        private DefaultLeaf[] lightLeaves;

        [Params(500, 1_000)]
        public int LightLeafCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var rule = WeightedAverageRule.Create();
            composite = new WeightedRealtimeComposite(rule);
            heavyLeaf = new DefaultLeaf();
            composite.AddChild(heavyLeaf, 0.9f);
            lightLeaves = new DefaultLeaf[LightLeafCount];
            float lightWeight = 0.1f / LightLeafCount;
            for (int i = 0; i < LightLeafCount; i++)
            {
                lightLeaves[i] = new DefaultLeaf();
                composite.AddChild(lightLeaves[i], lightWeight);
            }
        }

        [Benchmark]
        public void UpdateHeavyLeaf() => heavyLeaf.Report(1.0);

        [Benchmark]
        public void UpdateAllLightLeaves()
        {
            for (int i = 0; i < lightLeaves.Length; i++)
                lightLeaves[i].Report(1.0);
        }
    }
}
