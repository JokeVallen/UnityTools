using Moq;
using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    public class InterfaceContractTests
    {
        [Fact]
        public void IStep_ShouldHaveRequiredProperties()
        {
            var mockStep = new Mock<IValueTaskStep<string, int>>();
            mockStep.Setup(s => s.Name).Returns("TestStep");
            mockStep.Setup(s => s.Dependencies).Returns(new List<IStep>());
            var step = mockStep.Object;
            Assert.Equal("TestStep", step.Name);
            Assert.NotNull(step.Dependencies);
        }

        [Fact]
        public async Task IStep_ExecuteAsync_ShouldReturnStepResult()
        {
            var mockStep = new Mock<IValueTaskStep<string, int>>();
            var expectedResult = StepResult<int>.Continue(42);
            mockStep.Setup(s => s.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns(new ValueTask<StepResult<int>>(expectedResult));

            var result = await mockStep.Object.ExecuteAsync("test", CancellationToken.None);
            Assert.Equal(expectedResult, result);
            Assert.Equal(42, result.Output);
        }

        [Fact]
        public async Task IBehavior_HandleAsync_ShouldWrapExecution()
        {
            var mockBehavior = new Mock<IValueTaskBehavior<string, int>>();
            var expectedResult = StepResult<int>.Continue(42);

            mockBehavior.Setup(b => b.HandleAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<ValueTask<StepResult<int>>>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<StepResult<int>>(expectedResult));

            var result = await mockBehavior.Object.HandleAsync(
                "input",
                () => new ValueTask<StepResult<int>>(StepResult<int>.Continue(42)),
                CancellationToken.None);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IExecutionResult_ShouldProvideCompleteExecutionInfo()
        {
            var mockResult = new Mock<IExecutionResult<string, IStepExecutionResult>>();
            var stepResults = new List<IStepExecutionResult>();
            var duration = TimeSpan.FromSeconds(5);
            mockResult.Setup(r => r.Success).Returns(true);
            mockResult.Setup(r => r.Output).Returns("final output");
            mockResult.Setup(r => r.StepResults).Returns(stepResults);
            mockResult.Setup(r => r.Duration).Returns(duration);

            var result = mockResult.Object;
            Assert.True(result.Success);
            Assert.Equal("final output", result.Output);
            Assert.Equal(stepResults, result.StepResults);
            Assert.Equal(duration, result.Duration);
        }

        [Fact]
        public void IStepExecutionResult_ShouldProvideStepExecutionDetails()
        {
            var mockResult = new Mock<IStepExecutionResult>();
            var exception = new InvalidOperationException();
            var duration = TimeSpan.FromMilliseconds(100);
            mockResult.Setup(r => r.StepName).Returns("Step1");
            mockResult.Setup(r => r.Success).Returns(false);
            mockResult.Setup(r => r.Flow).Returns(StepFlow.Fail);
            mockResult.Setup(r => r.Output).Returns("output");
            mockResult.Setup(r => r.Exception).Returns(exception);
            mockResult.Setup(r => r.Duration).Returns(duration);

            var result = mockResult.Object;
            Assert.Equal("Step1", result.StepName);
            Assert.False(result.Success);
            Assert.Equal(StepFlow.Fail, result.Flow);
            Assert.Equal("output", result.Output);
            Assert.Equal(exception, result.Exception);
            Assert.Equal(duration, result.Duration);
        }
    }
}