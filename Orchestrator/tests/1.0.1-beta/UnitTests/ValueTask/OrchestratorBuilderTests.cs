using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    // ======================== 编排器构建器测试 ========================

    public class OrchestratorBuilderTests
    {
        [Fact]
        public void Build_WithNoSteps_ShouldThrow()
        {
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void AddStep_ShouldAddStepToBuilder()
        {
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            var step = new NullStep("Step1", StepFlow.Continue);

            var result = builder.AddStep(step);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddBehavior_ShouldAddBehaviorToBuilder()
        {
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            var step = new NullStep("Step1", StepFlow.Continue);
            var behavior = new NullBehavior();

            var result = builder.AddStep(step).AddBehavior<NullStep>(behavior);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddBehaviorForAll_ShouldAddBehaviorToAllSteps()
        {
            var step1 = new NullStep("Step1", StepFlow.Continue);
            var step2 = new NullStep("Step2", StepFlow.Continue);
            var behavior = new NullBehavior();
            var builder = ValueTaskOrchestrator<string>.Builder.Create();

            var result = builder.AddStep(step1).AddStep(step2).AddBehaviorForAll(behavior);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void UsePolicy_ShouldSetInterruptionPolicy()
        {
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            var step = new NullStep("Step1", StepFlow.Continue);

            var result = builder.AddStep(step).UsePolicy(InterruptionPolicy.Strict);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void WithMaxConcurrency_ShouldSetConcurrencyLimit()
        {
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            var step = new NullStep("Step1", StepFlow.Continue);

            var result = builder.AddStep(step).WithMaxConcurrency(3);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddStep_WithNullStep_ShouldThrow()
        {
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            Assert.Throws<ArgumentNullException>(() => builder.AddStep(null!));
        }

        [Fact]
        public void AddBehavior_WithNullBehavior_ShouldThrow()
        {
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            var step = new NullStep("Step1", StepFlow.Continue);
            builder.AddStep(step);

            Assert.Throws<ArgumentNullException>(() => builder.AddBehavior<NullStep>(null!));
        }

        [Fact]
        public void Build_ShouldNotBeCallableTwice()
        {
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            var step = new NullStep("Step1", StepFlow.Continue);
            builder.AddStep(step);

            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }
    }
}
