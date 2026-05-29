using System;
using System.Linq;
using Xunit;

namespace EasyProgress.Core.Tests
{
    public class RealtimeCompositeTests
    {
        private readonly ICompositionRule<double> _rule = WeightedAverageRule.Create();

        [Fact]
        public void AddChild_ShouldRecalcProgress()
        {
            var composite = new RealtimeComposite(_rule);
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChild(leaf1);
            composite.AddChild(leaf2);
            leaf1.Report(0.5);
            leaf2.Report(0.5);
            Assert.Equal(0.5, composite.Progress);
        }

        [Fact]
        public void RemoveChild_ShouldRecalcProgress()
        {
            var composite = new RealtimeComposite(_rule);
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChild(leaf1);
            composite.AddChild(leaf2);
            leaf1.Report(1);
            leaf2.Report(0);
            composite.RemoveChild(leaf2);
            Assert.Equal(1, composite.Progress);
        }

        [Fact]
        public void ChildProgressChange_ShouldTriggerRecalcAndEvent()
        {
            var composite = new RealtimeComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            int eventCount = 0;
            composite.OnProgressChanged += (_, _) => eventCount++;
            leaf.Report(0.3);
            Assert.Equal(0.3, composite.Progress);
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void SetRule_ShouldRecalcWithNewRule()
        {
            var composite = new RealtimeComposite(_rule);
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChild(leaf1);
            composite.AddChild(leaf2);
            leaf1.Report(1);
            leaf2.Report(0);
            var maxRule = MaxRule.Create();
            composite.SetRule(maxRule);
            Assert.Equal(1, composite.Progress);
        }

        [Fact]
        public void Progress_ShouldReturnLatestCachedValue()
        {
            var composite = new RealtimeComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            leaf.Report(0.2);
            Assert.Equal(0.2, composite.Progress);
        }

        [Fact]
        public void MultipleChildren_ShouldUseEqualWeight()
        {
            var composite = new RealtimeComposite(_rule);
            var leaf1 = new DefaultLeaf();
            var leaf2 = new DefaultLeaf();
            composite.AddChild(leaf1);
            composite.AddChild(leaf2);
            leaf1.Report(1);
            leaf2.Report(0);
            Assert.Equal(0.5, composite.Progress);
        }

        [Fact]
        public void EmptyComposite_ProgressShouldBeZero()
        {
            var composite = new RealtimeComposite(_rule);
            Assert.Equal(0, composite.Progress);
        }

        [Fact]
        public void Reset_ShouldClearChildrenAndSetDefaultRule()
        {
            var composite = new RealtimeComposite(_rule);
            var leaf = new DefaultLeaf();
            composite.AddChild(leaf);
            composite.SetRule(MaxRule.Create());
            composite.Reset();
            Assert.Empty(composite.Children);
            Assert.IsType<WeightedAverageRule>(composite.Rule);
            Assert.Equal(0, composite.Progress);
        }
    }
}