using Xunit;

namespace EasyProgress.Core.Tests
{
    public class DefaultProgressManagerTests
    {
        [Fact]
        public void CreateDefault_ShouldReturnManagerWithValidFactories()
        {
            var manager = DefaultProgressManager.CreateDefault();
            Assert.NotNull(manager);
        }

        [Fact]
        public void AcquireLeaf_ReturnsPooledLeaf()
        {
            var manager = DefaultProgressManager.CreateDefault();
            var leaf1 = manager.AcquireLeaf();
            var leaf2 = manager.AcquireLeaf();
            Assert.NotSame(leaf1, leaf2);
            manager.ReleaseLeaf(leaf1);
            var leaf3 = manager.AcquireLeaf();
            Assert.Same(leaf1, leaf3);
        }

        [Fact]
        public void AcquireComposite_WithRule_SetsRule()
        {
            var manager = DefaultProgressManager.CreateDefault();
            var rule = MaxRule.Create();
            var composite = manager.AcquireComposite(rule);
            Assert.Same(rule, composite.Rule);
        }

        [Fact]
        public void Release_ResetsAndReturnsToPool()
        {
            var manager = DefaultProgressManager.CreateDefault();
            var leaf = manager.AcquireLeaf();
            leaf.Report(0.8);
            manager.ReleaseLeaf(leaf);
            var leafAgain = manager.AcquireLeaf();
            Assert.Equal(0, leafAgain.Progress);
            Assert.Same(leaf, leafAgain);
        }

        [Fact]
        public void CustomFactories_WorkCorrectly()
        {
            var leafFactoryCalled = false;
            var compositeFactoryCalled = false;
            var manager = new DefaultProgressManager<double>(
                leafFactory: () => { leafFactoryCalled = true; return new DefaultLeaf(); },
                compositeFactory: rule => { compositeFactoryCalled = true; return new WeightedRealtimeComposite(rule); }
            );
            manager.AcquireLeaf();
            manager.AcquireComposite(WeightedAverageRule.Create());
            Assert.True(leafFactoryCalled);
            Assert.True(compositeFactoryCalled);
        }
    }
}