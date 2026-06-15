using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Orchestrator.Tasks;

namespace Orchestrator.Benchmark.Tasks
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class TaskOrchestratorBenchmarks
    {
        private TaskOrchestrator<string> singleStepOrchestrator;
        private TaskOrchestrator<string> parallelOrchestrator;
        private TaskOrchestrator<string> diamondOrchestrator;
        private TaskOrchestrator<string> behaviorOrchestrator;
        private TaskOrchestrator<string> chain10Orchestrator;
        private TaskOrchestrator<string> fanOut50Orchestrator;
        private TaskOrchestrator<string> failAllOrchestrator;
        private CancellationTokenSource cts;

        [GlobalSetup]
        public void Setup()
        {
            // 1. 单步骤
            singleStepOrchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(new NullStep("A"))
                .Build();

            // 2. 并行双根
            var stepA = new NullStep("A");
            var stepB = new NullStep("B");
            parallelOrchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .Build();

            // 3. 钻石依赖 (4 步骤)
            var root = new NullStep("Root");
            var left = new NullStep("Left", new[] { root });
            var right = new NullStep("Right", new[] { root });
            var sink = new NullStep("Sink", new[] { left, right });
            diamondOrchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(root)
                .AddStep(left)
                .AddStep(right)
                .AddStep(sink)
                .Build();

            // 4. 带2个行为链的单步骤
            var inner = new NullStep("Inner");
            behaviorOrchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(inner)
                .AddBehavior<NullStep>(new NullBehavior())
                .AddBehavior<NullStep>(new NullBehavior())
                .Build();

            // 5. 10步骤链式依赖
            var chainSteps = new NullStep[10];
            chainSteps[0] = new NullStep("C0");
            for (int i = 1; i < 10; i++)
                chainSteps[i] = new NullStep($"C{i}", new[] { chainSteps[i - 1] });
            var chainBuilder = TaskOrchestrator<string>.Builder.Create();
            foreach (var s in chainSteps) chainBuilder.AddStep(s);
            chain10Orchestrator = chainBuilder.Build();

            // 6. 50步扇形汇聚
            var fans = new NullStep[50];
            for (int i = 0; i < 50; i++)
                fans[i] = new NullStep($"Fan{i}");
            var collector = new NullStep("Collector", fans);
            var fanBuilder = TaskOrchestrator<string>.Builder.Create();
            foreach (var f in fans) fanBuilder.AddStep(f);
            fanBuilder.AddStep(collector);
            fanOut50Orchestrator = fanBuilder.Build();

            // 7. 失败步骤
            var failStep = new FailStep("FailAll", new InvalidOperationException("test"));
            failAllOrchestrator = TaskOrchestrator<string>.Builder.Create()
                .AddStep(failStep)
                .Build();

            cts = new CancellationTokenSource();
        }

        [GlobalCleanup]
        public void Cleanup() => cts?.Dispose();

        private static ITypedPipelineContext CreateContext() => new EmptyContext();

        // ========================= 并行场景（原有） =========================
        [Benchmark(Baseline = true, Description = "单步骤(并行)")]
        public async Task SingleStepParallel()
            => await singleStepOrchestrator.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "双根步骤并行执行(并行)")]
        public async Task ParallelRootStepsParallel()
            => await parallelOrchestrator.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "钻石依赖(4步骤)(并行)")]
        public async Task DiamondDependencyParallel()
            => await diamondOrchestrator.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "带2个行为链的单步骤(并行)")]
        public async Task WithBehaviorPipelineParallel()
            => await behaviorOrchestrator.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "10步链式依赖(并行)")]
        public async Task Chain10StepsParallel()
            => await chain10Orchestrator.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "50步扇形汇聚(并行)")]
        public async Task FanOut50StepsParallel()
            => await fanOut50Orchestrator.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);

        // ========================= 串行场景（新增） =========================
        [Benchmark(Description = "单步骤(串行)")]
        public async Task SingleStepSequential()
            => await singleStepOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "钻石依赖(4步骤)(串行)")]
        public async Task DiamondDependencySequential()
            => await diamondOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "带2个行为链的单步骤(串行)")]
        public async Task WithBehaviorPipelineSequential()
            => await behaviorOrchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "10步链式依赖(串行)")]
        public async Task Chain10StepsSequential()
            => await chain10Orchestrator.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);

        [Benchmark(Description = "100步链式依赖(串行)")]
        public async Task Chain100StepsSequential()
        {
            const int n = 100;
            var steps = new NullStep[n];
            steps[0] = new NullStep("C0");
            for (int i = 1; i < n; i++)
                steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });

            var builder = TaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            var orch = builder.Build();
            await orch.ExecuteAsyncSequentially(CreateContext(), CancellationToken.None);
        }

        // ========================= 原有的其他测试（保持兼容） =========================
        [Benchmark(Description = "并发限制(2) + 10步链")]
        public async Task WithConcurrencyLimit()
        {
            var steps = new NullStep[10];
            steps[0] = new NullStep("C0");
            for (int i = 1; i < 10; i++)
                steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });
            var builder = TaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            builder.WithMaxConcurrency(2);
            var orch = builder.Build();
            await orch.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);
        }

        [Benchmark(Description = "单步骤重复10次")]
        public async Task SingleStepRepeated10()
        {
            for (int i = 0; i < 10; i++)
                await singleStepOrchestrator.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);
        }

        [Benchmark(Description = "单步骤失败")]
        public async Task SingleStepFail()
        {
            await failAllOrchestrator.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);
        }

        [Benchmark(Description = "取消令牌(立即取消)")]
        public async Task CancelImmediately()
        {
            var src = new CancellationTokenSource();
            src.Cancel();
            try
            {
                await singleStepOrchestrator.ExecuteAsyncInParallel(CreateContext(), src.Token);
            }
            catch (OperationCanceledException) { }
        }

        [Benchmark(Description = "100步链式依赖(并行)")]
        public async Task Chain100StepsParallel()
        {
            const int n = 100;
            var steps = new NullStep[n];
            steps[0] = new NullStep("C0");
            for (int i = 1; i < n; i++)
                steps[i] = new NullStep($"C{i}", new[] { steps[i - 1] });

            var builder = TaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            var orch = builder.Build();
            await orch.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);
        }

        [Benchmark(Description = "10行为链 + 单步骤")]
        public async Task ManyBehaviors10()
        {
            var step = new NullStep("Core");
            var builder = TaskOrchestrator<string>.Builder.Create()
                .AddStep(step);
            for (int i = 0; i < 10; i++)
                builder.AddBehavior<NullStep>(new NullBehavior());
            var orch = builder.Build();
            await orch.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);
        }

        [Benchmark(Description = "复杂混合(20步骤+3行为+并发)")]
        public async Task ComplexMixed()
        {
            const int n = 20;
            var steps = new NullStep[n];
            steps[0] = new NullStep("M0");
            for (int i = 1; i < n; i++)
                steps[i] = new NullStep($"M{i}", new[] { steps[i - 1] });

            var builder = TaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            builder.WithMaxConcurrency(4);
            builder.AddBehavior<NullStep>(new NullBehavior());
            builder.AddBehavior<NullStep>(new NullBehavior());
            builder.AddBehavior<NullStep>(new NullBehavior());
            var orch = builder.Build();
            await orch.ExecuteAsyncInParallel(CreateContext(), CancellationToken.None);
        }
    }

    // ======================== 最精简的辅助实现（保持不变） ========================

    /// <summary>空步骤 - 只返回 Continue，无任何操作</summary>
    internal sealed class NullStep : ITaskStep<string>
    {
        private readonly string key;
        private readonly IReadOnlyCollection<IStep<string>> deps;

        public NullStep(string key, ITaskStep<string>[] deps = null)
        {
            this.key = key;
            this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => key;
        public IReadOnlyCollection<IStep<string>> Dependencies => deps;

        public Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            return Task.FromResult(StepResult.Continue());
        }
    }

    /// <summary>空行为 - 只调用 NextAsync，无任何额外操作</summary>
    internal sealed class NullBehavior : ITaskBehavior<string>
    {
        public Task<StepResult> HandleAsync(
            ITypedPipelineContext context,
            TaskBehaviorStepper<string> stepper,
            CancellationToken token)
        {
            return stepper.NextAsync(token);
        }
    }

    /// <summary>失败步骤 - 只返回 Fail</summary>
    internal sealed class FailStep : ITaskStep<string>
    {
        private readonly string key;
        private readonly Exception exception;
        private readonly IReadOnlyCollection<IStep<string>> deps;

        public FailStep(string key, Exception exception, ITaskStep<string>[] deps = null)
        {
            this.key = key;
            this.exception = exception;
            this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => key;
        public IReadOnlyCollection<IStep<string>> Dependencies => deps;

        public Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            return Task.FromResult(StepResult.Fail(exception));
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