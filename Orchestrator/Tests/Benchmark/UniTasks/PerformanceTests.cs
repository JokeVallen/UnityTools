using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.PerformanceTesting;
using Orchestrator.UniTasks;
using System.Diagnostics;
using System.Collections;
using UnityEngine.TestTools;
using Unity.Profiling;
using UnityEngine.Profiling;

namespace PlayModeTests
{
    public class PerformanceTests
    {
        private const int WarmupCount = 5;
        private const int MeasurementCount = 30;

        // ---------- GC 测量基础结构 ----------
        private static readonly bool UseGCAllocRecorder;
        private static readonly ProfilerRecorder GCAllocRecorder;

        static PerformanceTests()
        {
            try
            {
                GCAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated Memory");
                UseGCAllocRecorder = GCAllocRecorder.Valid;
            }
            catch
            {
                UseGCAllocRecorder = false;
            }
        }

        /// <summary>
        /// 获取从某个时刻开始的 GC 分配字节数（增量）。
        /// </summary>
        private static long GetGCAllocDelta(long preAlloc)
        {
            if (UseGCAllocRecorder)
                return GCAllocRecorder.CurrentValue - preAlloc;

            // 回退方案：强制 GC 后取存活内存差值
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long postAlloc = Profiler.GetTotalAllocatedMemoryLong();
            return postAlloc - preAlloc;
        }

        /// <summary>
        /// 获取当前 GC 分配基线（字节）。
        /// </summary>
        private static long GetGCAllocBaseline()
        {
            if (UseGCAllocRecorder)
                return GCAllocRecorder.CurrentValue;

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            return Profiler.GetTotalAllocatedMemoryLong();
        }

        private IEnumerator MeasurePerformance(string name, Func<UniTask> asyncAction)
        {
            // 预热
            for (int i = 0; i < WarmupCount; i++)
                yield return UniTask.ToCoroutine(() => asyncAction());

            for (int i = 0; i < MeasurementCount; i++)
            {
                double elapsedMs = 0;
                long allocatedBytes = 0;

                yield return UniTask.ToCoroutine(async () =>
                {
                    // 获取 GC 基线（必须在异步 lambda 内，因为需要与 sw 同帧）
                    long preAlloc = GetGCAllocBaseline();

                    var sw = Stopwatch.StartNew();

                    using (new ProfilerMarker($"Performance_{name}").Auto())
                        await asyncAction();

                    sw.Stop();
                    elapsedMs = sw.Elapsed.TotalMilliseconds;

                    // 计算本次异步操作的 GC 分配增量
                    allocatedBytes = GetGCAllocDelta(preAlloc);
                });

                Measure.Custom(new SampleGroup(name + "_Time", SampleUnit.Millisecond), elapsedMs);
                Measure.Custom(new SampleGroup(name + "_Alloc", SampleUnit.Byte), allocatedBytes);

                yield return null;
            }
        }

        // ==================== 测试用例（完全不变）====================

        [UnityTest, Performance]
        public IEnumerator SingleStep_SyncSuccess()
        {
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(new SyncStep("A", "done")).Build();
            yield return MeasurePerformance("SingleStep", () => orch.ExecuteAsync("input"));
        }

        [UnityTest, Performance]
        public IEnumerator TwoParallelRootSteps()
        {
            var a = new SyncStep("A", "A");
            var b = new SyncStep("B", "B");
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(a).AddStep(b).SetFinalStep(b).Build();
            yield return MeasurePerformance("TwoParallelRoots", () => orch.ExecuteAsync("input"));
        }

        [UnityTest, Performance]
        public IEnumerator DiamondDependency_4Steps()
        {
            var root = new SyncStep("Root", "root");
            var left = new SyncStep("Left", "left", new[] { root });
            var right = new SyncStep("Right", "right", new[] { root });
            var sink = new SyncStep("Sink", "sink", new[] { left, right });
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(root).AddStep(left).AddStep(right).AddStep(sink)
                .SetFinalStep(sink).Build();
            yield return MeasurePerformance("DiamondDependency", () => orch.ExecuteAsync("input"));
        }

        [UnityTest, Performance]
        public IEnumerator SingleStepWithTwoBehaviors()
        {
            var step = new SyncStep("Core", "done");
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(step)
                .AddBehavior(new LoggingBehavior<string, string>())
                .AddBehavior(new TimingBehavior<string, string>())
                .Build();
            yield return MeasurePerformance("WithTwoBehaviors", () => orch.ExecuteAsync("input"));
        }

        [UnityTest, Performance]
        public IEnumerator ChainOf10Steps()
        {
            var steps = new SyncStep[10];
            steps[0] = new SyncStep("C0", "v0");
            for (int i = 1; i < 10; i++)
                steps[i] = new SyncStep($"C{i}", $"v{i}", new[] { steps[i - 1] });
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            builder.SetFinalStep(steps[9]);
            var orch = builder.Build();
            yield return MeasurePerformance("ChainOf10", () => orch.ExecuteAsync("start"));
        }

        [UnityTest, Performance]
        public IEnumerator FanOut50()
        {
            var fans = new SyncStep[50];
            for (int i = 0; i < 50; i++)
                fans[i] = new SyncStep($"Fan{i}", $"fan{i}");
            var collector = new SyncStep("Collector", "final", fans);
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
            foreach (var f in fans) builder.AddStep(f);
            builder.AddStep(collector);
            builder.SetFinalStep(collector);
            var orch = builder.Build();
            yield return MeasurePerformance("FanOut50", () => orch.ExecuteAsync("input"));
        }

        [UnityTest, Performance]
        public IEnumerator ConcurrencyLimit_Two_ChainOf10()
        {
            var a = new SyncStep("A", "");
            var b = new SyncStep("B", "");
            var c = new SyncStep("C", "", new[] { a, b });
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(a).AddStep(b).AddStep(c)
                .WithMaxConcurrency(2).SetFinalStep(c).Build();
            yield return MeasurePerformance("ConcurrencyLimit2", () => orch.ExecuteAsync("input"));
        }

        [UnityTest, Performance]
        public IEnumerator InputMapping_5Steps()
        {
            var steps = new SyncStep[5];
            steps[0] = new SyncStep("Step0", "v0");
            for (int i = 1; i < 5; i++)
                steps[i] = new SyncStep($"Step{i}", $"v{i}", new[] { steps[i - 1] });
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            for (int i = 1; i < 5; i++)
            {
                var prev = steps[i - 1];
                var cur = steps[i];
                builder.MapInput(cur, (input, cache) => $"{input}_{cache[prev]}");
            }
            builder.SetFinalStep(steps[4]);
            var orch = builder.Build();
            yield return MeasurePerformance("InputMapping5", () => orch.ExecuteAsync("start"));
        }

        [UnityTest, Performance]
        public IEnumerator SingleStepRepeated10Times()
        {
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(new SyncStep("A", "done")).Build();
            yield return MeasurePerformance("Repeat10Times", async () =>
            {
                for (int i = 0; i < 10; i++)
                    await orch.ExecuteAsync("input");
            });
        }

        [UnityTest, Performance]
        public IEnumerator AllStepsFail()
        {
            var fail = new FailStep("FailAll", new InvalidOperationException("test"));
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(fail).Build();
            yield return MeasurePerformance("AllFail", () => orch.ExecuteAsync("input"));
        }

        [UnityTest, Performance]
        public IEnumerator ImmediateCancellation()
        {
            var orch = UniTaskOrchestrator<string, string>.Builder.Create()
                .AddStep(new SyncStep("A", "done")).Build();
            var src = new CancellationTokenSource();
            src.Cancel();
            yield return MeasurePerformance("ImmediateCancel", async () =>
            {
                try { await orch.ExecuteAsync("input", src.Token); }
                catch (OperationCanceledException) { }
            });
        }

        [UnityTest, Performance]
        public IEnumerator ChainOf100Steps()
        {
            var steps = new SyncStep[100];
            steps[0] = new SyncStep("C0", "v0");
            for (int i = 1; i < 100; i++)
                steps[i] = new SyncStep($"C{i}", $"v{i}", new[] { steps[i - 1] });
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            builder.SetFinalStep(steps[99]);
            var orch = builder.Build();
            yield return MeasurePerformance("ChainOf100", () => orch.ExecuteAsync("start"));
        }

        [UnityTest, Performance]
        public IEnumerator TenBehaviors_SingleStep()
        {
            var step = new SyncStep("Core", "ok");
            var builder = UniTaskOrchestrator<string, string>.Builder.Create().AddStep(step);
            for (int i = 0; i < 10; i++)
                builder.AddBehavior(new LoggingBehavior<string, string>());
            var orch = builder.Build();
            yield return MeasurePerformance("TenBehaviors", () => orch.ExecuteAsync("input"));
        }

        [UnityTest, Performance]
        public IEnumerator ComplexMixed_20Steps_3Behaviors_Mapping_Concurrency()
        {
            const int n = 20;
            var steps = new SyncStep[n];
            steps[0] = new SyncStep("M0", "v0");
            for (int i = 1; i < n; i++)
                steps[i] = new SyncStep($"M{i}", $"v{i}", new[] { steps[i - 1] });
            var builder = UniTaskOrchestrator<string, string>.Builder.Create();
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
            yield return MeasurePerformance("ComplexMixed", () => orch.ExecuteAsync("mix"));
        }
    }
}