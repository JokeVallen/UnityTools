using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    public class OrchestratorTests
    {
        [Fact]
        public async Task ExecuteAsync_WithSingleStep_ShouldExecuteStep()
        {
            var step = TestSteps.CreateSuccessStep("Step1", "output");
            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("output", result.Output);
            Assert.Single(result.StepResults);
            Assert.Equal("Step1", result.StepResults.First().StepName);
        }

        [Fact]
        public async Task ExecuteAsync_WithFailingStep_ShouldReturnFailedResult()
        {
            var expectedException = new InvalidOperationException("Step failed");
            var step = TestSteps.CreateFailingStep("FailingStep", expectedException);
            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.False(result.Success);
            Assert.Single(result.StepResults);
            Assert.False(result.StepResults.First().Success);
            Assert.Equal(expectedException, result.StepResults.First().Exception);
        }

        [Fact]
        public async Task ExecuteAsync_WithBrokenStep_ShouldStopExecution()
        {
            var step1 = TestSteps.CreateBrokenStep("Step1", "broken");
            var step2 = new TestStep<string, string>("Step2",
                (input, token) => new ValueTask<StepResult<string>>(StepResult<string>.Continue("should not execute")),
                new[] { step1 });

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step1)
                .AddStep(step2)
                .UsePolicy(InterruptionPolicy.Strict)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Single(result.StepResults);
            Assert.Equal("Step1", result.StepResults.First().StepName);
        }

        [Fact]
        public async Task ExecuteAsync_WithDiamondDependencies_ShouldExecuteEachStepOnce()
        {
            var executionLog = new List<string>();
            var stepA = TestSteps.CreateSuccessStep("A", "A");
            var stepB = new TestStep<string, string>("B", async (input, token) =>
            {
                executionLog.Add("B");
                return StepResult<string>.Continue("B");
            });
            var stepC = new TestStep<string, string>("C", async (input, token) =>
            {
                executionLog.Add("C");
                return StepResult<string>.Continue("C");
            });
            var stepD = new TestStep<string, string>("D",
                async (input, token) =>
                {
                    executionLog.Add("D");
                    return StepResult<string>.Continue("D");
                },
                new[] { stepB, stepC });

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .AddStep(stepD)
                .SetFinalStep(stepD)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(4, result.StepResults.Count);
            var stepDResult = result.StepResults.First(r => r.StepName == "D");
            Assert.True(stepDResult.Success);
        }

        [Fact]
        public async Task ExecuteAsync_WithBehaviors_ShouldWrapStepExecution()
        {
            var executionOrder = new List<string>();
            var step = new TestStep<string, string>("Step", async (input, token) =>
            {
                executionOrder.Add("Step");
                await Task.Delay(10);
                return StepResult<string>.Continue("output");
            });

            var behavior1 = new TestBehavior<string, string>("Behavior1",
                async (input, next, token) =>
                {
                    executionOrder.Add("Behavior1_Before");
                    var result = await next();
                    executionOrder.Add("Behavior1_After");
                    return result;
                });
            var behavior2 = new TestBehavior<string, string>("Behavior2",
                async (input, next, token) =>
                {
                    executionOrder.Add("Behavior2_Before");
                    var result = await next();
                    executionOrder.Add("Behavior2_After");
                    return result;
                });

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step)
                .AddBehavior(behavior2)
                .AddBehavior(behavior1)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(5, executionOrder.Count);
            Assert.Equal("Behavior2_Before", executionOrder[0]);
            Assert.Equal("Behavior1_Before", executionOrder[1]);
            Assert.Equal("Step", executionOrder[2]);
            Assert.Equal("Behavior1_After", executionOrder[3]);
            Assert.Equal("Behavior2_After", executionOrder[4]);
        }

        [Fact]
        public async Task ExecuteAsync_WithCancellation_ShouldThrowOperationCanceledException()
        {
            var cts = new CancellationTokenSource();
            var step = new TestStep<string, string>("SlowStep", async (input, token) =>
            {
                for (int i = 0; i < 10; i++)
                {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException(token);
                    try
                    {
                        await Task.Delay(50, token);
                    }
                    catch (TaskCanceledException)
                    {
                        throw new OperationCanceledException(token);
                    }
                }
                return StepResult<string>.Continue("completed");
            });

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step)
                .Build();

            cts.CancelAfter(30);

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => orchestrator.ExecuteAsync("input", cts.Token).AsTask());
            Assert.True(exception is OperationCanceledException);
        }

        [Fact]
        public async Task ExecuteAsync_WithDependencyBasedPolicy_ShouldContinueIndependentBranches()
        {
            var stepA = TestSteps.CreateBrokenStep("A", "break");
            var stepB = new TestStep<string, string>("B",
                async (input, token) => StepResult<string>.Continue("B"),
                new[] { stepA });
            var stepC = TestSteps.CreateSuccessStep("C", "C");
            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .AddStep(stepC)
                .UsePolicy(InterruptionPolicy.DependencyBased)
                .SetFinalStep(stepC)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Contains(result.StepResults, r => r.StepName == "C" && r.Success);
            Assert.DoesNotContain(result.StepResults, r => r.StepName == "B");
            Assert.Equal(2, result.StepResults.Count);
        }

        [Fact]
        public async Task ExecuteAsync_WithIgnorePolicy_ShouldExecuteAllSteps()
        {
            var stepA = new TestStep<string, string>("A", async (input, token) =>
            {
                return StepResult<string>.Fail(new Exception("A failed"));
            });
            var stepB = new TestStep<string, string>("B",
                async (input, token) => StepResult<string>.Continue("B success"),
                new[] { stepA });

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .UsePolicy(InterruptionPolicy.Ignore)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(2, result.StepResults.Count);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldMeasureTotalDuration()
        {
            var step1 = TestSteps.CreateSlowStep("Step1", 100);
            var step2 = new TestStep<string, string>("Step2",
                async (input, token) =>
                {
                    await Task.Delay(100, token);
                    return StepResult<string>.Continue("slow2");
                },
                new[] { step1 });

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step1)
                .AddStep(step2)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.True(result.Duration.TotalMilliseconds >= 200);
            Assert.True(result.Duration.TotalMilliseconds < 300);
        }

        [Fact]
        public async Task ExecuteAsync_WithComplexDAG_ShouldRespectDependencies()
        {
            var executionOrder = new List<string>();
            var step1 = new TestStep<string, string>("1", async (input, token) =>
            {
                executionOrder.Add("1");
                return StepResult<string>.Continue("1");
            });
            var step2 = new TestStep<string, string>("2",
                async (input, token) =>
                {
                    executionOrder.Add("2");
                    return StepResult<string>.Continue("2");
                }, new[] { step1 });
            var step3 = new TestStep<string, string>("3",
                async (input, token) =>
                {
                    executionOrder.Add("3");
                    return StepResult<string>.Continue("3");
                }, new[] { step1 });
            var step4 = new TestStep<string, string>("4",
                async (input, token) =>
                {
                    executionOrder.Add("4");
                    return StepResult<string>.Continue("4");
                }, new[] { step2, step3 });

            var orchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step1)
                .AddStep(step2)
                .AddStep(step3)
                .AddStep(step4)
                .Build();

            var result = await orchestrator.ExecuteAsync("input", CancellationToken.None);

            Assert.Equal(4, result.StepResults.Count);
            Assert.True(executionOrder.IndexOf("1") < executionOrder.IndexOf("2"));
            Assert.True(executionOrder.IndexOf("1") < executionOrder.IndexOf("3"));
            Assert.True(executionOrder.IndexOf("2") < executionOrder.IndexOf("4"));
            Assert.True(executionOrder.IndexOf("3") < executionOrder.IndexOf("4"));
        }
    }
}