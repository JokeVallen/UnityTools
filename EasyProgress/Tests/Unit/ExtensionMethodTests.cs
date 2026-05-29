using EasyProgress.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyProgress.UnitTests
{
    public class ExtensionMethodTests : IDisposable
    {
        private readonly SpyLeafManager _leafManager;
        private readonly SpyCompositeManager _compositeManager;

        public ExtensionMethodTests()
        {
            _leafManager = new SpyLeafManager();
            _compositeManager = new SpyCompositeManager();
        }

        public void Dispose() { }

        // -----------------------------------------
        // ReleaseLeafChildren
        // -----------------------------------------
        [Fact]
        public void ReleaseLeafChildren_ReleasesDirectLeafChildrenAndRemovesThem()
        {
            var composite = new RealtimeComposite(WeightedAverageRule.Create());
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            var childComposite = new RealtimeComposite(WeightedAverageRule.Create());
            composite.AddChild(leaf1);
            composite.AddChild(leaf2);
            composite.AddChild(childComposite);

            composite.ReleaseLeafChildren(_leafManager);

            // 验证叶子节点被释放
            Assert.Equal(2, _leafManager.ReleasedCount);
            Assert.Contains(leaf1, _leafManager.ReleasedLeaves);
            Assert.Contains(leaf2, _leafManager.ReleasedLeaves);
            // 验证叶子节点从父节点中移除
            Assert.DoesNotContain(leaf1, composite.Children);
            Assert.DoesNotContain(leaf2, composite.Children);
            // 子组合节点未被释放且未被移除
            Assert.Contains(childComposite, composite.Children);
            Assert.DoesNotContain(childComposite, _compositeManager.ReleasedComposites);
        }

        // -----------------------------------------
        // ReleaseTree
        // -----------------------------------------
        [Fact]
        public void ReleaseTree_ReleasesAllDescendantsAndRemovesThem()
        {
            var root = new RealtimeComposite(WeightedAverageRule.Create());
            var childComp = new RealtimeComposite(WeightedAverageRule.Create());
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            root.AddChild(childComp);
            childComp.AddChild(leaf1);
            childComp.AddChild(leaf2);

            root.ReleaseTree(_leafManager, _compositeManager);

            // 叶子节点被释放
            Assert.Equal(2, _leafManager.ReleasedCount);
            Assert.Contains(leaf1, _leafManager.ReleasedLeaves);
            Assert.Contains(leaf2, _leafManager.ReleasedLeaves);
            // 子组合节点被释放
            Assert.Equal(1, _compositeManager.ReleasedComposites.Count);
            Assert.Contains(childComp, _compositeManager.ReleasedComposites);
            // 所有子节点从 root 中移除（包括 childComp）
            Assert.Empty(root.Children);
        }

        // -----------------------------------------
        // BeginProgress (LeafScope)
        // -----------------------------------------
        [Fact]
        public void BeginProgress_AddsLeafAndAutomaticallyRemovesOnDispose()
        {
            var composite = new RealtimeComposite(WeightedAverageRule.Create());
            IProgressLeaf<double> leafInside = null;

            using (var scope = composite.BeginProgress(_leafManager))
            {
                leafInside = scope.Leaf;
                Assert.Contains(leafInside, composite.Children);
                scope.Report(0.5);
                Assert.Equal(0.5, leafInside.Progress);
            }

            Assert.DoesNotContain(leafInside, composite.Children);
            Assert.Contains(leafInside, _leafManager.ReleasedLeaves);
        }

        // -----------------------------------------
        // BeginComposite (CompositeScope)
        // -----------------------------------------
        [Fact]
        public void BeginComposite_AddsCompositeAndOnDisposeReleasesWholeSubtree()
        {
            var parent = new RealtimeComposite(WeightedAverageRule.Create());
            IProgressComposite<double> tempComp = null;
            var leaf = new DefaultLeaf();

            using (var scope = parent.BeginComposite(WeightedAverageRule.Create(), _leafManager, _compositeManager))
            {
                tempComp = scope.Composite;
                Assert.Contains(tempComp, parent.Children);
                tempComp.AddChild(leaf);
            }

            Assert.DoesNotContain(tempComp, parent.Children);
            // leaf 应被释放（因为 ReleaseTree 递归）
            Assert.Contains(leaf, _leafManager.ReleasedLeaves);
            // tempComp 应被释放
            Assert.Contains(tempComp, _compositeManager.ReleasedComposites);
        }

        // -----------------------------------------
        // RunWithProgress / RunWithProgressAsync
        // -----------------------------------------
        [Fact]
        public void RunWithProgress_ExecutesWorkAndCleansUp()
        {
            var composite = new RealtimeComposite(WeightedAverageRule.Create());
            IProgressLeaf<double> leafInside = null;

            composite.RunWithProgress(leaf =>
            {
                leafInside = leaf;
                leaf.Report(0.8);
            }, _leafManager);

            Assert.DoesNotContain(leafInside, composite.Children);
            Assert.Contains(leafInside, _leafManager.ReleasedLeaves);
        }

        [Fact]
        public async Task RunWithProgressAsync_ExecutesWorkAndCleansUp()
        {
            var composite = new RealtimeComposite(WeightedAverageRule.Create());
            IProgressLeaf<double> leafInside = null;

            await composite.RunWithProgressAsync(async leaf =>
            {
                leafInside = leaf;
                await Task.Delay(1);
                leaf.Report(0.9);
            }, _leafManager);

            Assert.DoesNotContain(leafInside, composite.Children);
            Assert.Contains(leafInside, _leafManager.ReleasedLeaves);
        }

        // -----------------------------------------
        // AddChildren (普通和加权)
        // -----------------------------------------
        [Fact]
        public void AddChildren_AddsMultiplePlainNodes()
        {
            var composite = new RealtimeComposite(WeightedAverageRule.Create());
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChildren(leaf1, leaf2);
            Assert.Equal(2, composite.Children.Count);
            Assert.Contains(leaf1, composite.Children);
            Assert.Contains(leaf2, composite.Children);
        }

        [Fact]
        public void AddChildren_Weighted_AddsWithWeights()
        {
            var composite = new WeightedRealtimeComposite(WeightedAverageRule.Create());
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChildren((leaf1, 0.3f), (leaf2, 0.7f));
            Assert.Equal(2, composite.Children.Count);
            Assert.Equal(0.3f, composite.GetWeight(leaf1));
            Assert.Equal(0.7f, composite.GetWeight(leaf2));
        }
    }
}
