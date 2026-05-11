using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class ExceptionTests
    {
        [Fact]
        public void ProcessorBeforeException_Should_Carry_Context_And_ProcessorType()
        {
            var ctx = new MockContext();
            var ex = new ProcessorBeforeException(typeof(CountingProcessor), ctx, new InvalidOperationException());
            Assert.Equal(typeof(CountingProcessor), ex.ProcessorType);
            Assert.Same(ctx, ex.Context);
        }

        [Fact]
        public void FeatureTypeException_Should_Contain_FeatureType()
        {
            var ctx = new MockContext();
            var ex = new FeatureTypeException(typeof(string), ctx);
            Assert.Equal(typeof(string), ex.FeatureType);
        }
    }
}
