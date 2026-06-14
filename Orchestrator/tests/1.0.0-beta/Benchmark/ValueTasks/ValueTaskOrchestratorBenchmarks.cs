using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Orchestrator.ValueTasks;

namespace Orchestrator.Benchmark.ValueTasks
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class ValueTaskOrchestratorBenchmarks
    {
        // -------------------------------------------------------
        // 预构建编排器（ GlobalSetup 中初始化）
        // -------------------------------------------------------
        private ValueTaskOrchestrator<string, string> singleStepOrchestrator;
        private ValueTaskOrchestrator<string, string> parallelOrchestrator;
        private ValueTaskOrchestrator<string, string> diamondOrchestrator;
        private ValueTaskOrchestrator<string, string> behaviorOrchestrator;
        private ValueTaskOrchestrator<string, string> chain10NoPolicy;
        private ValueTaskOrchestrator<string, string> fanOut50;
        private ValueTaskOrchestrator<string, string> failAllOrchestrator;
        private CancellationTokenSource cts;

        [GlobalSetup]
        public void Setup()
        {
            // 1. 单步骤
            singleStepOrchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(new SyncStep("A", "done"))
                .Build();

            // 2. 并行双根（显式设置最终步骤）
            var stepA = new SyncStep("A", "A");
            var stepB = new SyncStep("B", "B");
            parallelOrchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(stepA)
                .AddStep(stepB)
                .SetFinalStep(stepB)
                .Build();

            // 3. 钻石依赖 (4 步骤)
            var root = new SyncStep("Root", "root");
            var left = new SyncStep("Left", "left", new[] { root });
            var right = new SyncStep("Right", "right", new[] { root });
            var sink = new SyncStep("Sink", "sink", new[] { left, right });
            diamondOrchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(root)
                .AddStep(left)
                .AddStep(right)
                .AddStep(sink)
                .SetFinalStep(sink)
                .Build();

            // 4. 带行为链（两个行为）
            var inner = new SyncStep("Inner", "done");
            behaviorOrchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(inner)
                .AddBehavior(new LoggingBehavior<string, string>())
                .AddBehavior(new TimingBehavior<string, string>())
                .Build();

            // 5. 10步骤链式依赖
            var chainSteps = new List<SyncStep>();
            SyncStep prev = null;
            for (int i = 0; i < 10; i++)
            {
                var s = new SyncStep($"Chain{i}", $"val{i}", prev != null ? new[] { prev } : null);
                chainSteps.Add(s);
                prev = s;
            }
            chain10NoPolicy = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(chainSteps[0])
                .AddStep(chainSteps[1])
                .AddStep(chainSteps[2])
                .AddStep(chainSteps[3])
                .AddStep(chainSteps[4])
                .AddStep(chainSteps[5])
                .AddStep(chainSteps[6])
                .AddStep(chainSteps[7])
                .AddStep(chainSteps[8])
                .AddStep(chainSteps[9])
                .SetFinalStep(chainSteps[9])
                .Build();

            // 6. 扇形汇聚（50 个无依赖步骤并行，最后汇聚到一个步骤）
            var fans = new SyncStep[50];
            for (int i = 0; i < 50; i++)
                fans[i] = new SyncStep($"Fan{i}", $"fan{i}");
            var collector = new SyncStep("Collector", "final", fans);
            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            foreach (var f in fans)
                builder.AddStep(f);
            builder.AddStep(collector);
            builder.SetFinalStep(collector);
            fanOut50 = builder.Build();

            // 7. 全部失败的编排器（用于异常路径测试）
            var failStep = new FailingStep("FailAll", new InvalidOperationException("test"));
            failAllOrchestrator = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(failStep)
                .Build();

            // 取消令牌
            cts = new CancellationTokenSource();
        }

        [GlobalCleanup]
        public void Cleanup() => cts?.Dispose();

        // ========================= 基础场景 =========================
        [Benchmark(Baseline = true, Description = "单步骤(同步成功)")]
        public async Task SingleStepSyncSuccess()
            => await singleStepOrchestrator.ExecuteAsync("input", CancellationToken.None);

        [Benchmark(Description = "双根步骤并行执行")]
        public async Task ParallelRootSteps()
            => await parallelOrchestrator.ExecuteAsync("input", CancellationToken.None);

        [Benchmark(Description = "钻石依赖(4步骤)")]
        public async Task DiamondDependency()
            => await diamondOrchestrator.ExecuteAsync("input", CancellationToken.None);

        [Benchmark(Description = "带2个行为链的单步骤")]
        public async Task WithBehaviorPipeline()
            => await behaviorOrchestrator.ExecuteAsync("input", CancellationToken.None);

        [Benchmark(Description = "10步链式依赖")]
        public async Task Chain10Steps()
            => await chain10NoPolicy.ExecuteAsync("input", CancellationToken.None);

        [Benchmark(Description = "50步扇形汇聚")]
        public async Task FanOut50Steps()
            => await fanOut50.ExecuteAsync("input", CancellationToken.None);

        // ========================= 可选功能开销 =========================
        [Benchmark(Description = "并发限制(2) + 10步链")]
        public async Task WithConcurrencyLimit()
        {
            var a = new SyncStep("A", "");
            var b = new SyncStep("B", "");
            var c = new SyncStep("C", "", new[] { a, b });
            var orch = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(a).AddStep(b).AddStep(c)
                .WithMaxConcurrency(2)
                .SetFinalStep(c)
                .Build();
            await orch.ExecuteAsync("input", CancellationToken.None);
        }

        [Benchmark(Description = "输入映射(5步链,每步映射)")]
        public async Task InputMappingChain5()
        {
            SyncStep[] steps = new SyncStep[5];
            steps[0] = new SyncStep("Step0", "v0");
            for (int i = 1; i < 5; i++)
                steps[i] = new SyncStep($"Step{i}", $"v{i}", new[] { steps[i - 1] });

            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            for (int i = 1; i < 5; i++)
            {
                var prevStep = steps[i - 1];
                var currentStep = steps[i];
                builder.MapInput(currentStep, (input, cache) =>
                {
                    var prev = cache[prevStep].ToString();
                    return $"{input}_{prev}";
                });
            }
            builder.SetFinalStep(steps[4]);
            var orch = builder.Build();
            await orch.ExecuteAsync("start", CancellationToken.None);
        }

        // ========================= 重复执行（分摊分配） =========================
        [Benchmark(Description = "单步骤重复10次")]
        public async Task SingleStepRepeated10()
        {
            for (int i = 0; i < 10; i++)
                await singleStepOrchestrator.ExecuteAsync("input", CancellationToken.None);
        }

        // ========================= 异常路径 =========================
        [Benchmark(Description = "全部步骤失败(Fail)")]
        public async Task AllStepsFail()
        {
            await failAllOrchestrator.ExecuteAsync("input", CancellationToken.None);
        }

        // ========================= 取消令牌 =========================
        [Benchmark(Description = "取消令牌(立即取消)")]
        public async Task CancelImmediately()
        {
            var src = new CancellationTokenSource();
            src.Cancel();
            try
            {
                await singleStepOrchestrator.ExecuteAsync("input", src.Token);
            }
            catch (OperationCanceledException) { }
        }

        // ========================= 大量步骤 / 行为 =========================
        [Benchmark(Description = "100步链式依赖")]
        public async Task Chain100Steps()
        {
            const int n = 100;
            var steps = new SyncStep[n];
            steps[0] = new SyncStep("C0", "v0");
            for (int i = 1; i < n; i++)
                steps[i] = new SyncStep($"C{i}", $"v{i}", new[] { steps[i - 1] });

            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            builder.SetFinalStep(steps[n - 1]);
            var orch = builder.Build();
            await orch.ExecuteAsync("start", CancellationToken.None);
        }

        [Benchmark(Description = "10行为链 + 单步骤")]
        public async Task ManyBehaviors10()
        {
            var step = new SyncStep("Core", "ok");
            var builder = ValueTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step);
            for (int i = 0; i < 10; i++)
                builder.AddBehavior(new LoggingBehavior<string, string>());
            var orch = builder.Build();
            await orch.ExecuteAsync("input", CancellationToken.None);
        }

        // ========================= 混合场景 =========================
        [Benchmark(Description = "复杂混合(20步骤+3行为+映射+并发)")]
        public async Task ComplexMixed()
        {
            const int n = 20;
            var steps = new SyncStep[n];
            steps[0] = new SyncStep("M0", "v0");
            for (int i = 1; i < n; i++)
                steps[i] = new SyncStep($"M{i}", $"v{i}", new[] { steps[i - 1] });

            var builder = ValueTaskOrchestrator<string, string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            for (int i = 1; i < n; i++)
            {
                var prev = steps[i - 1];
                var cur = steps[i];
                builder.MapInput(cur, (input, cache) => $"{input}_{cache[prev]}");
            }
            builder.SetFinalStep(steps[n - 1]);
            builder.WithMaxConcurrency(4);
            builder.AddBehavior(new LoggingBehavior<string, string>());
            builder.AddBehavior(new TimingBehavior<string, string>());
            builder.AddBehavior(new LoggingBehavior<string, string>());
            var orch = builder.Build();
            await orch.ExecuteAsync("mix", CancellationToken.None);
        }
    }

    // ======================== 辅助实现（ValueTask 版本） ========================
    internal sealed class SyncStep : IValueTaskStep<string, string>
    {
        private readonly string _name;
        private readonly string _output;
        private readonly IReadOnlyCollection<IStep> _dependencies;

        public SyncStep(string name, string output, IReadOnlyList<IValueTaskStep<string, string>> dependencies = null)
        {
            _name = name;
            _output = output;
            _dependencies = dependencies?.Select(d => (IStep)d).ToList();
            if (_dependencies == null) _dependencies = Array.Empty<IStep>();
        }

        public string Name => _name;
        public IReadOnlyCollection<IStep> Dependencies => _dependencies;

        public ValueTask<StepResult<string>> ExecuteAsync(string input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return new ValueTask<StepResult<string>>(StepResult<string>.Continue(_output));
        }
    }

    internal sealed class FailingStep : IValueTaskStep<string, string>
    {
        private readonly string _name;
        private readonly Exception _exception;
        private readonly IReadOnlyCollection<IStep> _dependencies;

        public FailingStep(string name, Exception ex, IReadOnlyList<IValueTaskStep<string, string>> deps = null)
        {
            _name = name;
            _exception = ex;
            _dependencies = deps?.Select(d => (IStep)d).ToList();
            if (_dependencies == null) _dependencies = Array.Empty<IStep>();
        }

        public string Name => _name;
        public IReadOnlyCollection<IStep> Dependencies => _dependencies;

        public ValueTask<StepResult<string>> ExecuteAsync(string input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return new ValueTask<StepResult<string>>(StepResult<string>.Fail(_exception));
        }
    }

    internal sealed class LoggingBehavior<TIn, TOut> : IValueTaskBehavior<TIn, TOut>
    {
        public ValueTask<StepResult<TOut>> HandleAsync(
            TIn input, Func<ValueTask<StepResult<TOut>>> next, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return next(); // 直接转发
        }
    }

    internal sealed class TimingBehavior<TIn, TOut> : IValueTaskBehavior<TIn, TOut>
    {
        public async ValueTask<StepResult<TOut>> HandleAsync(
            TIn input, Func<ValueTask<StepResult<TOut>>> next, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var sw = Stopwatch.StartNew();
            var result = await next();
            sw.Stop();
            return result;
        }
    }
}