using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    public class OrchestratorBuilderTests
    {
        [Fact]
        public void Build_WithNoSteps_ShouldThrowOrCreateEmptyOrchestrator()
        {
            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void AddStep_ShouldAddStepToBuilder()
        {
            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("Step1");
            var result = builder.AddStep(step);
            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddBehavior_ShouldAddBehaviorToBuilder()
        {
            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("Step1");
            var behavior = TestBehaviors.CreateLoggingBehavior<string, string>(_ => { });
            var result = builder.AddStep(step).AddBehavior(behavior);
            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void UsePolicy_ShouldSetInterruptionPolicy()
        {
            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("Step1");
            var result = builder.AddStep(step).UsePolicy(InterruptionPolicy.Strict);
            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void Build_WithMultipleStepsAndBehaviors_ShouldCreateConfiguredOrchestrator()
        {
            var step1 = TestSteps.CreateSuccessStep("Step1");
            var step2 = TestSteps.CreateSuccessStep("Step2");
            var step2WithDep = new TestStep<string, string>("Step2",
                (input, token) => new ValueTask<StepResult<string>>(StepResult<string>.Continue("Step2")),
                new[] { step1 });

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step1)
                .AddStep(step2WithDep)
                .AddBehavior(TestBehaviors.CreateLoggingBehavior<string, string>(_ => { }))
                .AddBehavior(TestBehaviors.CreateTimingBehavior<string, string>(_ => { }))
                .UsePolicy(InterruptionPolicy.DependencyBased)
                .Build();

            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddStep_WithNullStep_ShouldThrowOrAccept()
        {
            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            Assert.Throws<ArgumentNullException>(() => builder.AddStep(null!));
        }
    }
}