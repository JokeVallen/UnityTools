namespace Orchestrator.Tests.ValueTasks
{
    public class ExecutionResultTests
    {
        [Fact]
        public void ExecutionResult_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var success = true;
            var output = "test output";
            var stepResults = new List<StepExecutionResult>();  // 具体类型
            var duration = TimeSpan.FromSeconds(5);

            // Act
            var result = new ExecutionResult<string>(success, output, stepResults, duration);

            // Assert
            Assert.Equal(success, result.Success);
            Assert.Equal(output, result.Output);
            Assert.Equal(stepResults, result.StepResults);
            Assert.Equal(duration, result.Duration);
        }

        [Fact]
        public void ExecutionResult_WithNullOutput_ShouldAllowNull()
        {
            // Arrange
            var result = new ExecutionResult<string>(true, default, new List<StepExecutionResult>(), TimeSpan.Zero);

            // Act & Assert
            Assert.Null(result.Output);
        }

        [Fact]
        public void StepExecutionResult_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var stepName = "TestStep";
            var success = true;
            var flow = StepFlow.Continue;
            var output = "step output";
            var exception = new InvalidOperationException();
            var duration = TimeSpan.FromMilliseconds(100);

            // Act
            var result = new StepExecutionResult(stepName, success, flow, output, exception, duration);

            // Assert
            Assert.Equal(stepName, result.StepName);
            Assert.Equal(success, result.Success);
            Assert.Equal(flow, result.Flow);
            Assert.Equal(output, result.Output);
            Assert.Equal(exception, result.Exception);
            Assert.Equal(duration, result.Duration);
        }

        [Fact]
        public void StepExecutionResult_WithNullOutput_ShouldAllowNull()
        {
            // Arrange
            var result = new StepExecutionResult("Step", true, StepFlow.Continue, null, null, TimeSpan.Zero);

            // Act & Assert
            Assert.Null(result.Output);
        }

        [Fact]
        public void StepExecutionResult_WithException_ShouldStoreException()
        {
            // Arrange
            var expectedException = new ArgumentException("test error");
            var result = new StepExecutionResult("Step", false, StepFlow.Fail, null, expectedException, TimeSpan.Zero);

            // Act & Assert
            Assert.Equal(expectedException, result.Exception);
        }
    }
}