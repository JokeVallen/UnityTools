using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Orchestrator.UniTasks
{
    public sealed class UniTaskOrchestrator<TIn, TOut>
    {
        private ExecutionPlan plan;
        private InterruptionPolicy policy;
        private SemaphoreSlim concurrencySemaphore;
        private bool enableOutputCache;

        private UniTaskOrchestrator() { }

        public async UniTask<ExecutionResult<TOut>> ExecuteAsync(TIn input, CancellationToken token = default)
        {
            var stepCount = plan.Steps.Length;
            var tasks = new UniTask<StepResult<TOut>>[stepCount];
            var stepResults = new StepExecutionResult[stepCount];
            object[] outputCache = enableOutputCache ? new object[stepCount] : null;
            StepOutputLookup outputLookup = null;
            if (enableOutputCache) outputLookup = new StepOutputLookup(plan.StepIndexMap, outputCache);

            var ctx = new ExecutionContext(tasks, stepResults, outputCache, outputLookup);
            var sw = Stopwatch.StartNew();

            try
            {
                for (int i = 0; i < stepCount; i++)
                    tasks[i] = RunStepAsync(i, input, ctx, token);

                var finalResult = await tasks[plan.FinalStepIndex];
                sw.Stop();

                var executedResults = stepResults.Where(r => r.StepName != null).ToArray();

                return new ExecutionResult<TOut>(
                    !ctx.IsGlobalBroken && finalResult.Flow == StepFlow.Continue,
                    finalResult.Output,
                    executedResults,
                    sw.Elapsed);
            }
            catch
            {
                if (sw.IsRunning) sw.Stop();
                throw;
            }
        }

        private async UniTask<StepResult<TOut>> RunStepAsync(int index, TIn input, ExecutionContext ctx, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var stepEntry = plan.Steps[index];
            var depIndices = stepEntry.DependencyIndices;

            if (depIndices.Length > 0)
            {
                var depTasks = new UniTask<StepResult<TOut>>[depIndices.Length];
                for (int j = 0; j < depIndices.Length; j++)
                    depTasks[j] = ctx.tasks[depIndices[j]];

                var depResults = await UniTask.WhenAll(depTasks);

                if (policy == InterruptionPolicy.DependencyBased && depResults.Any(r => r.Flow != StepFlow.Continue))
                    return StepResult<TOut>.Break();
            }

            token.ThrowIfCancellationRequested();

            if (policy == InterruptionPolicy.Strict && ctx.IsGlobalBroken)
                return StepResult<TOut>.Break();

            if (concurrencySemaphore != null)
                await concurrencySemaphore.WaitAsync(token).AsUniTask();

            var stepSw = Stopwatch.StartNew();
            StepResult<TOut> result;
            try
            {
                TIn effectiveInput = input;
                if (stepEntry.InputMapper != null)
                {
                    var outputSnapshot = ctx.GetOutputSnapshot();
                    effectiveInput = (TIn)stepEntry.InputMapper(input, outputSnapshot);
                }

                var pipeline = stepEntry.CompiledPipeline;
                result = await pipeline(effectiveInput, token);

                if (enableOutputCache && result.Flow == StepFlow.Continue)
                    ctx.outputCache[index] = result.Output;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                result = StepResult<TOut>.Fail(ex);
            }
            finally
            {
                concurrencySemaphore?.Release();
                stepSw.Stop();
            }

            if (result.Flow != StepFlow.Continue)
                ctx.IsGlobalBroken = true;

            ctx.stepResults[index] = new StepExecutionResult(
                stepEntry.Step.Name,
                result.Flow != StepFlow.Fail,
                result.Flow,
                result.Output,
                result.Exception,
                stepSw.Elapsed);

            return result;
        }

        // ---------- 内部类型 ----------
        private class StepOutputLookup : IReadOnlyDictionary<IStep, object>
        {
            private readonly Dictionary<IStep, int> indexMap;
            private readonly object[] outputs;

            public StepOutputLookup(Dictionary<IStep, int> indexMap, object[] outputs)
            {
                this.indexMap = indexMap;
                this.outputs = outputs;
            }

            public object this[IStep key]
            {
                get
                {
                    if (indexMap.TryGetValue(key, out int idx))
                        return outputs[idx];
                    throw new KeyNotFoundException();
                }
            }

            public IEnumerable<IStep> Keys => indexMap.Keys;
            public IEnumerable<object> Values => outputs;
            public int Count => outputs.Length;

            public bool ContainsKey(IStep key) => indexMap.ContainsKey(key);
            public bool TryGetValue(IStep key, out object value)
            {
                if (indexMap.TryGetValue(key, out int idx))
                {
                    value = outputs[idx];
                    return true;
                }
                value = null;
                return false;
            }

            public IEnumerator<KeyValuePair<IStep, object>> GetEnumerator()
            {
                foreach (var kvp in indexMap)
                    yield return new KeyValuePair<IStep, object>(kvp.Key, outputs[kvp.Value]);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class ExecutionContext
        {
            public readonly UniTask<StepResult<TOut>>[] tasks;
            public readonly StepExecutionResult[] stepResults;
            public readonly object[] outputCache;
            public readonly StepOutputLookup outputLookup;
            public volatile bool IsGlobalBroken;

            public ExecutionContext(UniTask<StepResult<TOut>>[] tasks, StepExecutionResult[] stepResults,
                object[] outputCache, StepOutputLookup outputLookup)
            {
                this.tasks = tasks;
                this.stepResults = stepResults;
                this.outputCache = outputCache;
                this.outputLookup = outputLookup;
            }

            public IReadOnlyDictionary<IStep, object> GetOutputSnapshot()
            {
                return outputLookup;
            }
        }

        private readonly struct ExecutionPlan
        {
            public StepEntry[] Steps { get; }
            public int FinalStepIndex { get; }
            public Dictionary<IStep, int> StepIndexMap { get; }

            public ExecutionPlan(StepEntry[] steps, int finalStepIndex, Dictionary<IStep, int> stepIndexMap)
            {
                Steps = steps;
                FinalStepIndex = finalStepIndex;
                StepIndexMap = stepIndexMap;
            }
        }

        private readonly struct StepEntry
        {
            public IUniTaskStep<TIn, TOut> Step { get; }
            public int[] DependencyIndices { get; }
            public Func<TIn, IReadOnlyDictionary<IStep, object>, object> InputMapper { get; }
            public Func<TIn, CancellationToken, UniTask<StepResult<TOut>>> CompiledPipeline { get; }

            public StepEntry(IUniTaskStep<TIn, TOut> step, int[] dependencyIndices,
                Func<TIn, IReadOnlyDictionary<IStep, object>, object> inputMapper,
                Func<TIn, CancellationToken, UniTask<StepResult<TOut>>> compiledPipeline)
            {
                Step = step;
                DependencyIndices = dependencyIndices;
                InputMapper = inputMapper;
                CompiledPipeline = compiledPipeline;
            }
        }

        // ---------- Builder ----------
        public class Builder
        {
            private List<IUniTaskStep<TIn, TOut>> Steps
            {
                get
                {
                    if (steps == null) steps = new List<IUniTaskStep<TIn, TOut>>();
                    return steps;
                }
            }

            private List<IUniTaskBehavior<TIn, TOut>> Behaviors
            {
                get
                {
                    if (behaviors == null) behaviors = new List<IUniTaskBehavior<TIn, TOut>>();
                    return behaviors;
                }
            }

            private Dictionary<IStep, Func<TIn, IReadOnlyDictionary<IStep, object>, object>> InputMappers
            {
                get
                {
                    if (inputMappers == null)
                        inputMappers = new Dictionary<IStep, Func<TIn, IReadOnlyDictionary<IStep, object>, object>>();
                    return inputMappers;
                }
            }

            private List<IUniTaskStep<TIn, TOut>> steps;
            private List<IUniTaskBehavior<TIn, TOut>> behaviors;
            private InterruptionPolicy policy = InterruptionPolicy.DependencyBased;
            private IUniTaskStep<TIn, TOut> finalStep;
            private int? maxDegreeOfParallelism;
            private Dictionary<IStep, Func<TIn, IReadOnlyDictionary<IStep, object>, object>> inputMappers;
            private bool built;

            private Builder() { }

            public static Builder Create() => new Builder();

            public Builder AddStep(IUniTaskStep<TIn, TOut> step)
            {
                ThrowIfBuilt();
                if (step == null) throw new ArgumentNullException(nameof(step));
                Steps.Add(step);
                return this;
            }

            public Builder AddBehavior(IUniTaskBehavior<TIn, TOut> behavior)
            {
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                Behaviors.Add(behavior);
                return this;
            }

            public Builder UsePolicy(InterruptionPolicy policy)
            {
                ThrowIfBuilt();
                this.policy = policy;
                return this;
            }

            public Builder SetFinalStep(IUniTaskStep<TIn, TOut> finalStep)
            {
                ThrowIfBuilt();
                if (finalStep == null) throw new ArgumentNullException(nameof(finalStep));
                this.finalStep = finalStep;
                return this;
            }

            public Builder WithMaxConcurrency(int count)
            {
                ThrowIfBuilt();
                maxDegreeOfParallelism = count > 0 ? count : 1;
                return this;
            }

            public Builder MapInput<TCurrentIn>(IUniTaskStep<TCurrentIn, TOut> step, Func<TIn, IReadOnlyDictionary<IStep, object>, TCurrentIn> mapper)
            {
                ThrowIfBuilt();
                if (step == null) throw new ArgumentNullException(nameof(step));
                if (mapper == null) throw new ArgumentNullException(nameof(mapper));
                InputMappers[step] = (originalIn, cache) => mapper(originalIn, cache);
                return this;
            }

            public UniTaskOrchestrator<TIn, TOut> Build()
            {
                ThrowIfBuilt();
                built = true;

                if (steps == null || steps.Count == 0)
                    throw new InvalidOperationException("Cannot build an orchestrator without any steps.");

                finalStep = ResolveFinalStep();
                if (!steps.Contains(finalStep))
                    throw new ArgumentException("The specified final step is not part of the step collection.");

                // 使用共享的图验证工具
                if (!OrchestratorUtility.ValidateNoCycles(steps.Cast<IStep>(), out var cycleSteps))
                    throw new InvalidOperationException(
                        $"Cycle detected in step dependencies. Involved steps: {string.Join(", ", cycleSteps)}");

                var sortedSteps = OrchestratorUtility.TopologicalSort(steps.Cast<IStep>()).Cast<IUniTaskStep<TIn, TOut>>().ToArray();
                var stepIndexMap = new Dictionary<IStep, int>();
                for (int i = 0; i < sortedSteps.Length; i++)
                    stepIndexMap[sortedSteps[i]] = i;

                var stepEntries = new StepEntry[sortedSteps.Length];
                bool hasMappers = inputMappers != null && inputMappers.Count > 0;
                for (int i = 0; i < sortedSteps.Length; i++)
                {
                    var step = sortedSteps[i];
                    int[] depIndices = Array.Empty<int>();
                    if (step.Dependencies != null && step.Dependencies.Count > 0)
                        depIndices = step.Dependencies.Select(d => stepIndexMap[d]).ToArray();

                    Func<TIn, IReadOnlyDictionary<IStep, object>, object> mapper = null;
                    if (hasMappers && inputMappers.TryGetValue(step, out var rawMapper))
                        mapper = rawMapper;

                    var compiledPipeline = UniTaskOrchestratorUtility.CompilePipeline(step, behaviors);
                    stepEntries[i] = new StepEntry(step, depIndices, mapper, compiledPipeline);
                }

                var plan = new ExecutionPlan(stepEntries, stepIndexMap[finalStep], stepIndexMap);
                var semaphore = maxDegreeOfParallelism.HasValue ? new SemaphoreSlim(maxDegreeOfParallelism.Value) : null;

                return new UniTaskOrchestrator<TIn, TOut>
                {
                    plan = plan,
                    policy = policy,
                    concurrencySemaphore = semaphore,
                    enableOutputCache = hasMappers
                };
            }

            private IUniTaskStep<TIn, TOut> ResolveFinalStep()
            {
                if (finalStep != null) return finalStep;

                var allSteps = steps.Cast<IStep>().ToList();
                var dependentSet = new HashSet<IStep>();
                foreach (var step in allSteps)
                {
                    if (step.Dependencies != null)
                        foreach (var dep in step.Dependencies)
                            dependentSet.Add(dep);
                }

                var sinkSteps = allSteps.Where(s => !dependentSet.Contains(s)).ToList();
                if (sinkSteps.Count == 0)
                    throw new InvalidOperationException(
                        "No final step could be determined. Every step is a dependency of another, which suggests a cycle. Please set an explicit final step.");
                if (sinkSteps.Count > 1)
                    throw new InvalidOperationException(
                        $"Multiple sink steps detected ({string.Join(", ", sinkSteps.Select(s => s.Name))}). Cannot automatically determine the final output step. Use the Builder's SetFinalStep method to specify which one produces the final output.");
                return (IUniTaskStep<TIn, TOut>)sinkSteps[0];
            }

            private void ThrowIfBuilt() { if (built) throw new InvalidOperationException("The builder instance cannot be reused."); }
        }
    }
}