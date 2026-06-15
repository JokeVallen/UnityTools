using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    // ======================== ExecutionResult 类型测试 ========================

    public class ExecutionResultTests
    {
        [Fact]
        public void ExecutionResult_ShouldInitializePropertiesCorrectly()
        {
            var success = true;
            var duration = TimeSpan.FromSeconds(5);
            var result = new ExecutionResult(success, duration);

            Assert.Equal(success, result.Success);
            Assert.Equal(duration, result.Duration);
        }

        [Fact]
        public void ExecutionResult_Failure_ShouldSetSuccessFalse()
        {
            var result = new ExecutionResult(false, TimeSpan.Zero);
            Assert.False(result.Success);
        }

        [Fact]
        public void StepExecutionResult_ShouldInitializePropertiesCorrectly()
        {
            var stepKey = "TestStep";
            var success = true;
            var flow = StepFlow.Continue;
            var exception = new InvalidOperationException();
            var duration = TimeSpan.FromMilliseconds(100);

            var result = new StepExecutionResult<string>(stepKey, success, flow, exception, duration);

            Assert.Equal(stepKey, result.StepKey.Value);
            Assert.Equal(success, result.Success);
            Assert.Equal(flow, result.Flow);
            Assert.Equal(exception, result.Exception);
            Assert.Equal(duration, result.Duration);
        }

        [Fact]
        public void StepExecutionResult_WithException_ShouldStoreException()
        {
            var expectedException = new ArgumentException("test error");
            var result = new StepExecutionResult<string>("Step", false, StepFlow.Fail, expectedException, TimeSpan.Zero);

            Assert.Equal(expectedException, result.Exception);
        }

        [Fact]
        public void StepExecutionResult_WithOptionalStepKey()
        {
            var result = new StepExecutionResult<string>("Step", true, StepFlow.Continue, null, TimeSpan.Zero);
            Assert.True(result.StepKey.HasValue);
            Assert.Equal("Step", result.StepKey.Value);
        }
    }
}
