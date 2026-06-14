using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orchestrator.Tasks;

namespace Orchestrator.Tests.Tasks
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

            var orchestrator = Orchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.Strict)
                .SetFinalStep(stepD)          // 显式指定最终步骤，解决多汇点
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(2, result.StepResults.Count); // A and B executed, C and D cancelled
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

            var orchestrator = Orchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.DependencyBased)
                .SetFinalStep(stepD)          // 显式指定最终步骤
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(3, result.StepResults.Count); // A, B, D executed, C cancelled
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

            var orchestrator = Orchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.Ignore)
                .SetFinalStep(stepD)          // 显式指定最终步骤
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(4, result.StepResults.Count);
        }
    }
}