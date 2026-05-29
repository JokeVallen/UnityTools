using Xunit;

namespace EasyProgress.Core.Tests
{
    public class WeightedRealtimeCompositeTests
    {
        private readonly ICompositionRule<double> _rule = WeightedAverageRule.Create();

        [Fact]
        public void AddChild_WithWeight_ShouldApplyWeightInAverage()
        {
            var composite = new WeightedRealtimeComposite(_rule);
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChild(leaf1, 0.3f);
            composite.AddChild(leaf2, 0.7f);
            leaf1.Report(1);
            leaf2.Report(0);
            TestHelpers.AssertApproxEqual(0.3, composite.Progress);
        }

        [Fact]
        public void SetWeight_ShouldRecalc()
        {
            var composite = new WeightedRealtimeComposite(_rule);
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChild(leaf1, 0.5f);
            composite.AddChild(leaf2, 0.5f);
            leaf1.Report(1);
            leaf2.Report(0);
            composite.SetWeight(leaf1, 0.9f);
            composite.SetWeight(leaf2, 0.1f);
            TestHelpers.AssertApproxEqual(0.9, composite.Progress);
        }

        [Fact]
        public void GetWeight_DefaultWeightOne()
        {
            var composite = new WeightedRealtimeComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            Assert.Equal(1f, composite.GetWeight(leaf));
        }

        [Fact]
        public void RemoveChild_ShouldAlsoRemoveWeight()
        {
            var composite = new WeightedRealtimeComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf, 0.8f);
            composite.RemoveChild(leaf);
            Assert.Empty(composite.Children);
            // Accessing GetWeight on removed node throws ArgumentNullException? Not defined.
            // We just verify that composite works after removal.
            composite.AddChild(new DefaultLeaf(), 0.5f);
            Assert.Equal(0.5f, composite.GetWeight(composite.Children.FirstOrDefault()));
        }

        [Fact]
        public void WeightedAverage_ShouldCalculateCorrectly()
        {
            var composite = new WeightedRealtimeComposite(_rule);
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            var leaf3 = new DefaultLeaf();
            composite.AddChild(leaf1, 0.2f);
            composite.AddChild(leaf2, 0.3f);
            composite.AddChild(leaf3, 0.5f);
            leaf1.Report(1);
            leaf2.Report(0.5);
            leaf3.Report(0);
            var expected = (1 * 0.2 + 0.5 * 0.3) / (0.2 + 0.3 + 0.5);
            TestHelpers.AssertApproxEqual(expected, composite.Progress);
        }

        [Fact]
        public void ZeroWeightChild_ShouldBeIgnored()
        {
            var composite = new WeightedRealtimeComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf, 0f);
            leaf.Report(1);
            Assert.Equal(0, composite.Progress); // total weight 0 -> returns 0
        }
    }
}