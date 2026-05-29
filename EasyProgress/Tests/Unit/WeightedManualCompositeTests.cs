using Xunit;

namespace EasyProgress.Core.Tests
{
    public class WeightedManualCompositeTests
    {
        private readonly ICompositionRule<double> _rule = WeightedAverageRule.Create();

        [Fact]
        public void AddChild_WithWeight_ShouldMarkDirtyAndNotRecalc()
        {
            var composite = new WeightedManualComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf, 0.6f);
            leaf.Report(0.5);
            Assert.Equal(0, composite.Progress);
            composite.Refresh();
            TestHelpers.AssertApproxEqual(0.5, composite.Progress);
        }

        [Fact]
        public void SetWeight_ShouldMarkDirty()
        {
            var composite = new WeightedManualComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf, 0.5f);
            leaf.Report(1);
            composite.Refresh();
            Assert.Equal(1, composite.Progress);
            composite.SetWeight(leaf, 0.2f);
            composite.Refresh();
            TestHelpers.AssertApproxEqual(1, composite.Progress); // weight doesn't affect single child
        }

        [Fact]
        public void Refresh_ShouldRecalcUsingWeights()
        {
            var composite = new WeightedManualComposite(_rule);
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChild(leaf1, 0.3f);
            composite.AddChild(leaf2, 0.7f);
            leaf1.Report(1);
            leaf2.Report(0);
            composite.Refresh();
            TestHelpers.AssertApproxEqual(0.3, composite.Progress);
        }

        [Fact]
        public void Reset_ShouldClearWeights()
        {
            var composite = new WeightedManualComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf, 0.8f);
            composite.Reset();
            Assert.Empty(composite.Children);
            // Re-add and default weight should be 1
            composite.AddChild(leaf);
            Assert.Equal(1f, composite.GetWeight(leaf));
        }
    }
}