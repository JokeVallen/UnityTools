using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Orchestrator.ValueTasks;

namespace Orchestrator.Benchmark.ValueTasks
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class ValueTaskOrchestratorBenchmarks
    {
        private ValueTaskOrchestrator<string> singleStepOrchestrator;
        private ValueTaskOrchestrator<string> parallelOrchestrator;
        private ValueTaskOrchestrator<string> diamondOrchestrator;
        private ValueTaskOrchestrator<string> behaviorOrchestrator;
        private ValueTaskOrchestrator<string> chain10Orchestrator;
        private ValueTaskOrchestrator<string> fanOut50Orchestrator;
        private ValueTaskOrchestrator<string> failAllOrchestrator;
        private CancellationTokenSource cts;

        [GlobalSetup]
        public void Setup()
        {
            // 1. 单步骤
            singleStepOrchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(new NullStep("A"))
                .Build();

            // 2. 并行双根
            var stepA = new NullStep("A");
            var stepB = new NullStep("B");
            parallelOrchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .Build();

            // 3. 钻石依赖 (4 步骤)
            var root = new NullStep("Root");
            var left = new NullStep("Left", new[] { root });
            var right = new NullStep("Right", new[] { root });
            var sink = new NullStep("Sink", new[] { left, right });
            diamondOrchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(root)
                .AddStep(left)
                .AddStep(right)
                .AddStep(sink)
                .Build();

            // 4. 带2个行为链的单步骤
            var inner = new NullStep("Inner");
            behaviorOrchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(inner)
                .AddBehavior<NullStep>(new NullBehavior())
                .AddBehavior<NullStep>(new NullBehavior())
                .Build();

            // 5. 10步骤链式依赖
            var chainSteps = new NullStep[10];
            chainSteps[0] = new NullStep("C0");
            for (int i = 1; i < 10; i++)
                chainSteps[i] = new NullStep($"C{i}", new[] { chainSteps[i - 1] });
            var chainBuilder = ValueTaskOrchestrator<string>.Builder.Create();
            foreach (var s in chainSteps) chainBuilder.AddStep(s);
            chain10Orchestrator = chainBuilder.Build();

            // 6. 50步扇形汇聚
            var fans = new NullStep[50];
            for (int i = 0; i < 50; i++)
                fans[i] = new NullStep($"Fan{i}");
            var collector = new NullStep("Collector", fans);
            var fanBuilder = ValueTaskOrchestrator<string>.Builder.Create();
            foreach (var f in fans) fanBuilder.AddStep(f);
            fanBuilder.AddStep(collector);
            fanOut50Orchestrator = fanBuilder.Build();

            // 7. 失败步骤
            var failStep = new FailStep("FailAll", new InvalidOperationException("test"));
            failAllOrchestrator = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(failStep)
                .Build();

            cts = new CancellationTokenSource();
        }

        [GlobalCleanup]
        public void Cleanup() => cts?.Dispose();

        private static ITypedPipelineContext CreateContext() => new EmptyContext();

        // ========================= 基础场景 =========================
        [Benchmark(Baseline = true, Description = "单步骤")]
        public async Task SingleStep()
            => await singleStepOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "双根步骤并行执行")]
        public async Task ParallelRootSteps()
            => await parallelOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "钻石依赖(4步骤)")]
        public async Task DiamondDependency()
            => await diamondOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "带2个行为链的单步骤")]
        public async Task WithBehaviorPipeline()
            => await behaviorOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "10步链式依赖")]
        public async Task Chain10Steps()
            => await chain10Orchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "50步扇形汇聚")]
        public async Task FanOut50Steps()
            => await fanOut50Orchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        // ========================= 并发限制 =========================
        [Benchmark(Description = "并发限制(2) + 10步链")]
        public async Task WithConcurrencyLimit()
        {
            var steps = new NullStep[10];
            steps[0] = new NullStep("C0");
            for (int i = 1; i < 10; i++)
                steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });
            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            builder.WithMaxConcurrency(2);
            var orch = builder.Build();
            await orch.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);
        }

        // ========================= 重复执行 =========================
        [Benchmark(Description = "单步骤重复10次")]
        public async Task SingleStepRepeated10()
        {
            for (int i = 0; i < 10; i++)
                await singleStepOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);
        }

        // ========================= 异常路径 =========================
        [Benchmark(Description = "单步骤失败")]
        public async Task SingleStepFail()
        {
            await failAllOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);
        }

        // ========================= 取消令牌 =========================
        [Benchmark(Description = "取消令牌(立即取消)")]
        public async Task CancelImmediately()
        {
            var src = new CancellationTokenSource();
            src.Cancel();
            try
            {
                await singleStepOrchestrator.ExecuteAsyncSequentially(CreateContext(), src.Token);
            }
            catch (OperationCanceledException) { }
        }

        // ========================= 大量步骤 =========================
        [Benchmark(Description = "100步链式依赖")]
        public async Task Chain100Steps()
        {
            const int n = 100;
            var steps = new NullStep[n];
            steps[0] = new NullStep("C0");
            for (int i = 1; i < n; i++)
                steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });

            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            var orch = builder.Build();
            await orch.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);
        }

        // ========================= 大量行为 =========================
        [Benchmark(Description = "10行为链 + 单步骤")]
        public async Task ManyBehaviors10()
        {
            var step = new NullStep("Core");
            var builder = ValueTaskOrchestrator<string>.Builder.Create()
                .AddStep(step);
            for (int i = 0; i < 10; i++)
                builder.AddBehavior<NullStep>(new NullBehavior());
            var orch = builder.Build();
            await orch.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);
        }

        // ========================= 混合场景 =========================
        [Benchmark(Description = "复杂混合(20步骤+3行为+并发)")]
        public async Task ComplexMixed()
        {
            const int n = 20;
            var steps = new NullStep[n];
            steps[0] = new NullStep("M0");
            for (int i = 1; i < n; i++)
                steps[i] = new NullStep($"M{i}", new[] { steps[i - 1] });

            var builder = ValueTaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            builder.WithMaxConcurrency(4);
            builder.AddBehavior<NullStep>(new NullBehavior());
            builder.AddBehavior<NullStep>(new NullBehavior());
            builder.AddBehavior<NullStep>(new NullBehavior());
            var orch = builder.Build();
            await orch.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);
        }
    }

    // ======================== 最精简的辅助实现 ========================

    /// <summary>空步骤 - 只返回 Continue，无任何操作</summary>
    internal sealed class NullStep : IValueTaskStep<string>
    {
        private readonly string key;
        private readonly IReadOnlyCollection<IStep<string>> deps;

        public NullStep(string key, IValueTaskStep<string>[] deps = null)
        {
            this.key = key;
            this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => key;
        public IReadOnlyCollection<IStep<string>> Dependencies => deps;

        public ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            return new ValueTask<StepResult>(StepResult.Continue());
        }
    }

    /// <summary>空行为 - 只调用 NextAsync，无任何额外操作</summary>
    internal sealed class NullBehavior : IValueTaskBehavior<string>
    {
        public ValueTask<StepResult> HandleAsync(
            ITypedPipelineContext context,
            ValueTaskBehaviorStepper<string> stepper,
            CancellationToken token)
        {
            return stepper.NextAsync(token);
        }
    }

    /// <summary>失败步骤 - 只返回 Fail</summary>
    internal sealed class FailStep : IValueTaskStep<string>
    {
        private readonly string key;
        private readonly Exception exception;
        private readonly IReadOnlyCollection<IStep<string>> deps;

        public FailStep(string key, Exception exception, IValueTaskStep<string>[] deps = null)
        {
            this.key = key;
            this.exception = exception;
            this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => key;
        public IReadOnlyCollection<IStep<string>> Dependencies => deps;

        public ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            return new ValueTask<StepResult>(StepResult.Fail(exception));
        }
    }

    internal sealed class EmptyContext : ITypedPipelineContext
    {
        public void Set<TKey, TValue>(TKey key, TValue value) { }
        public Optional<TValue> Get<TKey, TValue>(TKey key) => Optional<TValue>.None;
        public void AddStepExecutionResult<TStepKey>(StepExecutionResult<TStepKey> stepExecutionResult) { }
        public Optional<StepExecutionResult<TStepKey>> GetStepExecutionResult<TStepKey>(TStepKey key) => Optional<StepExecutionResult<TStepKey>>.None;
        public bool Remove<TKey, TValue>(TKey key) => false;
        public bool ContainsKey<TKey, TValue>(TKey key) => false;
        public void Clear() { }
        public IEnumerable<StepExecutionResult<TStepKey>> GetAllStepExecutionResults<TStepKey>()=> Array.Empty<StepExecutionResult<TStepKey>>();
    }
}