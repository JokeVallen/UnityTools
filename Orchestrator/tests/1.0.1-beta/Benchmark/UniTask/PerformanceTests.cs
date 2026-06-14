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
using Orchestrator;
using NUnit.Framework;

namespace PlayModeTests
{
    public class PerformanceTests
    {
        private const int WarmupCount = 5;
        private const int MeasurementCount = 30;

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

        private static long GetGCAllocDelta(long preAlloc)
        {
            if (UseGCAllocRecorder)
                return GCAllocRecorder.CurrentValue - preAlloc;

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long postAlloc = Profiler.GetTotalAllocatedMemoryLong();
            return postAlloc - preAlloc;
        }

        private static long GetGCAllocBaseline()
        {
            if (UseGCAllocRecorder)
                return GCAllocRecorder.CurrentValue;

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            return Profiler.GetTotalAllocatedMemoryLong();
        }

        private ITypedPipelineContext CreateContext()
        {
            return new TypedPipelineContext();
        }

        private IEnumerator MeasurePerformance(string name, Func<UniTask> asyncAction)
        {
            for (int i = 0; i < WarmupCount; i++)
                yield return UniTask.ToCoroutine(() => asyncAction());

            for (int i = 0; i < MeasurementCount; i++)
            {
                double elapsedMs = 0;
                long allocatedBytes = 0;

                yield return UniTask.ToCoroutine(async () =>
                {
                    long preAlloc = GetGCAllocBaseline();

                    var sw = Stopwatch.StartNew();

                    using (new ProfilerMarker($"Performance_{name}").Auto())
                    {
                        Profiler.BeginSample($"Performance_{name}");
                        await asyncAction();
                        Profiler.EndSample();
                    }

                    sw.Stop();
                    elapsedMs = sw.Elapsed.TotalMilliseconds;
                    allocatedBytes = GetGCAllocDelta(preAlloc);
                });

                Measure.Custom(new SampleGroup(name + "_Time", SampleUnit.Millisecond), elapsedMs);
                Measure.Custom(new SampleGroup(name + "_Alloc", SampleUnit.Byte), allocatedBytes);

                yield return null;
            }
        }

        [UnityTest, Performance]
        public IEnumerator SingleStep_SyncSuccess()
        {
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(new SyncStep("A", "result", "done"))
                .Build();
            yield return MeasurePerformance("SingleStep", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator TwoParallelRootSteps()
        {
            var a = new SyncStep("A");
            var b = new SyncStep("B");
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(a).AddStep(b)
                .Build();
            yield return MeasurePerformance("TwoParallelRoots", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator DiamondDependency_4Steps()
        {
            var root = new SyncStep("Root");
            var left = new SyncStep("Left", "left_result", "left", new[] { root });
            var right = new SyncStep("Right", "right_result", "right", new[] { root });
            var sink = new SyncStep("Sink", "sink_result", "sink", new[] { left, right });
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(root).AddStep(left).AddStep(right).AddStep(sink)
                .Build();
            yield return MeasurePerformance("DiamondDependency", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator SingleStepWithTwoBehaviors()
        {
            var step = new SyncStep("Core", "result", "done");
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(step)
                .AddBehavior<SyncStep>(new LoggingBehavior())
                .AddBehavior<SyncStep>(new TimingBehavior())
                .Build();
            yield return MeasurePerformance("WithTwoBehaviors", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator ChainOf10Steps()
        {
            var steps = new SyncStep[10];
            steps[0] = new SyncStep("C0");
            for (int i = 1; i < 10; i++)
                steps[i] = new SyncStep($"C{i}", $"result_{i}", $"v{i}", new[] { steps[i - 1] });
            var builder = UniTaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            var orch = builder.Build();
            yield return MeasurePerformance("ChainOf10", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator FanOut50()
        {
            var fans = new SyncStep[50];
            for (int i = 0; i < 50; i++)
                fans[i] = new SyncStep($"Fan{i}", $"fan_result_{i}", $"fan{i}");
            var collector = new SyncStep("Collector", "final_result", "final", fans);
            var builder = UniTaskOrchestrator<string>.Builder.Create();
            foreach (var f in fans) builder.AddStep(f);
            builder.AddStep(collector);
            var orch = builder.Build();
            yield return MeasurePerformance("FanOut50", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator ConcurrencyLimit_Two_ChainOf10()
        {
            var a = new SyncStep("A");
            var b = new SyncStep("B");
            var c = new SyncStep("C", "c_result", "C", new[] { a, b });
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(a).AddStep(b).AddStep(c)
                .WithMaxConcurrency(2)
                .Build();
            yield return MeasurePerformance("ConcurrencyLimit2", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator SingleStepRepeated10Times()
        {
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(new SyncStep("A", "result", "done"))
                .Build();
            yield return MeasurePerformance("Repeat10Times", async () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    var context = CreateContext();
                    await orch.ExecuteAsync(context);
                }
            });
        }

        [UnityTest, Performance]
        public IEnumerator AllStepsFail()
        {
            var fail = new FailStep("FailAll", new InvalidOperationException("test"));
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(fail)
                .Build();
            yield return MeasurePerformance("AllFail", async () =>
            {
                var context = CreateContext();
                try { await orch.ExecuteAsync(context); }
                catch { }
            });
        }

        [UnityTest, Performance]
        public IEnumerator ImmediateCancellation()
        {
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(new SyncStep("A", "result", "done"))
                .Build();
            var src = new CancellationTokenSource();
            src.Cancel();
            yield return MeasurePerformance("ImmediateCancel", async () =>
            {
                var context = CreateContext();
                try { await orch.ExecuteAsync(context, src.Token); }
                catch (OperationCanceledException) { }
            });
        }

        [UnityTest, Performance]
        public IEnumerator ChainOf100Steps()
        {
            var steps = new SyncStep[100];
            steps[0] = new SyncStep("C0");
            for (int i = 1; i < 100; i++)
                steps[i] = new SyncStep($"C{i}", $"result_{i}", $"v{i}", new[] { steps[i - 1] });
            var builder = UniTaskOrchestrator<string>.Builder.Create();
            foreach (var s in steps) builder.AddStep(s);
            var orch = builder.Build();
            yield return MeasurePerformance("ChainOf100", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator TenBehaviors_SingleStep()
        {
            var step = new SyncStep("Core", "result", "ok");
            var builder = UniTaskOrchestrator<string>.Builder.Create().AddStep(step);
            for (int i = 0; i < 10; i++)
                builder.AddBehavior<SyncStep>(new LoggingBehavior());
            var orch = builder.Build();
            yield return MeasurePerformance("TenBehaviors", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }

        [UnityTest, Performance]
        public IEnumerator HighConcurrency()
        {
            var orch = UniTaskOrchestrator<string>.Builder.Create()
                .AddStep(new SyncStep("A"))
                .AddStep(new SyncStep("B"))
                .AddStep(new SyncStep("C"))
                .AddStep(new SyncStep("D"))
                .WithMaxConcurrency(2)
                .Build();
            yield return MeasurePerformance("HighConcurrency", async () =>
            {
                var context = CreateContext();
                await orch.ExecuteAsync(context);
            });
        }
    }
}