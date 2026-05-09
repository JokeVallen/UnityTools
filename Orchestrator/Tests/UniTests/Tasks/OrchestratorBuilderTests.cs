using System;
using Orchestrator.Tasks;

namespace Orchestrator.Tests.Tasks
{
    public class OrchestratorBuilderTests
    {
        [Fact]
        public void Build_WithNoSteps_ShouldThrowOrCreateEmptyOrchestrator()
        {
            var builder = Orchestrator<string, string>.Builder.Create();
            // 空步骤集合在最终解析时发现无汇点，抛出异常
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void AddStep_ShouldAddStepToBuilder()
        {
            var builder = Orchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("Step1");

            var result = builder.AddStep(step);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void AddBehavior_ShouldAddBehaviorToBuilder()
        {
            var builder = Orchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("Step1"); // 至少需要一个步骤
            var behavior = TestBehaviors.CreateLoggingBehavior<string, string>(_ => { });

            var result = builder.AddStep(step).AddBehavior(behavior);

            Assert.Same(builder, result);
            var orchestrator = builder.Build();
            Assert.NotNull(orchestrator);
        }

        [Fact]
        public void UsePolicy_ShouldSetInterruptionPolicy()
        {
            var builder = Orchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("Step1"); // 至少需要一个步骤

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
            // 让 Step2 依赖 Step1，形成单汇点
            var step2WithDep = new TestStep<string, string>("Step2",
                (input, token) => Task.FromResult(StepResult<string>.Continue("Step2")),
                new[] { step1 });

            var orchestrator = Orchestrator<string, string>.Builder.Create()
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
            var builder = Orchestrator<string, string>.Builder.Create();
            // 现在 Builder 已增加 null 校验，应抛出 ArgumentNullException
            Assert.Throws<ArgumentNullException>(() => builder.AddStep(null!));
        }
    }
}