using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EasyProgress.Core.Tests
{
    public class ConcurrentTests
    {
        [Fact]
        public async Task MultipleThreads_ReportOnSameLeaf_ShouldNotCorrupt()
        {
            var leaf = new DefaultLeaf();
            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => leaf.Report(0.5)));
            }
            await Task.WhenAll(tasks);
            Assert.Equal(0.5, leaf.Progress);
        }

        [Fact]
        public async Task MultipleThreads_AddRemoveChildren_ShouldMaintainConsistency()
        {
            var composite = new WeightedRealtimeComposite(WeightedAverageRule.Create());
            var leaves = Enumerable.Range(0, 20).Select(_ => new DefaultLeaf()).ToList();
            var tasks = new List<Task>();
            // Mix of add, remove, report
            for (int i = 0; i < 100; i++)
            {
                int idx = i % leaves.Count;
                tasks.Add(Task.Run(() => composite.AddChild(leaves[idx], 0.1f)));
                tasks.Add(Task.Run(() => composite.RemoveChild(leaves[idx])));
                tasks.Add(Task.Run(() => leaves[idx].Report(i / 100.0)));
            }
            await Task.WhenAll(tasks);
            // No exception, composite is in some valid state
        }

        [Fact]
        public async Task PooledNodeManager_MultiThreaded_AcquireRelease()
        {
            var pool = new PooledNodeManager<double, DefaultLeaf>(_ => new DefaultLeaf());
            var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var leaf = pool.Acquire();
                    leaf.Report(0.3);
                    pool.Release(leaf);
                }
            })).ToArray();
            await Task.WhenAll(tasks);
        }
    }
}