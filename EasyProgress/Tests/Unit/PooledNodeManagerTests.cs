using System.Threading.Tasks;
using Xunit;

namespace EasyProgress.Core.Tests
{
    public class PooledNodeManagerTests
    {
        [Fact]
        public void Acquire_WhenEmpty_CreatesNewNode()
        {
            var manager = new PooledNodeManager<double, DefaultLeaf>(_ => new DefaultLeaf());
            var leaf = manager.Acquire();
            Assert.NotNull(leaf);
        }

        [Fact]
        public void Acquire_AfterRelease_ReturnsReusedNode()
        {
            var manager = new PooledNodeManager<double, DefaultLeaf>(_ => new DefaultLeaf());
            var leaf1 = manager.Acquire();
            leaf1.Report(0.5);
            manager.Release(leaf1);
            var leaf2 = manager.Acquire();
            Assert.Same(leaf1, leaf2);
            Assert.Equal(0, leaf2.Progress);
        }

        [Fact]
        public void Release_CallsReset()
        {
            var manager = new PooledNodeManager<double, DefaultLeaf>(_ => new DefaultLeaf());
            var leaf = manager.Acquire();
            leaf.Report(0.7);
            manager.Release(leaf);
            Assert.Equal(0, leaf.Progress);
        }

        [Fact]
        public void Acquire_WithUserData_PassesToFactory()
        {
            var manager = new PooledNodeManager<double, DummyLeaf>(userData =>
            {
                var name = userData as string;
                return new DummyLeaf(name);
            });
            var leaf = manager.Acquire("test");
            Assert.Equal("test", leaf.Name);
        }

        [Fact]
        public async Task ConcurrentAcquireRelease_NoCorruption()
        {
            var manager = new PooledNodeManager<double, DefaultLeaf>(_ => new DefaultLeaf());
            var tasks = new Task[100];
            for (int i = 0; i < 100; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        var leaf = manager.Acquire();
                        leaf.Report(j % 2);
                        manager.Release(leaf);
                    }
                });
            }
            await Task.WhenAll(tasks);
            // If no exception, test passes.
        }

        private class DummyLeaf : IProgressLeaf<double>, IResettable
        {
            public string Name { get; }
            public double Progress { get; private set; }
            public event Action<IProgressNode<double>, double> OnProgressChanged;
            public DummyLeaf(string name) => Name = name;
            public void Report(double value) => Progress = value;
            public void Complete() => Progress = 1;
            public void Reset() => Progress = 0;
        }
    }
}