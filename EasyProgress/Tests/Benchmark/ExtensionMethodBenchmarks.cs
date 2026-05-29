using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    public class ExtensionMethodBenchmarks
    {
        private ILeafManager<double> _leafManager;
        private ICompositeManager<double> _compositeManager;

        [GlobalSetup]
        public void Setup()
        {
            _leafManager = Progress.GetLeafManager<double>();
            _compositeManager = Progress.GetCompositeManager<double>();
        }

        [Benchmark]
        public void ReleaseLeafChildren()
        {
            var composite = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            var leaf = _leafManager.AcquireLeaf();
            composite.AddChild(leaf);
            composite.ReleaseLeafChildren(_leafManager);
            _compositeManager.ReleaseComposite(composite);
        }

        [Benchmark]
        public void ReleaseTree()
        {
            var composite = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            var leaf = _leafManager.AcquireLeaf();
            var childComp = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            childComp.AddChild(leaf);
            composite.AddChild(childComp);
            composite.ReleaseTree(_leafManager, _compositeManager);
            _compositeManager.ReleaseComposite(composite);
        }

        [Benchmark]
        public void BeginProgress_Using()
        {
            var composite = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            using (var scope = composite.BeginProgress(_leafManager))
            {
                scope.Report(0.5);
            }
            _compositeManager.ReleaseComposite(composite);
        }

        [Benchmark]
        public void BeginComposite_Using()
        {
            var parent = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            using (var scope = parent.BeginComposite(WeightedAverageRule.Create(), _leafManager, _compositeManager))
            {
                var leaf = _leafManager.AcquireLeaf();
                scope.Composite.AddChild(leaf);
                leaf.Complete();
            }
            _compositeManager.ReleaseComposite(parent);
        }

        [Benchmark]
        public void RunWithProgress()
        {
            var composite = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            composite.RunWithProgress(leaf => leaf.Complete(), _leafManager);
            _compositeManager.ReleaseComposite(composite);
        }

        [Benchmark]
        public async Task RunWithProgressAsync()
        {
            var composite = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            await composite.RunWithProgressAsync(async leaf =>
            {
                await Task.Yield();
                leaf.Complete();
            }, _leafManager);
            _compositeManager.ReleaseComposite(composite);
        }

        [Benchmark]
        public void AddChildren_Plain()
        {
            var composite = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            var leaf1 = _leafManager.AcquireLeaf();
            var leaf2 = _leafManager.AcquireLeaf();
            composite.AddChildren(leaf1, leaf2);
            // 释放子节点（AddChildren 不会自动释放，需手动）
            _leafManager.ReleaseLeaf(leaf1);
            _leafManager.ReleaseLeaf(leaf2);
            _compositeManager.ReleaseComposite(composite);
        }

        [Benchmark]
        public void AddChildren_Weighted()
        {
            var composite = _compositeManager.AcquireComposite(WeightedAverageRule.Create());
            // 注意：加权组合节点需使用 WeightedRealtimeComposite
            var weightedComposite = new WeightedRealtimeComposite(WeightedAverageRule.Create()); // 池化版本也可以从管理器获取，但需重载
            var leaf1 = _leafManager.AcquireLeaf();
            var leaf2 = _leafManager.AcquireLeaf();
            weightedComposite.AddChildren((leaf1, 0.3f), (leaf2, 0.7f));
            _leafManager.ReleaseLeaf(leaf1);
            _leafManager.ReleaseLeaf(leaf2);
            // 如果 weightedComposite 也是池化的，这里需释放
            // 为简化，保持原样
        }
    }
}