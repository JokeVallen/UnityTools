namespace Orchestrator.Tests.ValueTasks
{
    public class StepResultTests
    {
        [Fact]
        public void Continue_ShouldCreateSuccessResultWithContinueFlow()
        {
            var result = StepResult<string>.Continue("test output");
            Assert.Equal(StepFlow.Continue, result.Flow);
            Assert.Equal("test output", result.Output);
            Assert.Null(result.Exception);
        }

        [Fact]
        public void Break_ShouldCreateBreakResultWithDefaultOutput()
        {
            var result = StepResult<int>.Break();
            Assert.Equal(StepFlow.Break, result.Flow);
            Assert.Equal(default, result.Output);
            Assert.Null(result.Exception);
        }

        [Fact]
        public void Break_ShouldCreateBreakResultWithCustomOutput()
        {
            var result = StepResult<int>.Break(42);
            Assert.Equal(StepFlow.Break, result.Flow);
            Assert.Equal(42, result.Output);
            Assert.Null(result.Exception);
        }

        [Fact]
        public void Fail_ShouldCreateFailResultWithException()
        {
            var expectedException = new InvalidOperationException("test error");
            var result = StepResult<string>.Fail(expectedException);

            Assert.Equal(StepFlow.Fail, result.Flow);
            Assert.Equal(default, result.Output);
            Assert.Equal(expectedException, result.Exception);
        }

        // 移除 Constructor_ShouldInitializePropertiesCorrectly 测试，因为构造器已私有
    }
}