using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class FactoryTests
    {
        [Fact]
        public void TransientFactory_Should_Create_New_Instances()
        {
            var factory = TransientProcessorFactory.Default;
            var type = typeof(CountingProcessor);
            var a = factory.Create(type);
            var b = factory.Create(type);
            Assert.NotSame(a, b);
        }

        [Fact]
        public void SingletonFactory_Should_Return_Same_Instance()
        {
            var factory = SingletonProcessorFactory.Default;
            var type = typeof(CountingProcessor);
            var a = factory.Create(type);
            var b = factory.Create(type);
            Assert.Same(a, b);
        }
    }
}
