using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    public class InterruptionPolicyTests
    {
        [Fact]
        public async Task StrictPolicy_WithBrokenStep_ShouldCancelAllSubsequentSteps()
        {
            var stepA = TestSteps.CreateSuccessStep("A", "A");
            var stepB = TestSteps.CreateBrokenStep("B", "break");
            var stepC = new TestStep<string, string>("C",
                async (input, token) => StepResult<string>.Continue("C"),
                new[] { stepA, stepB });
            var stepD = TestSteps.CreateSuccessStep("D", "D");

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.Strict)
                .SetFinalStep(stepD)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(2, result.StepResults.Count);
            Assert.Contains(result.StepResults, r => r.StepName == "A");
            Assert.Contains(result.StepResults, r => r.StepName == "B");
            Assert.DoesNotContain(result.StepResults, r => r.StepName == "C");
            Assert.DoesNotContain(result.StepResults, r => r.StepName == "D");
        }

        [Fact]
        public async Task DependencyBasedPolicy_WithBrokenStep_ShouldCancelOnlyDependentSteps()
        {
            var stepA = TestSteps.CreateSuccessStep("A", "A");
            var stepB = TestSteps.CreateBrokenStep("B", "break");
            var stepC = new TestStep<string, string>("C",
                async (input, token) => StepResult<string>.Continue("C"),
                new[] { stepA, stepB });
            var stepD = TestSteps.CreateSuccessStep("D", "D");

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.DependencyBased)
                .SetFinalStep(stepD)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(3, result.StepResults.Count);
            Assert.Contains(result.StepResults, r => r.StepName == "A");
            Assert.Contains(result.StepResults, r => r.StepName == "B");
            Assert.Contains(result.StepResults, r => r.StepName == "D");
            Assert.DoesNotContain(result.StepResults, r => r.StepName == "C");
        }

        [Fact]
        public async Task IgnorePolicy_WithBrokenStep_ShouldExecuteAllSteps()
        {
            var stepA = TestSteps.CreateSuccessStep("A", "A");
            var stepB = TestSteps.CreateBrokenStep("B", "break");
            var stepC = new TestStep<string, string>("C",
                async (input, token) => StepResult<string>.Continue("C"),
                new[] { stepA, stepB });
            var stepD = TestSteps.CreateSuccessStep("D", "D");

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.Ignore)
                .SetFinalStep(stepD)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(4, result.StepResults.Count);
        }
    }
}