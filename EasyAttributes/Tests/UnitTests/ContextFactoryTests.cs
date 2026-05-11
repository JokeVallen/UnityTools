using EasyAttributes.Core;
using System.Reflection;

namespace EasyAttributes.UnitTests
{
    public class ContextFactoryTests
    {
        private static readonly MethodInfo SampleMethod = typeof(SampleClass).GetMethod(nameof(SampleClass.Do))!;
        private static readonly PropertyInfo SampleProperty = typeof(SampleClass).GetProperty(nameof(SampleClass.Name))!;

        [Fact]
        public void CreateMethodContext_Returns_IMethodContext_With_Values()
        {
            var attr = new TestAttribute();
            var target = new SampleClass();
            var args = new object[] { 1 };
            var ctx = ContextFactory.CreateMethodContext(attr, SampleMethod, target, args);

            Assert.NotNull(ctx);
            var mctx = Assert.IsAssignableFrom<IMethodContext>(ctx);
            Assert.Equal(SampleMethod, mctx.Method);
            Assert.Equal(target, mctx.Target);
            Assert.Equal(args, mctx.Arguments);
        }

        [Fact]
        public void CreatePropertyContext_Should_Set_Accessor_And_Value()
        {
            var attr = new TestAttribute();
            var ctx = ContextFactory.CreatePropertyContext(attr, SampleProperty, PropertyAccessor.Set, null, 42);
            Assert.Equal(PropertyAccessor.Set, ctx.Accessor);
            Assert.Equal(42, ctx.Value);
        }

        [Fact]
        public void All_Contexts_Implement_IAsyncContext()
        {
            var ctx = ContextFactory.CreateMethodContext(new TestAttribute(), SampleMethod, null, null);
            Assert.IsAssignableFrom<IAsyncContext>(ctx);
        }

        private class SampleClass
        {
            public string? Name { get; set; }
            public void Do(int x) { }
        }
    }
}
