using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    public class HandleTests
    {
        [Fact]
        public void Continue_Should_Have_Correct_Properties()
        {
            Assert.False(ProcessorHandle.Continue.IsAborted);
            Assert.False(ProcessorHandle.Continue.SkipAfterCallbacks);
            Assert.Null(ProcessorHandle.Continue.Result);
        }

        [Fact]
        public void Abort_With_Result_Should_Return_New_Instance_With_Result()
        {
            var result = "test";
            var handle = ProcessorHandle.Abort(result);
            Assert.True(handle.IsAborted);
            Assert.False(handle.SkipAfterCallbacks);
            Assert.Equal(result, handle.Result);
        }

        [Fact]
        public void AbortAll_With_Result_Should_Skip_After()
        {
            var handle = ProcessorHandle.AbortAll(42);
            Assert.True(handle.IsAborted);
            Assert.True(handle.SkipAfterCallbacks);
            Assert.Equal(42, handle.Result);
        }
    }
}
