using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class ContextTests
    {
        [Fact]
        public void Constructor_Should_Set_Attribute_Enabled_Priority()
        {
            var attr = new TestAttribute { Enabled = false, Priority = 3 };
            var ctx = new TestableContext(attr);
            Assert.Equal(attr, ctx.Attribute);
            Assert.False(ctx.IsEnabled);
            Assert.Equal(3, ctx.Priority);
        }

        [Fact]
        public void Items_Should_Be_Lazy_And_Cached()
        {
            var ctx = new TestableContext(new TestAttribute());
            var items1 = ctx.Items;
            var items2 = ctx.Items;
            Assert.NotNull(items1);
            Assert.Same(items1, items2);
        }

        [Fact]
        public void SetItem_Should_Add_And_Remove_Correctly()
        {
            var ctx = new TestableContext(new TestAttribute());
            IContextWriter writer = ctx;
            writer.SetItem("A", 1);
            Assert.True(ctx.Items.ContainsKey("A"));
            writer.RemoveItem("A");
            Assert.False(ctx.Items.ContainsKey("A"));
        }

        [Fact]
        public void SetFeature_Should_Throw_If_Type_Not_Implement_IFeature()
        {
            var ctx = new TestableContext(new TestAttribute());
            IContextWriter writer = ctx;
            Assert.Throws<ArgumentException>(() => writer.SetFeature(typeof(string), new TestFeature()));
        }

        [Fact]
        public void SetFeature_And_Remove_Should_Work()
        {
            var ctx = new TestableContext(new TestAttribute());
            IContextWriter writer = ctx;
            var feature = new TestFeature();
            writer.SetFeature(typeof(TestFeature), feature);
            Assert.Same(feature, ctx.Features[typeof(TestFeature)]);
            writer.RemoveFeature(typeof(TestFeature));
            Assert.False(ctx.Features.ContainsKey(typeof(TestFeature)));
        }

        // Helper class to expose protected constructor
        private class TestableContext : Context<TestAttribute>
        {
            public TestableContext(TestAttribute attribute) : base(attribute) { }
        }
    }
}
