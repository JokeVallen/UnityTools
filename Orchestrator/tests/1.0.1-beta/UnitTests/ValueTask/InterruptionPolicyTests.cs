using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    // ======================== 中断策略测试 ========================

    public class InterruptionPolicyTests
    {
        private static ITypedPipelineContext CreateContext() => new TestContext();

        [Fact]
        public async Task StrictPolicy_WithBrokenStep_ShouldCancelAllSubsequentSteps()
        {
            var stepA = new NullStep("A", StepFlow.Continue);
            var stepB = new NullStep("B", StepFlow.Break);
            var stepC = new NullStep("C", StepFlow.Continue, new IValueTaskStep<string>[] { stepA, stepB });
            var stepD = new NullStep("D", StepFlow.Continue);

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.Strict)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Contains(stepResults, r => r.StepKey.Value == "A");
            Assert.Contains(stepResults, r => r.StepKey.Value == "B");
            Assert.DoesNotContain(stepResults, r => r.StepKey.Value == "C");
        }

        [Fact]
        public async Task DependencyBasedPolicy_WithBrokenStep_ShouldCancelOnlyDependentSteps()
        {
            var stepA = new NullStep("A", StepFlow.Continue);
            var stepB = new NullStep("B", StepFlow.Break);
            var stepC = new NullStep("C", StepFlow.Continue, new IValueTaskStep<string>[] { stepA, stepB });
            var stepD = new NullStep("D", StepFlow.Continue);

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.DependencyBased)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Contains(stepResults, r => r.StepKey.Value == "A");
            Assert.Contains(stepResults, r => r.StepKey.Value == "B");
            Assert.Contains(stepResults, r => r.StepKey.Value == "D");
            Assert.DoesNotContain(stepResults, r => r.StepKey.Value == "C");
        }

        [Fact]
        public async Task IgnorePolicy_WithBrokenStep_ShouldExecuteAllSteps()
        {
            var stepA = new NullStep("A", StepFlow.Continue);
            var stepB = new NullStep("B", StepFlow.Break);
            var stepC = new NullStep("C", StepFlow.Continue, new IValueTaskStep<string>[] { stepA, stepB });
            var stepD = new NullStep("D", StepFlow.Continue);

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.Ignore)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Equal(4, stepResults.Count);
        }
    }
}
