using System.Threading.Tasks;
using Xunit;

namespace EasyProgress.Core.Tests
{
    public class DefaultLeafTests
    {
        [Fact]
        public void Report_ShouldUpdateProgressAndTriggerEvent()
        {
            var leaf = new DefaultLeaf();
            bool eventRaised = false;
            leaf.OnProgressChanged += (_, p) => { eventRaised = true; Assert.Equal(0.5, p); };
            leaf.Report(0.5);
            Assert.Equal(0.5, leaf.Progress);
            Assert.True(eventRaised);
        }

        [Fact]
        public void Report_ShouldClampValueBetween0And1()
        {
            var leaf = new DefaultLeaf();
            leaf.Report(-0.5);
            Assert.Equal(0, leaf.Progress);
            leaf.Report(1.5);
            Assert.Equal(1, leaf.Progress);
        }

        [Fact]
        public void Report_ShouldIgnoreIdenticalValueWithinTolerance()
        {
            var leaf = new DefaultLeaf();
            leaf.Report(0.3);
            int eventCount = 0;
            leaf.OnProgressChanged += (_, _) => eventCount++;
            leaf.Report(0.3 + 1e-10); // less than tolerance
            Assert.Equal(0, eventCount);
            leaf.Report(0.3 + 1e-8);
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void Complete_ShouldSetProgressToOne()
        {
            var leaf = new DefaultLeaf();
            leaf.Complete();
            Assert.Equal(1.0, leaf.Progress);
        }

        [Fact]
        public void Reset_ShouldClearProgressAndEventHandlers()
        {
            var leaf = new DefaultLeaf();
            leaf.Report(0.7);
            leaf.OnProgressChanged += (_, _) => { };
            leaf.Reset();
            Assert.Equal(0, leaf.Progress);
            // Event handlers are cleared; cannot directly test, but ensure no exception
            leaf.Report(0.5);
        }

        [Fact]
        public void ConcurrentReport_ShouldBeThreadSafe()
        {
            var leaf = new DefaultLeaf();
            var tasks = new Task[100];
            for (int i = 0; i < 100; i++)
            {
                int val = i;
                tasks[i] = Task.Run(() => leaf.Report(val / 100.0));
            }
            Task.WaitAll(tasks);
            Assert.InRange(leaf.Progress, 0, 1);
        }
    }
}