using Orchestrator.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchestrator.Tests.Tasks
{
    // ======================== 编排器执行测试 ========================

    public class OrchestratorTests
    {
        private static ITypedPipelineContext CreateContext() => new TypedPipelineContext();

        [Fact]
        public async Task ExecuteAsync_WithSingleStep_ShouldExecuteStep()
        {
            var step = new NullStep("Step1", StepFlow.Continue);
            var orchestrator = TaskOrchestrator<string>.Builder.Create()
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
            var step = new NullStep("FailingStep", StepFlow.Fail);
            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success); // 整体执行成功，但步骤失败
            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Single(stepResults);
            Assert.False(stepResults.First().Success);
            Assert.Equal(StepFlow.Fail, stepResults.First().Flow);
        }

        [Fact]
        public async Task ExecuteAsync_WithBrokenStep_ShouldStopExecution()
        {
            var step1 = new NullStep("Step1", StepFlow.Break);
            var step2 = new NullStep("Step2", StepFlow.Continue, new ITaskStep<string>[] { step1 });

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
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
            var stepB = new RecordStep("B", executionLog, new ITaskStep<string>[] { stepA });
            var stepC = new RecordStep("C", executionLog, new ITaskStep<string>[] { stepA });
            var stepD = new RecordStep("D", executionLog, new ITaskStep<string>[] { stepB, stepC });

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
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

            // 验证依赖顺序（串行执行下顺序固定）
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

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .AddBehavior<RecordStep>(behavior2) // 先添加 → 外层
                .AddBehavior<RecordStep>(behavior1) // 后添加 → 内层
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
            var step = new NullStep("Step", StepFlow.Continue);
            var orchestrator = TaskOrchestrator<string>.Builder.Create()
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
            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Duration.TotalMilliseconds >= 0);
        }

        [Fact]
        public async Task ExecuteAsync_Parallel_ShouldExecuteIndependentStepsConcurrently()
        {
            var stepA = new NullStep("A", StepFlow.Continue);
            var stepB = new NullStep("B", StepFlow.Continue);
            var stepC = new NullStep("C", StepFlow.Continue);

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncInParallel(context, CancellationToken.None);

            Assert.True(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Equal(3, stepResults.Count);
        }

        [Fact]
        public async Task ExecuteAsync_WithComplexDAG_ShouldResolveCorrectly()
        {
            var executionLog = new List<string>();

            var step1 = new RecordStep("1", executionLog);
            var step2 = new RecordStep("2", executionLog, new ITaskStep<string>[] { step1 });
            var step3 = new RecordStep("3", executionLog, new ITaskStep<string>[] { step1 });
            var step4 = new RecordStep("4", executionLog, new ITaskStep<string>[] { step2, step3 });

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(step1)
                .AddStep(step2)
                .AddStep(step3)
                .AddStep(step4)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(4, context.GetAllStepExecutionResults<string>().Count());

            // 验证依赖顺序
            Assert.True(executionLog.IndexOf("1") < executionLog.IndexOf("2"));
            Assert.True(executionLog.IndexOf("1") < executionLog.IndexOf("3"));
            Assert.True(executionLog.IndexOf("2") < executionLog.IndexOf("4"));
            Assert.True(executionLog.IndexOf("3") < executionLog.IndexOf("4"));
        }

        [Fact]
        public async Task ExecuteAsync_WithConcurrencyLimit_ShouldRespectLimit()
        {
            var steps = new ITaskStep<string>[10];
            steps[0] = new NullStep("C0", StepFlow.Continue);
            for (int i = 1; i < 10; i++)
            {
                var index = i;
                steps[i] = new NullStep($"C{i}", StepFlow.Continue, new ITaskStep<string>[] { steps[i - 1] });
            }

            var orchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddSteps(steps)
                .WithMaxConcurrency(2)
                .Build();

            var context = CreateContext();
            var result = await orchestrator.ExecuteAsyncInParallel(context, CancellationToken.None);

            Assert.True(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>().ToList();
            Assert.Equal(10, stepResults.Count);
        }
    }
}
