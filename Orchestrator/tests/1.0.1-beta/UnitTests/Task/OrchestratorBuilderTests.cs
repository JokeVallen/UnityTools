using Orchestrator.Tasks;

namespace Orchestrator.Tests.Tasks
{
    // ======================== 编排器构建器测试 ========================

    public class OrchestratorBuilderTests
    {
        [Fact]
        public void Build_WithNoSteps_ShouldThrow()
        {
            var builder = TaskOrchestrator<string>.Builder.Create();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void AddStep_ShouldAddStepToBuilder()
        {
            var step = new NullStep("Step1");
            var builder = TaskOrchestrator<string>.Builder.Create();

            var result = builder.AddStep(step);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddBehavior_ShouldAddBehaviorToBuilder()
        {
            var step = new NullStep("Step1");
            var behavior = new NullBehavior();
            var builder = TaskOrchestrator<string>.Builder.Create();

            var result = builder.AddStep(step).AddBehavior<NullStep>(behavior);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddBehaviorForAll_ShouldAddBehaviorToAllSteps()
        {
            var step1 = new NullStep("Step1");
            var step2 = new NullStep("Step2");
            var behavior = new NullBehavior();
            var builder = TaskOrchestrator<string>.Builder.Create();

            var result = builder.AddStep(step1).AddStep(step2).AddBehaviorForAll(behavior);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void UsePolicy_ShouldSetInterruptionPolicy()
        {
            var step = new NullStep("Step1");
            var builder = TaskOrchestrator<string>.Builder.Create();

            var result = builder.AddStep(step).UsePolicy(InterruptionPolicy.Strict);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void WithMaxConcurrency_ShouldSetConcurrencyLimit()
        {
            var step = new NullStep("Step1");
            var builder = TaskOrchestrator<string>.Builder.Create();

            var result = builder.AddStep(step).WithMaxConcurrency(3);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddStep_WithNullStep_ShouldThrow()
        {
            var builder = TaskOrchestrator<string>.Builder.Create();

            Assert.Throws<ArgumentNullException>(() => builder.AddStep(null!));
        }

        [Fact]
        public void AddBehavior_WithNullBehavior_ShouldThrow()
        {
            var step = new NullStep("Step1");
            var builder = TaskOrchestrator<string>.Builder.Create().AddStep(step);

            Assert.Throws<ArgumentNullException>(() => builder.AddBehavior<NullStep>(null!));
        }

        [Fact]
        public void Build_ShouldNotBeCallableTwice()
        {
            var step = new NullStep("Step1");
            var builder = TaskOrchestrator<string>.Builder.Create().AddStep(step);
            var orchestrator = builder.Build();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }
    }
}
