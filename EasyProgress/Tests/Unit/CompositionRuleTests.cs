using System;
using System.Collections.Generic;
using Xunit;

namespace EasyProgress.Core.Tests
{
    public class CompositionRuleTests
    {
        private readonly List<IProgressNode<double>> _empty = new List<IProgressNode<double>>();
        private readonly Func<IProgressNode<double>, float> _getWeight = _ => 1f;

        [Fact]
        public void WeightedAverageRule_ShouldComputeAverage()
        {
            var rule = WeightedAverageRule.Create();
            var nodes = new List<IProgressNode<double>>
            {
                new DummyNode(0.2),
                new DummyNode(0.8)
            };
            var result = rule.Compute(nodes, _getWeight);
            Assert.Equal(0.5, result);
        }

        [Fact]
        public void WeightedAverageRule_WithWeights()
        {
            var rule = WeightedAverageRule.Create();
            var nodes = new List<IProgressNode<double>>
            {
                new DummyNode(0.2),
                new DummyNode(0.8)
            };
            Func<IProgressNode<double>, float> getWeight = n => n == nodes[0] ? 0.3f : 0.7f;
            var result = rule.Compute(nodes, getWeight);
            var expected = (0.2 * 0.3 + 0.8 * 0.7) / (0.3 + 0.7);
            TestHelpers.AssertApproxEqual(expected, result);
        }

        [Fact]
        public void WeightedAverageRule_ZeroTotalWeightReturnsZero()
        {
            var rule = WeightedAverageRule.Create();
            var nodes = new List<IProgressNode<double>> { new DummyNode(0.5) };
            var result = rule.Compute(nodes, _ => 0f);
            Assert.Equal(0, result);
        }

        [Fact]
        public void WeightedAverageRule_EmptyListReturnsZero()
        {
            var rule = WeightedAverageRule.Create();
            var result = rule.Compute(_empty, _getWeight);
            Assert.Equal(0, result);
        }

        [Fact]
        public void SequentialRule_ShouldAccumulateSequentially()
        {
            var rule = SequentialRule.Create();
            var nodes = new List<IProgressNode<double>>
            {
                new DummyNode(1.0),
                new DummyNode(0.5)
            };
            Func<IProgressNode<double>, float> getWeight = n => n == nodes[0] ? 0.6f : 0.4f;
            var result = rule.Compute(nodes, getWeight);
            // first completed: accumulated 0.6, second in progress: 0.5*0.4 = 0.2, total 0.8
            TestHelpers.AssertApproxEqual(0.8, result);
        }

        [Fact]
        public void SequentialRule_NormalizesTotalWeightOverOne()
        {
            var rule = SequentialRule.Create();
            var nodes = new List<IProgressNode<double>>
            {
                new DummyNode(1.0),
                new DummyNode(1.0)
            };
            Func<IProgressNode<double>, float> getWeight = _ => 0.8f; // total 1.6 > 1
            var result = rule.Compute(nodes, getWeight);
            // normalized each weight = 0.8/1.6 = 0.5, accumulated = 0.5+0.5 = 1.0
            Assert.Equal(1.0, result);
        }

        [Fact]
        public void SequentialRule_EmptyListReturnsZero()
        {
            var rule = SequentialRule.Create();
            var result = rule.Compute(_empty, _getWeight);
            Assert.Equal(0, result);
        }

        [Fact]
        public void MaxRule_ShouldReturnMaxProgress()
        {
            var rule = MaxRule.Create();
            var nodes = new List<IProgressNode<double>>
            {
                new DummyNode(0.2),
                new DummyNode(0.9),
                new DummyNode(0.5)
            };
            var result = rule.Compute(nodes, _getWeight);
            Assert.Equal(0.9, result);
        }

        [Fact]
        public void MaxRule_EmptyListReturnsZero()
        {
            var rule = MaxRule.Create();
            var result = rule.Compute(_empty, _getWeight);
            Assert.Equal(0, result);
        }

        [Fact]
        public void MinRule_ShouldReturnMinProgress()
        {
            var rule = MinRule.Create();
            var nodes = new List<IProgressNode<double>>
            {
                new DummyNode(0.2),
                new DummyNode(0.9),
                new DummyNode(0.5)
            };
            var result = rule.Compute(nodes, _getWeight);
            Assert.Equal(0.2, result);
        }

        [Fact]
        public void MinRule_EmptyListReturnsZero()
        {
            var rule = MinRule.Create();
            var result = rule.Compute(_empty, _getWeight);
            Assert.Equal(0, result);
        }

        private class DummyNode : IProgressNode<double>
        {
            public double Progress { get; }
            public event Action<IProgressNode<double>, double> OnProgressChanged { add { } remove { } }
            public DummyNode(double progress) { Progress = progress; }
        }
    }
}