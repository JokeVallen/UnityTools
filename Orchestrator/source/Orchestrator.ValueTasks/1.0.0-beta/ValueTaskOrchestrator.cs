using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.ValueTasks
{
    public sealed class ValueTaskOrchestrator<TIn, TOut>
    {
        private ExecutionPlan plan;
        private InterruptionPolicy policy;
        private SemaphoreSlim concurrencySemaphore;
        private bool enableOutputCache;

        private ValueTaskOrchestrator(){}

        public async ValueTask<ExecutionResult<TOut>> ExecuteAsync(TIn input, CancellationToken token = default)
        {
            var stepCount = plan.Steps.Length;
            var tasks = new ValueTask<StepResult<TOut>>[stepCount];
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

        private async ValueTask<StepResult<TOut>> RunStepAsync(int index, TIn input, ExecutionContext ctx, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var stepEntry = plan.Steps[index];
            var depIndices = stepEntry.DependencyIndices;

            if (depIndices.Length > 0)
            {
                var depTasks = new ValueTask<StepResult<TOut>>[depIndices.Length];
                for (int j = 0; j < depIndices.Length; j++)
                    depTasks[j] = ctx.tasks[depIndices[j]];

                var depResults = await ValueTaskOrchestratorUtility.WhenAll(depTasks);

                if (policy == InterruptionPolicy.DependencyBased && depResults.Any(r => r.Flow != StepFlow.Continue))
                    return StepResult<TOut>.Break();
            }

            token.ThrowIfCancellationRequested();

            if (policy == InterruptionPolicy.Strict && ctx.IsGlobalBroken)
                return StepResult<TOut>.Break();

            if (concurrencySemaphore != null)
                await concurrencySemaphore.WaitAsync(token);

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
            catch (OperationCanceledException){ throw; }
            catch (Exception ex){ result = StepResult<TOut>.Fail(ex); }
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
            public readonly ValueTask<StepResult<TOut>>[] tasks;
            public readonly StepExecutionResult[] stepResults;
            public readonly object[] outputCache;
            public readonly StepOutputLookup outputLookup;
            public volatile bool IsGlobalBroken;

            public ExecutionContext(ValueTask<StepResult<TOut>>[] tasks, StepExecutionResult[] stepResults, object[] outputCache, StepOutputLookup outputLookup)
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
            public IValueTaskStep<TIn, TOut> Step { get; }
            public int[] DependencyIndices { get; }
            public Func<TIn, IReadOnlyDictionary<IStep, object>, object> InputMapper { get; }
            public Func<TIn, CancellationToken, ValueTask<StepResult<TOut>>> CompiledPipeline { get; }

            public StepEntry(IValueTaskStep<TIn, TOut> step, int[] dependencyIndices, Func<TIn, IReadOnlyDictionary<IStep, object>, object> inputMapper, Func<TIn, CancellationToken, ValueTask<StepResult<TOut>>> compiledPipeline)
            {
                Step = step;
                DependencyIndices = dependencyIndices;
                InputMapper = inputMapper;
                CompiledPipeline = compiledPipeline;
            }
        }

        public class Builder
        {
            private List<IValueTaskStep<TIn, TOut>> Steps
            {
                get 
                {
                    if (steps == null)
                        steps = new List<IValueTaskStep<TIn, TOut>>();
                    return steps;
                }
            }

            private List<IValueTaskBehavior<TIn, TOut>> Behaviors
            {
                get 
                {
                    if (behaviors == null)
                        behaviors = new List<IValueTaskBehavior<TIn, TOut>>();
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

            private List<IValueTaskStep<TIn, TOut>> steps;
            private List<IValueTaskBehavior<TIn, TOut>> behaviors;
            private InterruptionPolicy policy = InterruptionPolicy.DependencyBased;
            private IValueTaskStep<TIn, TOut> finalStep;
            private int? maxDegreeOfParallelism;
            private Dictionary<IStep, Func<TIn, IReadOnlyDictionary<IStep, object>, object>> inputMappers;
            private bool built;

            private Builder() { }

            /// <summary>
            /// 创建构建器实例
            /// </summary>
            /// <returns>构建器实例</returns>
            public static Builder Create() { return new Builder(); }

            /// <summary>添加执行步骤</summary>
            /// <param name="step">步骤实例</param>
            /// <returns>构建器实例</returns>
            /// <remarks><para>步骤将根据其内部定义的依赖关系自动决定执行顺序，添加顺序不代表执行顺序。</para></remarks>
            public Builder AddStep(IValueTaskStep<TIn, TOut> step) 
            { 
                ThrowIfBuilt();
                if (step == null) throw new ArgumentNullException(nameof(step));
                Steps.Add(step); 
                return this; 
            }

            /// <summary>添加全局行为</summary>
            /// <param name="behavior">行为实例</param>
            /// <returns>构建器实例</returns>
            /// <remarks><para>行为将按添加的先后顺序形成管道，包裹在步骤执行外层。</para></remarks>
            public Builder AddBehavior(IValueTaskBehavior<TIn, TOut> behavior) 
            { 
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                Behaviors.Add(behavior); 
                return this; 
            }

            /// <summary>设置中断策略</summary>
            /// <param name="policy">策略枚举</param>
            /// <returns>构建器实例</returns>
            /// <remarks><para>默认策略为 <see cref="InterruptionPolicy.DependencyBased"/>。</para></remarks>
            public Builder UsePolicy(InterruptionPolicy policy) 
            { 
                ThrowIfBuilt(); 
                this.policy = policy; 
                return this; 
            }

            /// <summary>
            /// 设置最终产出步骤
            /// </summary>
            /// <param name="finalStep">最终产出步骤</param>
            /// <returns>构建器实例</returns>
            public Builder SetFinalStep(IValueTaskStep<TIn, TOut> finalStep) 
            { 
                ThrowIfBuilt(); 
                if(finalStep == null) throw new ArgumentNullException(nameof(finalStep));
                this.finalStep = finalStep; 
                return this; 
            }

            /// <summary>
            /// 设置最大并发数
            /// </summary>
            /// <param name="count">最大并发数</param>
            /// <returns>构建器实例</returns>
            public Builder WithMaxConcurrency(int count) 
            {
                ThrowIfBuilt();
                maxDegreeOfParallelism = count > 0 ? count : 1;
                return this;
            }

            /// <summary>
            /// 输入映射
            /// </summary>
            /// <typeparam name="TCurrentIn">映射的输入类型</typeparam>
            /// <param name="step">步骤</param>
            /// <param name="mapper">映射函数</param>
            /// <returns>构建器实例</returns>
            public Builder MapInput<TCurrentIn>(IValueTaskStep<TCurrentIn, TOut> step, Func<TIn, IReadOnlyDictionary<IStep, object>, TCurrentIn> mapper)
            {
                ThrowIfBuilt();
                if (step == null) throw new ArgumentNullException(nameof(step));
                if (mapper == null) throw new ArgumentNullException(nameof(mapper));
                InputMappers[step] = (originalIn, cache) => mapper(originalIn, cache);
                return this;
            }

            /// <summary>生成编排器实例</summary>
            /// <returns>编排器实例</returns>
            /// <remarks><para>根据当前配置完成初始化并返回可执行的编排器实例。</para></remarks>
            public ValueTaskOrchestrator<TIn, TOut> Build() 
            {
                ThrowIfBuilt();
                built = true;

                if (steps == null || steps.Count == 0)
                    throw new InvalidOperationException("Cannot build an orchestrator without any steps.");

                finalStep = ResolveFinalStep();
                if (!steps.Contains(finalStep))
                    throw new ArgumentException("The specified final step is not part of the step collection.");

                if (!OrchestratorUtility.ValidateNoCycles(steps.Cast<IStep>(), out var cycleSteps))
                    throw new InvalidOperationException(
                        $"Cycle detected in step dependencies. Involved steps: {string.Join(", ", cycleSteps)}");

                var sortedSteps = OrchestratorUtility.TopologicalSort(steps.Cast<IStep>()).Cast<IValueTaskStep<TIn, TOut>>().ToArray();
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

                    var compiledPipeline = ValueTaskOrchestratorUtility.CompilePipeline(step, behaviors);
                    stepEntries[i] = new StepEntry(step, depIndices, mapper, compiledPipeline);
                }

                var plan = new ExecutionPlan(stepEntries, stepIndexMap[finalStep], stepIndexMap);
                var semaphore = maxDegreeOfParallelism.HasValue ? new SemaphoreSlim(maxDegreeOfParallelism.Value) : null;

                return new ValueTaskOrchestrator<TIn, TOut>() 
                { 
                    plan = plan,
                    policy = policy,
                    concurrencySemaphore = semaphore,
                    enableOutputCache = hasMappers
                };
            }

            private IValueTaskStep<TIn, TOut> ResolveFinalStep()
            {
                if (finalStep != null) return finalStep;

                var allSteps = steps.Cast<IStep>().ToList();
                var dependentSet = new HashSet<IStep>();
                int count = allSteps.Count;
                for (int i = 0; i < count; i++)
                {
                    var step = allSteps[i];
                    if (step.Dependencies != null)
                    {
                        foreach (var dep in step.Dependencies)
                            dependentSet.Add(dep);
                    }
                }

                var sinkSteps = allSteps.Where(s => !dependentSet.Contains(s)).ToList();
                if (sinkSteps.Count == 0)
                    throw new InvalidOperationException(
                        "No final step could be determined. Every step is a dependency of another, which suggests a cycle. Please set an explicit final step.");
                if (sinkSteps.Count > 1)
                    throw new InvalidOperationException(
                        $"Multiple sink steps detected ({string.Join(", ", sinkSteps.Select(s => s.Name))}). Cannot automatically determine the final output step. Use the Builder's SetFinalStep method to specify which one produces the final output.");
                return (IValueTaskStep<TIn, TOut>)sinkSteps[0];
            }

            private void ThrowIfBuilt() { if (built) throw new InvalidOperationException("The builder instance cannot be reused."); }
        }
    }
}