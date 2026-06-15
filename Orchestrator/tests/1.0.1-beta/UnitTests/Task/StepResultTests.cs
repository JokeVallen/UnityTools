namespace Orchestrator.Tests.Tasks
{
    // ======================== StepResult 类型测试 ========================

    public class StepResultTests
    {
        [Fact]
        public void Continue_ShouldCreateContinueResult()
        {
            var result = StepResult.Continue();

            Assert.Equal(StepFlow.Continue, result.Flow);
            Assert.Null(result.Exception);
        }

        [Fact]
        public void Break_ShouldCreateBreakResult()
        {
            var result = StepResult.Break();

            Assert.Equal(StepFlow.Break, result.Flow);
            Assert.Null(result.Exception);
        }

        [Fact]
        public void Fail_ShouldCreateFailResultWithException()
        {
            var expectedException = new InvalidOperationException("test error");
            var result = StepResult.Fail(expectedException);

            Assert.Equal(StepFlow.Fail, result.Flow);
            Assert.Equal(expectedException, result.Exception);
        }
    }
}
