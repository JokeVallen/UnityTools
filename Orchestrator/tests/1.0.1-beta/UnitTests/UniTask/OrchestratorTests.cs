using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using Orchestrator;
using Orchestrator.UniTasks;

namespace EditModeTests
{
    public class OrchestratorTests
    {
        private IEnumerator Run(Func<UniTask> test) => UniTask.ToCoroutine(test);

        // 创建测试上下文
        private ITypedPipelineContext CreateContext()
        {
            return new TypedPipelineContext();
        }

        // --------------------------------------------------
        // StepResultTests
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator StepResult_Continue_ShouldCreateContinue() => Run(async () =>
        {
            var result = StepResult.Continue();
            Assert.AreEqual(StepFlow.Continue, result.Flow);
            Assert.IsNull(result.Exception);
        });

        [UnityTest]
        public IEnumerator StepResult_Break_ShouldCreateBreak() => Run(async () =>
        {
            var result = StepResult.Break();
            Assert.AreEqual(StepFlow.Break, result.Flow);
            Assert.IsNull(result.Exception);
        });

        [UnityTest]
        public IEnumerator StepResult_Fail_ShouldCreateFail() => Run(async () =>
        {
            var ex = new InvalidOperationException("test");
            var result = StepResult.Fail(ex);
            Assert.AreEqual(StepFlow.Fail, result.Flow);
            Assert.AreEqual(ex, result.Exception);
        });

        // --------------------------------------------------
        // ExecutionResultTests
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator ExecutionResult_InitProperties() => Run(async () =>
        {
            var result = new ExecutionResult(true, TimeSpan.FromSeconds(1));
            Assert.IsTrue(result.Success);
            Assert.AreEqual(TimeSpan.FromSeconds(1), result.Duration);
        });

        [UnityTest]
        public IEnumerator StepExecutionResult_InitProperties() => Run(async () =>
        {
            var r = new StepExecutionResult<string>("Step1", true, StepFlow.Continue, new Exception(), TimeSpan.FromMilliseconds(100));
            Assert.IsTrue(r.StepKey.HasValue);
            Assert.AreEqual("Step1", r.StepKey.Value);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(StepFlow.Continue, r.Flow);
            Assert.NotNull(r.Exception);
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), r.Duration);
        });

        [UnityTest]
        public IEnumerator StepExecutionResult_WithException() => Run(async () =>
        {
            var ex = new ArgumentException("e");
            var r = new StepExecutionResult<string>("Step", false, StepFlow.Fail, ex, TimeSpan.Zero);
            Assert.AreEqual(ex, r.Exception);
        });

        // --------------------------------------------------
        // InterfaceContractTests
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator IStep_Properties() => Run(async () =>
        {
            var step = new SyncStep("TestStep");
            Assert.AreEqual("TestStep", step.Key);
            Assert.NotNull(step.Dependencies);
        });

        [UnityTest]
        public IEnumerator IStep_ExecuteAsync_ReturnsStepResult_Parallel() => Run(async () =>
        {
            var step = new SyncStep("S", "result", "42");
            var orch = UniTaskOrchestrator<string>.Builder.Create().AddStep(step).Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);
            var optional = context.Get<string, string>("result");
            Assert.IsTrue(optional.HasValue);
            Assert.AreEqual("42", optional.Value);
        });

        [UnityTest]
        public IEnumerator IStep_ExecuteAsync_ReturnsStepResult_Sequential() => Run(async () =>
        {
            var step = new SyncStep("S", "result", "42");
            var orch = UniTaskOrchestrator<string>.Builder.Create().AddStep(step).Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);
            var optional = context.Get<string, string>("result");
            Assert.IsTrue(optional.HasValue);
            Assert.AreEqual("42", optional.Value);
        });

        [UnityTest]
        public IEnumerator IBehavior_HandleAsync_Parallel() => Run(async () =>
        {
            var step = new SyncStep("S", "result", "done");
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .AddBehavior<SyncStep>(new LoggingBehavior())
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);
            Assert.IsTrue(result.Success);
        });

        [UnityTest]
        public IEnumerator IBehavior_HandleAsync_Sequential() => Run(async () =>
        {
            var step = new SyncStep("S", "result", "done");
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .AddBehavior<SyncStep>(new LoggingBehavior())
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);
            Assert.IsTrue(result.Success);
        });

        // --------------------------------------------------
        // InterruptionPolicyTests - 并行
        // --------------------------------------------------
        private (IUniTaskStep<string> A, IUniTaskStep<string> B, IUniTaskStep<string> C, IUniTaskStep<string> D) BuildInterruptionScenario()
        {
            var stepA = new SyncStep("A");
            var stepB = new BreakStep("B");
            var stepC = new SyncStep("C", "c_result", "C", new IUniTaskStep<string>[] { stepA, stepB });
            var stepD = new SyncStep("D");
            return (stepA, stepB, stepC, stepD);
        }

        [UnityTest]
        public IEnumerator StrictPolicy_ShouldCancelAllSubsequent_Parallel() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.Strict)
                .Build();

            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(2, stepResults.Count());
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "A"));
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "B"));
            Assert.IsFalse(stepResults.Any(r => r.StepKey == "C"));
            Assert.IsFalse(stepResults.Any(r => r.StepKey == "D"));
        });

        [UnityTest]
        public IEnumerator DependencyBasedPolicy_CancelOnlyDependent_Parallel() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.DependencyBased)
                .Build();

            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(3, stepResults.Count());
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "A"));
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "B"));
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "D"));
            Assert.IsFalse(stepResults.Any(r => r.StepKey == "C"));
        });

        [UnityTest]
        public IEnumerator IgnorePolicy_ExecuteAll_Parallel() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.Ignore)
                .Build();

            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(4, stepResults.Count());
        });

        // --------------------------------------------------
        // InterruptionPolicyTests - 串行
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator StrictPolicy_ShouldCancelAllSubsequent_Sequential() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.Strict)
                .Build();

            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            // 串行模式下，B 中断后后续步骤停止，只有 A 和 B 执行
            Assert.AreEqual(2, stepResults.Count());
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "A"));
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "B"));
            Assert.IsFalse(stepResults.Any(r => r.StepKey == "C"));
            Assert.IsFalse(stepResults.Any(r => r.StepKey == "D"));
        });

        [UnityTest]
        public IEnumerator DependencyBasedPolicy_CancelOnlyDependent_Sequential() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.DependencyBased)
                .Build();

            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            // 串行模式下，B 中断后，C 依赖 B 不会执行，但 D 不依赖任何中断步骤，应该继续执行
            Assert.AreEqual(3, stepResults.Count());
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "A"));
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "B"));
            Assert.IsTrue(stepResults.Any(r => r.StepKey == "D"));
            Assert.IsFalse(stepResults.Any(r => r.StepKey == "C"));
        });

        [UnityTest]
        public IEnumerator IgnorePolicy_ExecuteAll_Sequential() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.Ignore)
                .Build();

            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(4, stepResults.Count());
        });

        // --------------------------------------------------
        // OrchestratorBuilderTests
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator Build_NoSteps_Throws() => Run(async () =>
        {
            var builder = UniTaskOrchestrator<string>.Builder.Create();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        });

        [UnityTest]
        public IEnumerator AddStep_ThenBuild() => Run(async () =>
        {
            var builder = UniTaskOrchestrator<string>.Builder.Create();
            builder.AddStep(new SyncStep("S"));
            var orch = builder.Build();
            Assert.NotNull(orch);
        });

        [UnityTest]
        public IEnumerator AddBehavior_ThenBuild() => Run(async () =>
        {
            var builder = UniTaskOrchestrator<string>.Builder.Create();
            builder.AddStep(new SyncStep("S"));
            builder.AddBehavior<SyncStep>(new LoggingBehavior());
            var orch = builder.Build();
            Assert.NotNull(orch);
        });

        // --------------------------------------------------
        // OrchestratorTests - 并行
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator ExecuteAsync_SingleStep_Parallel() => Run(async () =>
        {
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(new SyncStep("S1", "result", "out"))
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            Assert.IsTrue(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(1, stepResults.Count());
            var stepResult = stepResults.First();
            Assert.IsTrue(stepResult.StepKey.HasValue);
            Assert.AreEqual("S1", stepResult.StepKey.Value);
            var res = context.Get<string, string>("result");
            Assert.IsTrue(res.HasValue);
            Assert.AreEqual("out", res.Value);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_FailingStep_Parallel() => Run(async () =>
        {
            var ex = new InvalidOperationException("fail");
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(new FailStep("F", ex))
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            Assert.IsTrue(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(1, stepResults.Count());
            Assert.IsFalse(stepResults.First().Success);
            Assert.AreEqual(ex, stepResults.First().Exception);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_BrokenStep_Strict_Parallel() => Run(async () =>
        {
            var step1 = new BreakStep("S1");
            var step2 = new SyncStep("S2", "result", "should not exec", new[] { step1 });
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step1).AddStep(step2)
                .UsePolicy(InterruptionPolicy.Strict)
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(1, stepResults.Count());
            var stepResult = stepResults.First();
            Assert.AreEqual("S1", stepResult.StepKey.Value);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_DiamondDependencies_Parallel() => Run(async () =>
        {
            var order = new List<string>();
            var stepA = new RecordStep("A", order);
            var stepB = new RecordStep("B", order, new[] { stepA });
            var stepC = new RecordStep("C", order, new[] { stepA });
            var stepD = new RecordStep("D", order, new[] { stepB, stepC });

            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA).AddStep(stepB).AddStep(stepC).AddStep(stepD)
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(4, stepResults.Count());
            Assert.Less(order.IndexOf("A"), order.IndexOf("B"));
            Assert.Less(order.IndexOf("A"), order.IndexOf("C"));
            Assert.Less(order.IndexOf("B"), order.IndexOf("D"));
            Assert.Less(order.IndexOf("C"), order.IndexOf("D"));
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_WithBehaviors_Parallel() => Run(async () =>
        {
            var order = new List<string>();
            var step = new RecordStep("Core", order);
            var b1 = new LoggingBehavior(msg => order.Add(msg));
            var b2 = new LoggingBehavior(msg => order.Add(msg));

            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .AddBehavior<RecordStep>(b1)
                .AddBehavior<RecordStep>(b2)
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, order.Count);
            Assert.AreEqual("Before", order[0]);
            Assert.AreEqual("Before", order[1]);
            Assert.AreEqual("Core", order[2]);
            Assert.AreEqual("After", order[3]);
            Assert.AreEqual("After", order[4]);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_Cancellation_Parallel() => Run(async () =>
        {
            var cts = new CancellationTokenSource();
            var step = new SlowStep("Slow", 2000);
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();
            var context = CreateContext();

            cts.Cancel();

            try
            {
                await orch.ExecuteAsyncInParallel(context, cts.Token);
                Assert.Fail("Expected cancellation");
            }
            catch (OperationCanceledException)
            {
                // 预期异常
            }
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_ComplexDAG_Parallel() => Run(async () =>
        {
            var order = new List<string>();
            var step1 = new RecordStep("1", order);
            var step2 = new RecordStep("2", order, new[] { step1 });
            var step3 = new RecordStep("3", order, new[] { step1 });
            var step4 = new RecordStep("4", order, new[] { step2, step3 });

            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step1).AddStep(step2).AddStep(step3).AddStep(step4)
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncInParallel(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(4, stepResults.Count());
            Assert.Less(order.IndexOf("1"), order.IndexOf("2"));
            Assert.Less(order.IndexOf("1"), order.IndexOf("3"));
            Assert.Less(order.IndexOf("2"), order.IndexOf("4"));
            Assert.Less(order.IndexOf("3"), order.IndexOf("4"));
        });

        // --------------------------------------------------
        // OrchestratorTests - 串行
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator ExecuteAsync_SingleStep_Sequential() => Run(async () =>
        {
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(new SyncStep("S1", "result", "out"))
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.IsTrue(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(1, stepResults.Count());
            var stepResult = stepResults.First();
            Assert.IsTrue(stepResult.StepKey.HasValue);
            Assert.AreEqual("S1", stepResult.StepKey.Value);
            var res = context.Get<string, string>("result");
            Assert.IsTrue(res.HasValue);
            Assert.AreEqual("out", res.Value);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_FailingStep_Sequential() => Run(async () =>
        {
            var ex = new InvalidOperationException("fail");
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(new FailStep("F", ex))
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.IsTrue(result.Success);
            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(1, stepResults.Count());
            Assert.IsFalse(stepResults.First().Success);
            Assert.AreEqual(ex, stepResults.First().Exception);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_BrokenStep_Strict_Sequential() => Run(async () =>
        {
            var step1 = new BreakStep("S1");
            var step2 = new SyncStep("S2", "result", "should not exec", new[] { step1 });
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step1).AddStep(step2)
                .UsePolicy(InterruptionPolicy.Strict)
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(1, stepResults.Count());
            var stepResult = stepResults.First();
            Assert.AreEqual("S1", stepResult.StepKey.Value);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_DiamondDependencies_Sequential() => Run(async () =>
        {
            var order = new List<string>();
            var stepA = new RecordStep("A", order);
            var stepB = new RecordStep("B", order, new[] { stepA });
            var stepC = new RecordStep("C", order, new[] { stepA });
            var stepD = new RecordStep("D", order, new[] { stepB, stepC });

            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA).AddStep(stepB).AddStep(stepC).AddStep(stepD)
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(4, stepResults.Count());
            // 串行模式下顺序执行，依赖关系仍然满足
            Assert.Less(order.IndexOf("A"), order.IndexOf("B"));
            Assert.Less(order.IndexOf("A"), order.IndexOf("C"));
            Assert.Less(order.IndexOf("B"), order.IndexOf("D"));
            Assert.Less(order.IndexOf("C"), order.IndexOf("D"));
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_WithBehaviors_Sequential() => Run(async () =>
        {
            var order = new List<string>();
            var step = new RecordStep("Core", order);
            var b1 = new LoggingBehavior(msg => order.Add(msg));
            var b2 = new LoggingBehavior(msg => order.Add(msg));

            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .AddBehavior<RecordStep>(b1)
                .AddBehavior<RecordStep>(b2)
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, order.Count);
            Assert.AreEqual("Before", order[0]);
            Assert.AreEqual("Before", order[1]);
            Assert.AreEqual("Core", order[2]);
            Assert.AreEqual("After", order[3]);
            Assert.AreEqual("After", order[4]);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_Cancellation_Sequential() => Run(async () =>
        {
            var cts = new CancellationTokenSource();
            var step = new SlowStep("Slow", 2000);
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .Build();
            var context = CreateContext();

            cts.Cancel();

            try
            {
                await orch.ExecuteAsyncSequentially(context, cts.Token);
                Assert.Fail("Expected cancellation");
            }
            catch (OperationCanceledException)
            {
                // 预期异常
            }
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_ComplexDAG_Sequential() => Run(async () =>
        {
            var order = new List<string>();
            var step1 = new RecordStep("1", order);
            var step2 = new RecordStep("2", order, new[] { step1 });
            var step3 = new RecordStep("3", order, new[] { step1 });
            var step4 = new RecordStep("4", order, new[] { step2, step3 });

            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step1).AddStep(step2).AddStep(step3).AddStep(step4)
                .Build();
            var context = CreateContext();
            var result = await orch.ExecuteAsyncSequentially(context, CancellationToken.None);

            var stepResults = context.GetAllStepExecutionResults<string>();
            Assert.AreEqual(4, stepResults.Count());
            Assert.Less(order.IndexOf("1"), order.IndexOf("2"));
            Assert.Less(order.IndexOf("1"), order.IndexOf("3"));
            Assert.Less(order.IndexOf("2"), order.IndexOf("4"));
            Assert.Less(order.IndexOf("3"), order.IndexOf("4"));
        });
    }
}