using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class ExtensionTests
    {
        [Fact]
        public void GetItem_Should_Return_Value_If_Exists()
        {
            var ctx = new MockContext
            {
                Items = new Dictionary<string, object> { { "key", "value" } }
            };
            var result = ctx.GetItem<string>("key");
            Assert.Equal("value", result);
        }

        [Fact]
        public void GetItem_Should_Return_Default_If_Not_Found()
        {
            var ctx = new MockContext();
            var result = ctx.GetItem<int>("missing");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetFeature_Should_Return_Feature()
        {
            var feature = new TestFeature();
            var ctx = new MockContext
            {
                Features = new Dictionary<System.Type, IFeature> { { typeof(TestFeature), feature } }
            };
            var retrieved = ctx.GetFeature<TestFeature>();
            Assert.Same(feature, retrieved);
        }

        [Fact]
        public void GetResult_Should_Return_Default_If_Null()
        {
            var handle = ProcessorHandle.Continue;
            Assert.Equal(0, handle.GetResult<int>());
        }
    }
}
