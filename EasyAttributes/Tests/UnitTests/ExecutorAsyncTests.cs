using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class ExecutorAsyncTests
    {
        private static readonly IReadOnlyDictionary<Type, IFeature> EmptyFeatures = new Dictionary<Type, IFeature>();

        [Fact]
        public async Task ExecuteAsync_Should_Run_Sync_And_Async_In_Order()
        {
            var callOrder = new List<string>();
            var sync = new SpySyncProcessor(callOrder);
            var async = new SpyAsyncProcessor(callOrder);
            var factory = new FakeProcessorFactory(sync, async);
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(SpySyncProcessor), typeof(SpyAsyncProcessor));
            var executor = new DefaultExecutorAsync(registry, factory, NullExceptionHandler.Instance, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            await executor.ExecuteAsync(ctx);

            Assert.Collection(callOrder,
                s => Assert.Equal("Sync.Before", s),
                s => Assert.Equal("Sync.Process", s),
                s => Assert.Equal("Async.Before", s),
                s => Assert.Equal("Async.Process", s),
                s => Assert.Equal("Async.After", s),
                s => Assert.Equal("Sync.After", s));
        }

        [Fact]
        public async Task ExecuteAsync_Should_Abort_And_Not_Run_Subsequent()
        {
            var abortProc = new AbortProcessor(abort: true, skipAfter: false);
            var normalProc = new CountingProcessor();
            var factory = new FakeProcessorFactory(abortProc, normalProc);
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(AbortProcessor), typeof(CountingProcessor));
            var executor = new DefaultExecutorAsync(registry, factory, NullExceptionHandler.Instance, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            await executor.ExecuteAsync(ctx);

            Assert.True(abortProc.AfterCalled);
            Assert.False(normalProc.BeforeCalled);
        }

        [Fact]
        public async Task ExecuteAsync_With_Cancellation_Should_Throw_Before_Process()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(CountingProcessor));
            var factory = new FakeProcessorFactory(new CountingProcessor());
            var executor = new DefaultExecutorAsync(registry, factory, NullExceptionHandler.Instance, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            await Assert.ThrowsAsync<OperationCanceledException>(() => executor.ExecuteAsync(ctx, cts.Token));
        }
    }
}
