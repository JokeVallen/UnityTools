using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    // ======================== 接口契约测试 ========================

    public class InterfaceContractTests
    {
        [Fact]
        public void IStep_ShouldHaveRequiredProperties()
        {
            var step = new NullStep("TestStep", StepFlow.Continue);

            Assert.Equal("TestStep", step.Key);
            Assert.NotNull(step.Dependencies);
        }

        [Fact]
        public async Task IValueTaskStep_ExecuteAsync_ShouldReturnStepResult()
        {
            var step = new NullStep("TestStep", StepFlow.Continue);
            var context = new TestContext();
            var result = await step.ExecuteAsync(context, CancellationToken.None);

            Assert.Equal(StepFlow.Continue, result.Flow);
            Assert.Null(result.Exception);
        }

        [Fact]
        public async Task IValueTaskBehavior_HandleAsync_ShouldWrapExecution()
        {
            var executionLog = new List<string>();
            var behavior = new RecordBehavior("TestBehavior", executionLog);
            var innerStep = new NullStep("Inner", StepFlow.Continue);
            var context = new TestContext();

            var stepper = new ValueTaskBehaviorStepper<string>(
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

        [Fact]
        public void IValueTaskOrchestrator_ShouldExecuteSteps()
        {
            var step = new NullStep("TestStep", StepFlow.Continue);
            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();

            Assert.NotNull(orchestrator);
        }
    }
}
