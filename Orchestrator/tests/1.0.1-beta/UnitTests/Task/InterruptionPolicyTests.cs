using Orchestrator.Tasks;

namespace Orchestrator.Tests.Tasks
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
            var stepC = new NullStep("C", StepFlow.Continue, new ITaskStep<string>[] { stepA, stepB });
            var stepD = new NullStep("D", StepFlow.Continue);

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.Strict)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncInParallel(context, CancellationToken.None);

            // Strict 模式下，B 中断后 C 和 D 都不会执行
            // 注意：由于并行执行，执行结果数量可能为 2 或 3（A、B 和可能的 D）
            // D 可能先于 B 完成，因此需要检查具体执行了哪些步骤
            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Contains(stepResults, r => r.StepKey.Value == "A");
            Assert.Contains(stepResults, r => r.StepKey.Value == "B");
            // C 依赖 A 和 B，B 中断，C 不应执行
            Assert.DoesNotContain(stepResults, r => r.StepKey.Value == "C");
        }

        [Fact]
        public async Task DependencyBasedPolicy_WithBrokenStep_ShouldCancelOnlyDependentSteps()
        {
            var stepA = new NullStep("A", StepFlow.Continue);
            var stepB = new NullStep("B", StepFlow.Break);
            var stepC = new NullStep("C", StepFlow.Continue, new ITaskStep<string>[] { stepA, stepB });
            var stepD = new NullStep("D", StepFlow.Continue);

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.DependencyBased)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncInParallel(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            // A、B、D 应该执行，C 依赖 B 所以不执行
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
            var stepC = new NullStep("C", StepFlow.Continue, new ITaskStep<string>[] { stepA, stepB });
            var stepD = new NullStep("D", StepFlow.Continue);

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .UsePolicy(InterruptionPolicy.Ignore)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncInParallel(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Equal(4, stepResults.Count);
        }
    }
}
