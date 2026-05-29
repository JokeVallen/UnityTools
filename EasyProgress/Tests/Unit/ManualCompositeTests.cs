using Xunit;

namespace EasyProgress.Core.Tests
{
    public class ManualCompositeTests
    {
        private readonly ICompositionRule<double> _rule = WeightedAverageRule.Create();

        [Fact]
        public void AddChild_ShouldMarkDirtyButNotRecalc()
        {
            var composite = new ManualComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            leaf.Report(0.5);
            Assert.Equal(0, composite.Progress); // not refreshed yet
            composite.Refresh();
            Assert.Equal(0.5, composite.Progress);
        }

        [Fact]
        public void RemoveChild_ShouldMarkDirty()
        {
            var composite = new ManualComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            composite.Refresh();
            leaf.Report(0.5);
            composite.RemoveChild(leaf);
            composite.Refresh();
            Assert.Equal(0, composite.Progress);
        }

        [Fact]
        public void Refresh_ShouldRecalcAndTriggerEventOnce()
        {
            var composite = new ManualComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            int eventCount = 0;
            composite.OnProgressChanged += (_, _) => eventCount++;
            leaf.Report(0.3);
            composite.Refresh();
            Assert.Equal(0.3, composite.Progress);
            Assert.Equal(1, eventCount);
            // second refresh with no change should not trigger event
            composite.Refresh();
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void SetRule_ShouldMarkDirty()
        {
            var composite = new ManualComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            leaf.Report(1);
            composite.Refresh();
            Assert.Equal(1, composite.Progress);
            composite.SetRule(MinRule.Create());
            composite.Refresh();
            Assert.Equal(1, composite.Progress); // min of [1] is 1
        }

        [Fact]
        public void Reset_ShouldClearChildrenAndDirtyFlag()
        {
            var composite = new ManualComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            composite.Refresh();
            composite.Reset();
            Assert.Empty(composite.Children);
            Assert.Equal(0, composite.Progress);
            composite.Refresh(); // should not throw
        }
    }
}