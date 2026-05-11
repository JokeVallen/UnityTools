using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class FeaturesTests
    {
        [Fact]
        public void Inject_Global_Features_Should_Not_Throw_When_Context_Has_No_Features()
        {
            var feature = new TestFeature();
            var global = new Dictionary<System.Type, IFeature> { { typeof(TestFeature), feature } };
            var registry = new FakeRegistry(typeof(TestAttribute), typeof(CountingProcessor));
            var factory = new FakeProcessorFactory(new CountingProcessor());
            var executor = new DefaultExecutor(registry, factory, NullExceptionHandler.Instance, global);
            var ctx = new MockContext { Attribute = new TestAttribute() };

            executor.Execute(ctx);
            Assert.True(ctx.Features.ContainsKey(typeof(TestFeature)));
        }
    }
}
