using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class ExecutorTests
    {
        private static readonly IReadOnlyDictionary<Type, IFeature> EmptyFeatures = new Dictionary<Type, IFeature>();

        [Fact]
        public void Execute_Should_Run_All_Processors()
        {
            var proc1 = new CountingProcessor();
            var proc2 = new CountingProcessor();
            var factory = new FakeProcessorFactory(proc1, proc2);
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(CountingProcessor), typeof(CountingProcessor));
            var executor = new DefaultExecutor(registry, factory, NullExceptionHandler.Instance, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            executor.Execute(ctx);

            Assert.True(proc1.BeforeCalled && proc1.ProcessCalled && proc1.AfterCalled);
            Assert.True(proc2.BeforeCalled && proc2.ProcessCalled && proc2.AfterCalled);
        }

        [Fact]
        public void Execute_Should_Abort_And_Not_Run_Subsequent_Processors()
        {
            var abortProc = new AbortProcessor(abort: true, skipAfter: false);
            var normalProc = new CountingProcessor();
            var factory = new FakeProcessorFactory(abortProc, normalProc);
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(AbortProcessor), typeof(CountingProcessor));
            var executor = new DefaultExecutor(registry, factory, NullExceptionHandler.Instance, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            var handle = executor.Execute(ctx);

            Assert.True(handle.IsAborted);
            Assert.False(handle.SkipAfterCallbacks);
            Assert.True(abortProc.AfterCalled); // After should be called
            Assert.False(normalProc.BeforeCalled); // Never reached
        }

        [Fact]
        public void Execute_Should_Skip_After_When_SkipAfterCallbacks_True()
        {
            var abortProc = new AbortProcessor(abort: true, skipAfter: true);
            var factory = new FakeProcessorFactory(abortProc);
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(AbortProcessor));
            var executor = new DefaultExecutor(registry, factory, NullExceptionHandler.Instance, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            executor.Execute(ctx);

            Assert.False(abortProc.AfterCalled);
        }

        [Fact]
        public void Before_Exception_Should_Stop_Chain_But_Run_After_Of_Executed()
        {
            var throwProc = new ThrowingBeforeProcessor();
            var normalProc = new CountingProcessor(); // will never run Before
            var earlyProc = new CountingProcessor(); // runs before throwing processor
            var factory = new FakeProcessorFactory(earlyProc, throwProc, normalProc);
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(CountingProcessor), typeof(ThrowingBeforeProcessor), typeof(CountingProcessor));
            var handler = new MemorizingExceptionHandler(true);
            var executor = new DefaultExecutor(registry, factory, handler, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            executor.Execute(ctx);

            Assert.True(earlyProc.AfterCalled); // executed Before, so After must run
            Assert.False(throwProc.AfterCalled); // thrown in Before, no After
            Assert.False(normalProc.BeforeCalled); // never reached
            Assert.True(handler.WasCalled);
        }

        [Fact]
        public void Execute_With_Disabled_Context_Should_Return_Continue()
        {
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(CountingProcessor));
            var executor = new DefaultExecutor(registry, TransientProcessorFactory.Default, NullExceptionHandler.Instance, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute(), IsEnabled = false };

            var handle = executor.Execute(ctx);
            Assert.Equal(ProcessorHandle.Continue, handle);
        }

        [Fact]
        public void Execute_With_Empty_Registry_Should_Return_Continue()
        {
            var registry = new FakeRegistry(typeof(TestAttribute)); // no processors
            var executor = new DefaultExecutor(registry, TransientProcessorFactory.Default, NullExceptionHandler.Instance, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            var handle = executor.Execute(ctx);
            Assert.Equal(ProcessorHandle.Continue, handle);
        }

        [Fact]
        public void After_Exception_Handled_Should_Continue_Other_Afters()
        {
            var normalProc = new CountingProcessor();
            var throwProc = new ThrowingAfterProcessor();
            var factory = new FakeProcessorFactory(normalProc, throwProc);
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(CountingProcessor), typeof(ThrowingAfterProcessor));
            var handler = new MemorizingExceptionHandler(true);
            var executor = new DefaultExecutor(registry, factory, handler, EmptyFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            executor.Execute(ctx);

            Assert.True(normalProc.AfterCalled); // executes normally
            Assert.True(throwProc.AfterCalled); // after called, but exception handled
            Assert.True(handler.WasCalled);
        }

        [Fact]
        public void Execute_Should_Inject_Global_Feature_If_Not_Present()
        {
            var feature = new TestFeature();
            var globalFeatures = new Dictionary<Type, IFeature> { { typeof(TestFeature), feature } };
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(CountingProcessor));
            var factory = new FakeProcessorFactory(new CountingProcessor());
            var executor = new DefaultExecutor(registry, factory, NullExceptionHandler.Instance, globalFeatures);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            executor.Execute(ctx);

            Assert.True(ctx.Features.TryGetValue(typeof(TestFeature), out var injected));
            Assert.Same(feature, injected);
        }

        [Fact]
        public void Execute_Should_Not_Override_Existing_Feature()
        {
            var existing = new TestFeature { Name = "Existing" };
            var global = new TestFeature { Name = "Global" };
            var ctx = new MockContext
            {
                Attribute = new TestAttribute(),
                Features = new Dictionary<Type, IFeature> { { typeof(TestFeature), existing } }
            };
            var globalFeatures = new Dictionary<Type, IFeature> { { typeof(TestFeature), global } };
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(CountingProcessor));
            var factory = new FakeProcessorFactory(new CountingProcessor());
            var executor = new DefaultExecutor(registry, factory, NullExceptionHandler.Instance, globalFeatures);

            executor.Execute(ctx);

            var retrieved = ctx.Features[typeof(TestFeature)] as TestFeature;
            Assert.Equal("Existing", retrieved!.Name);
        }
    }
}
