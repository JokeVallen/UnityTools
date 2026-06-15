using Orchestrator.Tasks;

namespace Orchestrator.Tests.Tasks
{
    // ======================== 接口契约测试 ========================

    public class InterfaceContractTests
    {
        [Fact]
        public void IStep_ShouldHaveRequiredProperties()
        {
            var step = new NullStep("TestStep");

            Assert.Equal("TestStep", step.Key);
            Assert.NotNull(step.Dependencies);
        }

        [Fact]
        public async Task ITaskStep_ExecuteAsync_ShouldReturnStepResult()
        {
            var step = new NullStep("TestStep", StepFlow.Continue);
            var context = new TestContext();
            var result = await step.ExecuteAsync(context, CancellationToken.None);

            Assert.Equal(StepFlow.Continue, result.Flow);
        }

        [Fact]
        public async Task ITaskBehavior_HandleAsync_ShouldWrapExecution()
        {
            var executionLog = new List<string>();
            var behavior = new RecordBehavior("TestBehavior", executionLog);
            var innerStep = new NullStep("Inner", StepFlow.Continue);
            var context = new TestContext();

            var stepper = new TaskBehaviorStepper<string>(
                new[] { behavior },
                0,
                innerStep,
                context);

            var result = await stepper.NextAsync(CancellationToken.None);

            Assert.Equal(StepFlow.Continue, result.Flow);
            Assert.Equal(2, executionLog.Count);
            Assert.Equal("TestBehavior_Before", executionLog[0]);
            Assert.Equal("TestBehavior_After", executionLog[1]);
        }
    }
}
