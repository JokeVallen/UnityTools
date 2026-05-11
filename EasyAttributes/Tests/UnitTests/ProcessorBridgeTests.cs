using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class ProcessorBridgeTests
    {
        private class MyAttr : EasyAttribute { }
        private class MyProcessor : Processor<MyAttr>
        {
            public bool TypedCalled { get; private set; }
            public override IProcessorHandle Process(IContext context, MyAttr attribute)
            {
                TypedCalled = true;
                return ProcessorHandle.Continue;
            }
        }

        [Fact]
        public void NonGeneric_Process_With_Correct_Attribute_Should_Call_Typed_Process()
        {
            var proc = new MyProcessor();
            var ctx = new MockContext { Attribute = new MyAttr() };
            ((IProcessor)proc).Process(ctx);
            Assert.True(proc.TypedCalled);
        }

        [Fact]
        public void NonGeneric_Process_With_Wrong_Attribute_Should_Return_Continue_And_Not_Call_Typed()
        {
            var proc = new MyProcessor();
            var ctx = new MockContext { Attribute = new TestAttribute() };
            var handle = ((IProcessor)proc).Process(ctx);
            Assert.Equal(ProcessorHandle.Continue, handle);
            Assert.False(proc.TypedCalled);
        }
    }
}
