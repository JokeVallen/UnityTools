using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    // ======================== 编排器执行测试 ========================

    public class OrchestratorTests
    {
        private static ITypedPipelineContext CreateContext() => new TypedPipelineContext();

        [Fact]
        public async Task ExecuteAsync_WithSingleStep_ShouldExecuteStep()
        {
            var step = new NullStep("Step1", StepFlow.Continue);
            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Single(stepResults);
            Assert.Equal("Step1", stepResults.First().StepKey.Value);
            Assert.True(stepResults.First().Success);
        }

        [Fact]
        public async Task ExecuteAsync_WithFailingStep_ShouldReturnFailedResult()
        {
            var expectedException = new InvalidOperationException("Step failed");
            var step = new FailStep("FailingStep", expectedException);
            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Single(stepResults);
            Assert.False(stepResults.First().Success);
            Assert.Equal(StepFlow.Fail, stepResults.First().Flow);
            Assert.Equal(expectedException, stepResults.First().Exception);
        }

        [Fact]
        public async Task ExecuteAsync_WithBrokenStep_ShouldStopExecution()
        {
            var step1 = new NullStep("Step1", StepFlow.Break);
            var step2 = new NullStep("Step2", StepFlow.Continue, new IValueTaskStep<string>[] { step1 });

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step1)
                .AddStep(step2)
                .UsePolicy(InterruptionPolicy.Strict)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Single(stepResults);
            Assert.Equal("Step1", stepResults.First().StepKey.Value);
        }

        [Fact]
        public async Task ExecuteAsync_WithDiamondDependencies_ShouldRespectOrder()
        {
            var executionLog = new List<string>();

            var stepA = new RecordStep("A", executionLog);
            var stepB = new RecordStep("B", executionLog, new IValueTaskStep<string>[] { stepA });
            var stepC = new RecordStep("C", executionLog, new IValueTaskStep<string>[] { stepA });
            var stepD = new RecordStep("D", executionLog, new IValueTaskStep<string>[] { stepB, stepC });

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Equal(4, stepResults.Count);

            Assert.True(executionLog.IndexOf("A") < executionLog.IndexOf("B"));
            Assert.True(executionLog.IndexOf("A") < executionLog.IndexOf("C"));
            Assert.True(executionLog.IndexOf("B") < executionLog.IndexOf("D"));
            Assert.True(executionLog.IndexOf("C") < executionLog.IndexOf("D"));
        }

        [Fact]
        public async Task ExecuteAsync_WithBehaviors_ShouldWrapStepExecution()
        {
            var executionLog = new List<string>();
            var step = new RecordStep("Core", executionLog);
            var behavior1 = new RecordBehavior("B1", executionLog);
            var behavior2 = new RecordBehavior("B2", executionLog);

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .AddBehavior<RecordStep>(behavior2)
                .AddBehavior<RecordStep>(behavior1)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(5, executionLog.Count);
            Assert.Equal("B2_Before", executionLog[0]);
            Assert.Equal("B1_Before", executionLog[1]);
            Assert.Equal("Core", executionLog[2]);
            Assert.Equal("B1_After", executionLog[3]);
            Assert.Equal("B2_After", executionLog[4]);
        }

        [Fact]
        public async Task ExecuteAsync_WithCancellation_ShouldThrow()
        {
            var cts = new CancellationTokenSource();
            var step = new SlowStep("SlowStep", 1000);
            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await orchestrator.ExecuteAsyncSequentially(CreateContext(), cts.Token));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldMeasureTotalDuration()
        {
            var step = new NullStep("Step", StepFlow.Continue);
            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Duration.TotalMilliseconds >= 0);
        }

        [Fact]
        public async Task ExecuteAsync_WithComplexDAG_ShouldResolveCorrectly()
        {
            var executionLog = new List<string>();

            var step1 = new RecordStep("1", executionLog);
            var step2 = new RecordStep("2", executionLog, new IValueTaskStep<string>[] { step1 });
            var step3 = new RecordStep("3", executionLog, new IValueTaskStep<string>[] { step1 });
            var step4 = new RecordStep("4", executionLog, new IValueTaskStep<string>[] { step2, step3 });

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step1)
                .AddStep(step2)
                .AddStep(step3)
                .AddStep(step4)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(4, context.GetAllStepExecutionResults<string>().Count());

            Assert.True(executionLog.IndexOf("1") < executionLog.IndexOf("2"));
            Assert.True(executionLog.IndexOf("1") < executionLog.IndexOf("3"));
            Assert.True(executionLog.IndexOf("2") < executionLog.IndexOf("4"));
            Assert.True(executionLog.IndexOf("3") < executionLog.IndexOf("4"));
        }

        [Fact]
        public async Task ExecuteAsync_WithContextData_ShouldPassDataBetweenSteps()
        {
            var step1 = new SetDataStep("Step1", "key", 42);
            var step2 = new GetDataStep("Step2", "key", 42, new IValueTaskStep<string>[] { step1 });

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step1)
                .AddStep(step2)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(2, context.GetAllStepExecutionResults<string>().Count());
        }

        [Fact]
        public async Task ExecuteAsync_WithIndependentSteps_ShouldExecuteAll()
        {
            var stepA = new NullStep("A", StepFlow.Continue);
            var stepB = new NullStep("B", StepFlow.Continue);
            var stepC = new NullStep("C", StepFlow.Continue);

            var orchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Equal(3, stepResults.Count);
        }
    }
}
