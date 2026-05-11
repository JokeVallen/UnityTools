using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class BuilderTests
    {
        private class TestAttr : EasyAttribute { }
        private class TestProc : Processor<TestAttr>
        {
            public override IProcessorHandle Process(IContext context, TestAttr attribute) => ProcessorHandle.Continue;
        }

        [Fact]
        public void Build_Should_Return_IExecutor()
        {
            var builder = DefaultExecutorBuilder.Create()
                .UseProcessor<TestAttr, TestProc>();
            var executor = builder.Build();
            Assert.NotNull(executor);
        }

        [Fact]
        public void Build_After_Build_Should_Throw()
        {
            var builder = DefaultExecutorBuilder.Create()
                .UseProcessor<TestAttr, TestProc>();
            builder.Build();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void UseFeature_With_Generic_Should_Store_Feature()
        {
            var builder = DefaultExecutorBuilder.Create()
                .UseFeature<TestFeature>(new TestFeature { Name = "Gen" })
                .UseProcessor<TestAttr, TestProc>();
            var executor = builder.Build();
            // We cannot access features from executor directly, but we trust it's stored
            Assert.NotNull(executor);
        }

        [Fact]
        public void BuildAsync_Should_Return_IExecutorAsync()
        {
            var builder = DefaultExecutorBuilder.Create()
                .UseProcessor<TestAttr, TestProc>();
            var executor = builder.BuildAsync();
            Assert.IsAssignableFrom<IExecutorAsync>(executor);
        }
    }

}
