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
using System.Threading.Tasks;

namespace EditModeTests
{
    public class OrchestratorTests
    {
        // 辅助：将 async 测试转换为 IEnumerator
        private IEnumerator Run(Func<UniTask> test) => UniTask.ToCoroutine(test);

        // --------------------------------------------------
        // StepResultTests (4 测试)
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator StepResult_Continue_ShouldCreateContinue() => Run(async () =>
        {
            var result = StepResult<string>.Continue("test output");
            Assert.AreEqual(StepFlow.Continue, result.Flow);
            Assert.AreEqual("test output", result.Output);
            Assert.IsNull(result.Exception);
        });

        [UnityTest]
        public IEnumerator StepResult_Break_DefaultOutput() => Run(async () =>
        {
            var result = StepResult<int>.Break();
            Assert.AreEqual(StepFlow.Break, result.Flow);
            Assert.AreEqual(default(int), result.Output);
            Assert.IsNull(result.Exception);
        });

        [UnityTest]
        public IEnumerator StepResult_Break_CustomOutput() => Run(async () =>
        {
            var result = StepResult<int>.Break(42);
            Assert.AreEqual(StepFlow.Break, result.Flow);
            Assert.AreEqual(42, result.Output);
            Assert.IsNull(result.Exception);
        });

        [UnityTest]
        public IEnumerator StepResult_Fail_WithException() => Run(async () =>
        {
            var ex = new InvalidOperationException("test");
            var result = StepResult<string>.Fail(ex);
            Assert.AreEqual(StepFlow.Fail, result.Flow);
            Assert.AreEqual(default(string), result.Output);
            Assert.AreEqual(ex, result.Exception);
        });

        // --------------------------------------------------
        // ExecutionResultTests (5 测试)
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator ExecutionResult_InitProperties() => Run(async () =>
        {
            var steps = new List<StepExecutionResult>();
            var result = new ExecutionResult<string>(true, "out", steps, TimeSpan.FromSeconds(1));
            Assert.IsTrue(result.Success);
            Assert.AreEqual("out", result.Output);
            Assert.AreEqual(steps, result.StepResults);
            Assert.AreEqual(TimeSpan.FromSeconds(1), result.Duration);
        });

        [UnityTest]
        public IEnumerator ExecutionResult_NullOutput() => Run(async () =>
        {
            var result = new ExecutionResult<string>(true, default, new List<StepExecutionResult>(), TimeSpan.Zero);
            Assert.IsNull(result.Output);
        });

        [UnityTest]
        public IEnumerator StepExecutionResult_InitProperties() => Run(async () =>
        {
            var r = new StepExecutionResult("Step1", true, StepFlow.Continue, "out", new Exception(), TimeSpan.FromMilliseconds(100));
            Assert.AreEqual("Step1", r.StepName);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(StepFlow.Continue, r.Flow);
            Assert.AreEqual("out", r.Output);
            Assert.NotNull(r.Exception);
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), r.Duration);
        });

        [UnityTest]
        public IEnumerator StepExecutionResult_NullOutput() => Run(async () =>
        {
            var r = new StepExecutionResult("Step", true, StepFlow.Continue, null, null, TimeSpan.Zero);
            Assert.IsNull(r.Output);
        });

        [UnityTest]
        public IEnumerator StepExecutionResult_WithException() => Run(async () =>
        {
            var ex = new ArgumentException("e");
            var r = new StepExecutionResult("Step", false, StepFlow.Fail, null, ex, TimeSpan.Zero);
            Assert.AreEqual(ex, r.Exception);
        });

        // --------------------------------------------------
        // InterfaceContractTests (5 测试) – 直接使用真实实现代替 Mock
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator IStep_Properties() => Run(async () =>
        {
            var step = TestSteps.CreateSuccessStep("TestStep", "ok");
            Assert.AreEqual("TestStep", step.Name);
            Assert.NotNull(step.Dependencies);
        });

        [UnityTest]
        public IEnumerator IStep_ExecuteAsync_ReturnsStepResult() => Run(async () =>
        {
            var step = TestSteps.CreateSuccessStep("S", "42");
            var orch = UniTaskOrchestrator<string, string>.Builder.Create().AddStep(step).Build();
            var result = await orch.ExecuteAsync("test", CancellationToken.None);
            Assert.AreEqual("42", result.Output);
            Assert.IsTrue(result.Success);
        });

        [UnityTest]
        public IEnumerator IBehavior_HandleAsync() => Run(async () =>
        {
            var behavior = TestBehaviors.CreateLoggingBehavior<string, string>(_ => { });
            var step = TestSteps.CreateSuccessStep("S", "done");
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step).AddBehavior(behavior).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.IsTrue(result.Success);
        });

        [UnityTest]
        public IEnumerator IExecutionResult_Info() => Run(async () =>
        {
            var step = TestSteps.CreateSuccessStep("A", "final");
            var orch = UniTaskOrchestrator<string, string>.Builder.Create().AddStep(step).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("final", result.Output);
            Assert.AreEqual(1, result.StepResults.Count);
            Assert.Greater(result.Duration.TotalMilliseconds, 0);
        });

        [UnityTest]
        public IEnumerator IStepExecutionResult_Details() => Run(async () =>
        {
            var step = TestSteps.CreateSuccessStep("S1", "output");
            var orch = UniTaskOrchestrator<string, string>.Builder.Create().AddStep(step).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            var sr = result.StepResults.First();
            Assert.AreEqual("S1", sr.StepName);
            Assert.IsTrue(sr.Success);
            Assert.AreEqual(StepFlow.Continue, sr.Flow);
            Assert.AreEqual("output", sr.Output);
            Assert.IsNull(sr.Exception);
        });

        // --------------------------------------------------
        // InterruptionPolicyTests (3 测试)
        // --------------------------------------------------
        private (IUniTaskStep<string, string> A, IUniTaskStep<string, string> B, IUniTaskStep<string, string> C, IUniTaskStep<string, string> D) BuildInterruptionScenario()
        {
            var stepA = TestSteps.CreateSuccessStep("A", "A");
            var stepB = TestSteps.CreateBrokenStep("B", "break");
            var stepC = TestSteps.CreateStepWithDependencies("C", () => UniTask.FromResult(StepResult<string>.Continue("C")), stepA, stepB);
            var stepD = TestSteps.CreateSuccessStep("D", "D");
            return (stepA, stepB, stepC, stepD);
        }

        [UnityTest]
        public IEnumerator StrictPolicy_ShouldCancelAllSubsequent() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.Strict).SetFinalStep(D).Build();

            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.AreEqual(2, result.StepResults.Count);
            CollectionAssert.Contains(result.StepResults.Select(r => r.StepName), "A");
            CollectionAssert.Contains(result.StepResults.Select(r => r.StepName), "B");
            CollectionAssert.DoesNotContain(result.StepResults.Select(r => r.StepName), "C");
            CollectionAssert.DoesNotContain(result.StepResults.Select(r => r.StepName), "D");
        });

        [UnityTest]
        public IEnumerator DependencyBasedPolicy_CancelOnlyDependent() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.DependencyBased).SetFinalStep(D).Build();

            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.AreEqual(3, result.StepResults.Count);
            CollectionAssert.Contains(result.StepResults.Select(r => r.StepName), "A");
            CollectionAssert.Contains(result.StepResults.Select(r => r.StepName), "B");
            CollectionAssert.Contains(result.StepResults.Select(r => r.StepName), "D");
            CollectionAssert.DoesNotContain(result.StepResults.Select(r => r.StepName), "C");
        });

        [UnityTest]
        public IEnumerator IgnorePolicy_ExecuteAll() => Run(async () =>
        {
            var (A, B, C, D) = BuildInterruptionScenario();
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(A).AddStep(B).AddStep(C).AddStep(D)
                .UsePolicy(InterruptionPolicy.Ignore).SetFinalStep(D).Build();

            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.AreEqual(4, result.StepResults.Count);
        });

        // --------------------------------------------------
        // OrchestratorBuilderTests (5 测试)
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator Build_NoSteps_Throws() => Run(async () =>
        {
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        });

        [UnityTest]
        public IEnumerator AddStep_ThenBuild() => Run(async () =>
        {
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("S");
            builder.AddStep(step);
            var orch = builder.Build();
            Assert.NotNull(orch);
        });

        [UnityTest]
        public IEnumerator AddBehavior_ThenBuild() => Run(async () =>
        {
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("S");
            var behavior = TestBehaviors.CreateLoggingBehavior<string, string>(_ => { });
            builder.AddStep(step).AddBehavior(behavior);
            var orch = builder.Build();
            Assert.NotNull(orch);
        });

        [UnityTest]
        public IEnumerator UsePolicy_ThenBuild() => Run(async () =>
        {
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
            var step = TestSteps.CreateSuccessStep("S");
            builder.AddStep(step).UsePolicy(InterruptionPolicy.Strict);
            var orch = builder.Build();
            Assert.NotNull(orch);
        });

        [UnityTest]
        public IEnumerator Build_MultipleStepsAndBehaviors() => Run(async () =>
        {
            var step1 = TestSteps.CreateSuccessStep("Step1");
            var step2 = TestSteps.CreateStepWithDependencies("Step2",
                () => UniTask.FromResult(StepResult<string>.Continue("out")), step1);
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step1).AddStep(step2)
                .AddBehavior(TestBehaviors.CreateLoggingBehavior<string, string>(_ => { }))
                .AddBehavior(TestBehaviors.CreateTimingBehavior<string, string>(_ => { }))
                .Build();
            Assert.NotNull(orch);
        });

        // --------------------------------------------------
        // OrchestratorTests (11 测试) – 已包含上面部分重复，此处补齐剩余
        // --------------------------------------------------
        [UnityTest]
        public IEnumerator ExecuteAsync_SingleStep() => Run(async () =>
        {
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(TestSteps.CreateSuccessStep("S1", "out")).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("out", result.Output);
            Assert.AreEqual(1, result.StepResults.Count);
            Assert.AreEqual("S1", result.StepResults.First().StepName);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_FailingStep() => Run(async () =>
        {
            var ex = new InvalidOperationException("fail");
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(TestSteps.CreateFailingStep("F", ex)).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.StepResults.Count);
            Assert.IsFalse(result.StepResults.First().Success);
            Assert.AreEqual(ex, result.StepResults.First().Exception);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_BrokenStep_Strict() => Run(async () =>
        {
            var step1 = TestSteps.CreateBrokenStep("S1", "broken");
            var step2 = TestSteps.CreateStepWithDependencies("S2",
                () => UniTask.FromResult(StepResult<string>.Continue("should not exec")), step1);
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step1).AddStep(step2).UsePolicy(InterruptionPolicy.Strict).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.AreEqual(1, result.StepResults.Count);
            Assert.AreEqual("S1", result.StepResults.First().StepName);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_DiamondDependencies() => Run(async () =>
        {
            var log = new List<string>();
            var stepA = TestSteps.CreateSuccessStep("A", "A");
            var stepB = new TestStep<string, string>("B", async (input, token) => { log.Add("B"); return StepResult<string>.Continue("B"); });
            var stepC = new TestStep<string, string>("C", async (input, token) => { log.Add("C"); return StepResult<string>.Continue("C"); });
            var stepD = new TestStep<string, string>("D",
                async (input, token) => { log.Add("D"); return StepResult<string>.Continue("D"); },
                new IUniTaskStep<string, string>[] { stepB, stepC });

            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA).AddStep(stepB).AddStep(stepC).AddStep(stepD)
                .SetFinalStep(stepD).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.AreEqual(4, result.StepResults.Count);
            Assert.IsTrue(result.StepResults.Any(r => r.StepName == "D" && r.Success));
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_WithBehaviors() => Run(async () =>
        {
            var order = new List<string>();

            // 核心步骤（同步，不加延迟）
            var step = new TestStep<string, string>("Core", (input, token) =>
            {
                order.Add("Core");
                return UniTask.FromResult(StepResult<string>.Continue("ok"));
            });

            var b1 = new TestBehavior<string, string>("Outer", (input, next, token) =>
            {
                order.Add("OuterBefore");
                var r = next();
                order.Add("OuterAfter");
                return r; // 注意：直接转发 UniTask，不 await
            });
            var b2 = new TestBehavior<string, string>("Inner", (input, next, token) =>
            {
                order.Add("InnerBefore");
                var r = next();
                order.Add("InnerAfter");
                return r;
            });

            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step).AddBehavior(b1).AddBehavior(b2).Build();

            var result = await orch.ExecuteAsync("in", CancellationToken.None);

            Assert.IsTrue(result.Success, $"Expected Success but got {result.StepResults.First().Flow}");
            Assert.AreEqual(5, order.Count);
            Assert.AreEqual("OuterBefore", order[0]);
            Assert.AreEqual("InnerBefore", order[1]);
            Assert.AreEqual("Core", order[2]);
            Assert.AreEqual("InnerAfter", order[3]);
            Assert.AreEqual("OuterAfter", order[4]);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_Cancellation() => Run(async () =>
        {
            var cts = new CancellationTokenSource();
            var step = new TestStep<string, string>("Slow", async (input, token) =>
            {
                // 使用 Task.Delay 确保真实延迟，让取消令牌有时间生效
                for (int i = 0; i < 20; i++)
                {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException(token);
                    await Task.Delay(100, token);
                }
                return StepResult<string>.Continue("done");
            });
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step).Build();

            cts.CancelAfter(30); // 30ms 后取消

            try
            {
                await orch.ExecuteAsync("in", cts.Token);
                Assert.Fail("Expected cancellation");
            }
            catch (OperationCanceledException) { }
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_DependencyBased_ContinueIndependent() => Run(async () =>
        {
            var stepA = TestSteps.CreateBrokenStep("A", "break");
            var stepB = new TestStep<string, string>("B", (input, token) => UniTask.FromResult(StepResult<string>.Continue("B")), new[] { stepA });
            var stepC = TestSteps.CreateSuccessStep("C", "C");
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA).AddStep(stepB).AddStep(stepC)
                .UsePolicy(InterruptionPolicy.DependencyBased).SetFinalStep(stepC).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.AreEqual(2, result.StepResults.Count); // A and C, B is cancelled
            CollectionAssert.Contains(result.StepResults.Select(r => r.StepName), "C");
            CollectionAssert.DoesNotContain(result.StepResults.Select(r => r.StepName), "B");
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_IgnorePolicy_AllExecute() => Run(async () =>
        {
            var stepA = new TestStep<string, string>("A", (input, token) => UniTask.FromResult(StepResult<string>.Fail(new Exception("fail"))));
            var stepB = new TestStep<string, string>("B", (input, token) => UniTask.FromResult(StepResult<string>.Continue("B")), new[] { stepA });
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA).AddStep(stepB).UsePolicy(InterruptionPolicy.Ignore).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.AreEqual(2, result.StepResults.Count);
            Assert.IsFalse(result.Success);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_MeasureTotalDuration() => Run(async () =>
        {
            var step1 = new TestStep<string, string>("S1", async (input, token) =>
            {
                await Task.Delay(100, token);  // 使用 Task.Delay
                return StepResult<string>.Continue("ok");
            });
            var step2 = new TestStep<string, string>("S2", async (input, token) =>
            {
                await Task.Delay(100, token);  // 使用 Task.Delay
                return StepResult<string>.Continue("ok");
            }, new IUniTaskStep<string, string>[] { step1 });

            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step1).AddStep(step2).Build();

            var result = await orch.ExecuteAsync("in", CancellationToken.None);

            // 允许 -30ms ~ +50ms 的误差
            Assert.GreaterOrEqual(result.Duration.TotalMilliseconds, 170,
                $"Duration too short: {result.Duration.TotalMilliseconds}ms");
            Assert.Less(result.Duration.TotalMilliseconds, 350,
                $"Duration too long: {result.Duration.TotalMilliseconds}ms");
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_ComplexDAG() => Run(async () =>
        {
            var order = new List<string>();
            var step1 = new TestStep<string, string>("1", async (inp, token) => { order.Add("1"); return StepResult<string>.Continue("1"); });
            var step2 = new TestStep<string, string>("2", async (inp, token) => { order.Add("2"); return StepResult<string>.Continue("2"); }, new[] { step1 });
            var step3 = new TestStep<string, string>("3", async (inp, token) => { order.Add("3"); return StepResult<string>.Continue("3"); }, new[] { step1 });
            var step4 = new TestStep<string, string>("4", async (inp, token) => { order.Add("4"); return StepResult<string>.Continue("4"); }, new[] { step2, step3 });
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step1).AddStep(step2).AddStep(step3).AddStep(step4).Build();
            var result = await orch.ExecuteAsync("in", CancellationToken.None);
            Assert.AreEqual(4, result.StepResults.Count);
            Assert.Less(order.IndexOf("1"), order.IndexOf("2"));
            Assert.Less(order.IndexOf("1"), order.IndexOf("3"));
            Assert.Less(order.IndexOf("2"), order.IndexOf("4"));
            Assert.Less(order.IndexOf("3"), order.IndexOf("4"));
        });
    }
}