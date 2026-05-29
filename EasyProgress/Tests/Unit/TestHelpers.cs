using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EasyProgress.Core.Tests
{
    internal static class TestHelpers
    {
        public static void AssertEventRaised<T>(Action subscribe, Action unsubscribe, Action trigger, int expectedCount = 1)
        {
            var count = 0;
            subscribe();
            trigger();
            unsubscribe();
            Assert.Equal(expectedCount, count);
        }

        public static async Task AssertEventRaisedAsync<T>(Action subscribe, Action unsubscribe, Func<Task> triggerAsync, int expectedCount = 1)
        {
            var count = 0;
            subscribe();
            await triggerAsync();
            unsubscribe();
            Assert.Equal(expectedCount, count);
        }

        public static void AssertProgressChanges(Action<IProgressNode<double>> act, params double[] expectedProgressSequence)
        {
            var list = new List<double>();
            void handler(IProgressNode<double> node, double p) => list.Add(p);
            var node = new DefaultLeaf();
            node.OnProgressChanged += handler;
            act(node);
            node.OnProgressChanged -= handler;
            Assert.Equal(expectedProgressSequence, list);
        }

        public static void AssertApproxEqual(double expected, double actual, double tolerance = 1e-7)
        {
            Assert.True(Math.Abs(expected - actual) < tolerance, $"Expected {expected}, actual {actual}");
        }
    }
}