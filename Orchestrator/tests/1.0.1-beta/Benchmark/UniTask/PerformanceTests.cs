using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Profiling;
using UnityEngine.TestTools;
using Orchestrator;
using Orchestrator.UniTasks;
using Cysharp.Threading.Tasks;
using System.Threading;

public class UniTaskOrchestratorPerformanceTests
{
    private ITypedPipelineContext CreateContext() => new EmptyContext();
    private CancellationTokenSource cts;

    [SetUp]
    public void SetUp()
    {
        cts = new CancellationTokenSource();
    }

    [TearDown]
    public void TearDown()
    {
        cts?.Dispose();
    }

    // ======================== 辅助方法 ========================

    private struct MeasureResult
    {
        public double ElapsedMs;
        public long AllocatedBytes;
    }

    private IEnumerator MeasureAsync(UniTask task, System.Action<MeasureResult> callback)
    {
        using var recorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Memory,
            "GC Allocated In Frame");

        long allocatedBytes = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var coroutine = task.ToCoroutine();
        while (coroutine.MoveNext())
        {
            yield return coroutine.Current;
            allocatedBytes += recorder.CurrentValue;
            recorder.Reset();
        }

        sw.Stop();
        allocatedBytes += recorder.CurrentValue;

        callback(new MeasureResult
        {
            ElapsedMs = sw.Elapsed.TotalMilliseconds,
            AllocatedBytes = allocatedBytes
        });
    }

    private IEnumerator RunParallelPerformanceTest(
        string testName,
        UniTaskOrchestrator<string> orchestrator,
        int measurementCount = 30)
    {
        var context = CreateContext();
        var timeGroup = new SampleGroup($"{testName}_Parallel_Time_us", SampleUnit.Microsecond);
        var allocGroup = new SampleGroup($"{testName}_Parallel_Alloc_B", SampleUnit.Byte);

        for (int i = 0; i < 5; i++)
        {
            yield return orchestrator.ExecuteAsyncInParallel(context, cts.Token).ToCoroutine();
        }

        for (int i = 0; i < measurementCount; i++)
        {
            var testContext = CreateContext();
            testContext.SetTag(testName);
            MeasureResult result = default;

            yield return MeasureAsync(
                orchestrator.ExecuteAsyncInParallel(testContext, cts.Token),
                (r) => result = r);

            Measure.Custom(timeGroup, result.ElapsedMs * 1000.0);
            Measure.Custom(allocGroup, result.AllocatedBytes);
        }
    }

    private IEnumerator RunSequentialPerformanceTest(
        string testName,
        UniTaskOrchestrator<string> orchestrator,
        int measurementCount = 30)
    {
        var context = CreateContext();
        var timeGroup = new SampleGroup($"{testName}_Sequential_Time_us", SampleUnit.Microsecond);
        var allocGroup = new SampleGroup($"{testName}_Sequential_Alloc_B", SampleUnit.Byte);

        for (int i = 0; i < 5; i++)
        {
            yield return orchestrator.ExecuteAsyncSequentially(context, cts.Token).ToCoroutine();
        }

        for (int i = 0; i < measurementCount; i++)
        {
            var testContext = CreateContext();
            testContext.SetTag(testName);
            MeasureResult result = default;

            yield return MeasureAsync(
                orchestrator.ExecuteAsyncSequentially(testContext, cts.Token),
                (r) => result = r);

            Measure.Custom(timeGroup, result.ElapsedMs * 1000.0);
            Measure.Custom(allocGroup, result.AllocatedBytes);
        }
    }

    // ======================== 基础场景（并行） ========================

    [UnityTest, Performance]
    public IEnumerator SingleStepParallel()
    {
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(new NullStep("A"))
            .Build();

        yield return RunParallelPerformanceTest("SingleStep", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator ParallelRootStepsParallel()
    {
        var stepA = new NullStep("A");
        var stepB = new NullStep("B");
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(stepA)
            .AddStep(stepB)
            .Build();

        yield return RunParallelPerformanceTest("ParallelRootSteps", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator DiamondDependencyParallel()
    {
        var root = new NullStep("Root");
        var left = new NullStep("Left", new[] { root });
        var right = new NullStep("Right", new[] { root });
        var sink = new NullStep("Sink", new[] { left, right });
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(root)
            .AddStep(left)
            .AddStep(right)
            .AddStep(sink)
            .Build();

        yield return RunParallelPerformanceTest("DiamondDependency", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator WithBehaviorPipelineParallel()
    {
        var inner = new NullStep("Inner");
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(inner)
            .AddBehavior<NullStep>(new NullBehavior())
            .AddBehavior<NullStep>(new NullBehavior())
            .Build();

        yield return RunParallelPerformanceTest("WithBehaviorPipeline", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator Chain10StepsParallel()
    {
        var steps = new NullStep[10];
        steps[0] = new NullStep("C0");
        for (int i = 1; i < 10; i++)
            steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });

        var builder = UniTaskOrchestrator<string>.Builder.Create();
        foreach (var s in steps) builder.AddStep(s);
        var orchestrator = builder.Build();

        yield return RunParallelPerformanceTest("Chain10Steps", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator FanOut50StepsParallel()
    {
        var fans = new NullStep[50];
        for (int i = 0; i < 50; i++)
            fans[i] = new NullStep($"Fan{i}");
        var collector = new NullStep("Collector", fans);

        var builder = UniTaskOrchestrator<string>.Builder.Create();
        foreach (var f in fans) builder.AddStep(f);
        builder.AddStep(collector);
        var orchestrator = builder.Build();

        yield return RunParallelPerformanceTest("FanOut50Steps", orchestrator);
    }

    // ======================== 基础场景（串行） ========================

    [UnityTest, Performance]
    public IEnumerator SingleStepSequential()
    {
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(new NullStep("A"))
            .Build();

        yield return RunSequentialPerformanceTest("SingleStep", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator DiamondDependencySequential()
    {
        var root = new NullStep("Root");
        var left = new NullStep("Left", new[] { root });
        var right = new NullStep("Right", new[] { root });
        var sink = new NullStep("Sink", new[] { left, right });
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(root)
            .AddStep(left)
            .AddStep(right)
            .AddStep(sink)
            .Build();

        yield return RunSequentialPerformanceTest("DiamondDependency", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator WithBehaviorPipelineSequential()
    {
        var inner = new NullStep("Inner");
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(inner)
            .AddBehavior<NullStep>(new NullBehavior())
            .AddBehavior<NullStep>(new NullBehavior())
            .Build();

        yield return RunSequentialPerformanceTest("WithBehaviorPipeline", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator Chain10StepsSequential()
    {
        var steps = new NullStep[10];
        steps[0] = new NullStep("C0");
        for (int i = 1; i < 10; i++)
            steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });

        var builder = UniTaskOrchestrator<string>.Builder.Create();
        foreach (var s in steps) builder.AddStep(s);
        var orchestrator = builder.Build();

        yield return RunSequentialPerformanceTest("Chain10Steps", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator Chain100StepsSequential()
    {
        var steps = new NullStep[100];
        steps[0] = new NullStep("C0");
        for (int i = 1; i < 100; i++)
            steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });

        var builder = UniTaskOrchestrator<string>.Builder.Create();
        foreach (var s in steps) builder.AddStep(s);
        var orchestrator = builder.Build();

        yield return RunSequentialPerformanceTest("Chain100Steps", orchestrator);
    }

    // ======================== 其他现有测试（保持兼容，改为并行） ========================

    [UnityTest, Performance]
    public IEnumerator WithConcurrencyLimit()
    {
        var steps = new NullStep[10];
        steps[0] = new NullStep("C0");
        for (int i = 1; i < 10; i++)
            steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });

        var builder = UniTaskOrchestrator<string>.Builder.Create();
        foreach (var s in steps) builder.AddStep(s);
        builder.WithMaxConcurrency(2);
        var orchestrator = builder.Build();

        yield return RunParallelPerformanceTest("WithConcurrencyLimit", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator SingleStepRepeated10()
    {
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(new NullStep("A"))
            .Build();

        yield return RunParallelPerformanceTest("SingleStepRepeated10", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator SingleStepFail()
    {
        var failStep = new FailStep("FailAll", new System.InvalidOperationException("test"));
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(failStep)
            .Build();

        yield return RunParallelPerformanceTest("SingleStepFail", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator CancelImmediately()
    {
        var orchestrator = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(new NullStep("A"))
            .Build();

        var timeGroup = new SampleGroup("CancelImmediately_Time_ms", SampleUnit.Millisecond);
        var allocGroup = new SampleGroup("CancelImmediately_Alloc_B", SampleUnit.Byte);

        for (int i = 0; i < 5; i++)
        {
            var testCts = new CancellationTokenSource();
            testCts.Cancel();
            yield return ExecuteWithCancel(orchestrator, testCts.Token).ToCoroutine();
            testCts.Dispose();
        }

        for (int i = 0; i < 30; i++)
        {
            var testCts = new CancellationTokenSource();
            testCts.Cancel();

            MeasureResult result = default;
            yield return MeasureAsync(
                ExecuteWithCancel(orchestrator, testCts.Token),
                (r) => result = r);

            Measure.Custom(timeGroup, result.ElapsedMs);
            Measure.Custom(allocGroup, result.AllocatedBytes);
            testCts.Dispose();
        }
    }

    private async UniTask ExecuteWithCancel(UniTaskOrchestrator<string> orchestrator, System.Threading.CancellationToken token)
    {
        try
        {
            await orchestrator.ExecuteAsyncInParallel(CreateContext(), token);
        }
        catch (System.OperationCanceledException) { }
    }

    [UnityTest, Performance]
    public IEnumerator ManyBehaviors10()
    {
        var step = new NullStep("Core");
        var builder = UniTaskOrchestrator<string>.Builder.Create()
            .AddStep(step);
        for (int i = 0; i < 10; i++)
            builder.AddBehavior<NullStep>(new NullBehavior());
        var orchestrator = builder.Build();

        yield return RunParallelPerformanceTest("ManyBehaviors10", orchestrator);
    }

    [UnityTest, Performance]
    public IEnumerator ComplexMixed()
    {
        const int n = 20;
        var steps = new NullStep[n];
        steps[0] = new NullStep("M0");
        for (int i = 1; i < n; i++)
            steps[i] = new NullStep($"M{i}", new[] { steps[i - 1] });

        var builder = UniTaskOrchestrator<string>.Builder.Create();
        foreach (var s in steps) builder.AddStep(s);
        builder.WithMaxConcurrency(4);
        builder.AddBehavior<NullStep>(new NullBehavior());
        builder.AddBehavior<NullStep>(new NullBehavior());
        builder.AddBehavior<NullStep>(new NullBehavior());
        var orchestrator = builder.Build();

        yield return RunParallelPerformanceTest("ComplexMixed", orchestrator);
    }

    // ======================== 辅助实现 ========================

    private sealed class NullStep : IUniTaskStep<string>
    {
        private readonly string key;
        private readonly IReadOnlyCollection<IStep<string>> deps;

        public NullStep(string key, IUniTaskStep<string>[] deps = null)
        {
            this.key = key;
            this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? System.Array.Empty<IStep<string>>();
        }

        public string Key => key;
        public IReadOnlyCollection<IStep<string>> Dependencies => deps;

        public UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, System.Threading.CancellationToken token)
        {
            return new UniTask<StepResult>(StepResult.Continue());
        }
    }

    private sealed class NullBehavior : IUniTaskBehavior<string>
    {
        public UniTask<StepResult> HandleAsync(
            ITypedPipelineContext context,
            UniTaskBehaviorStepper<string> stepper,
            System.Threading.CancellationToken token)
        {
            return stepper.NextAsync(token);
        }
    }

    private sealed class FailStep : IUniTaskStep<string>
    {
        private readonly string key;
        private readonly System.Exception exception;
        private readonly IReadOnlyCollection<IStep<string>> deps;

        public FailStep(string key, System.Exception exception, IUniTaskStep<string>[] deps = null)
        {
            this.key = key;
            this.exception = exception;
            this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? System.Array.Empty<IStep<string>>();
        }

        public string Key => key;
        public IReadOnlyCollection<IStep<string>> Dependencies => deps;

        public UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, System.Threading.CancellationToken token)
        {
            return new UniTask<StepResult>(StepResult.Fail(exception));
        }
    }
}