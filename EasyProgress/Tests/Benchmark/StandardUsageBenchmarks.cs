using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net70, iterationCount: 10, warmupCount: 2)]
    [MemoryDiagnoser]
    public class StandardUsageBenchmarks
    {
        private ICompositionRule<double> _rule;
        private ILeafManager<double> _leafManager;
        private ICompositeManager<double> _compositeManager;

        [GlobalSetup]
        public void Setup()
        {
            _rule = WeightedAverageRule.Create();
            _leafManager = Progress.GetLeafManager<double>();
            _compositeManager = Progress.GetCompositeManager<double>();
        }

        // 场景1：叶子节点池化获取与报告
        [Benchmark]
        public void LeafPool_AcquireReportRelease()
        {
            var leaf = _leafManager.AcquireLeaf();
            leaf.Report(0.5);
            _leafManager.ReleaseLeaf(leaf);
        }

        // 场景2：临时叶子作用域（池化组合节点）
        [Benchmark]
        public void LeafScope_OnPooledComposite()
        {
            var composite = _compositeManager.AcquireComposite(_rule);
            using (var scope = composite.BeginProgress(_leafManager))
            {
                scope.Report(0.5);
            }
            _compositeManager.ReleaseComposite(composite);
        }

        // 场景3：临时组合节点作用域（自动释放子树）
        [Benchmark]
        public void CompositeScope_OnPooledParent()
        {
            var parent = _compositeManager.AcquireComposite(_rule);
            using (var scope = parent.BeginComposite(_rule, _leafManager, _compositeManager))
            {
                var temp = scope.Composite;
                var leaf = _leafManager.AcquireLeaf();
                temp.AddChild(leaf);
                leaf.Complete(); // 模拟任务完成
            }
            _compositeManager.ReleaseComposite(parent);
        }

        // 场景4：释放整个树（ReleaseTree）
        [Benchmark]
        public void ReleaseTree_FromPooledRoot()
        {
            var root = _compositeManager.AcquireComposite(_rule);
            var child = _compositeManager.AcquireComposite(_rule);
            var leaf = _leafManager.AcquireLeaf();
            root.AddChild(child);
            child.AddChild(leaf);
            root.ReleaseTree(_leafManager, _compositeManager);
            _compositeManager.ReleaseComposite(root);
        }

        // 额外：长期存活组合节点复用临时叶子（不重新获取组合节点）
        private IProgressComposite<double> _reusedComposite;

        [IterationSetup]
        public void ReusedCompositeSetup()
        {
            _reusedComposite = _compositeManager.AcquireComposite(_rule);
        }

        [IterationCleanup]
        public void ReusedCompositeCleanup()
        {
            _compositeManager.ReleaseComposite(_reusedComposite);
        }

        [Benchmark]
        public void ReusedComposite_LeafScope()
        {
            using (var scope = _reusedComposite.BeginProgress(_leafManager))
            {
                scope.Report(0.5);
            }
        }
    }
}