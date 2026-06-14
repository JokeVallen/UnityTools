using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tasks
{
    /// <summary>任务编排器</summary>
    /// <typeparam name="TKey">步骤唯一标识的类型</typeparam>
    public sealed class TaskOrchestrator<TKey>
    {
        private ExecutionPlan plan;
        private InterruptionPolicy policy;
        private SemaphoreSlim concurrencySemaphore;

        private TaskOrchestrator(){}

        /// <summary>执行编排</summary>
        /// <param name="context">上下文</param>
        /// <param name="token">取消令牌</param>
        /// <returns>编排执行结果</returns>
        public async Task<ExecutionResult<TKey>> ExecuteAsync(ITypedPipelineContext context, CancellationToken token = default)
        {
            var stepCount = plan.Steps.Length;
            var tasks = ArrayPool.Rent<Task<StepResult>>(stepCount);
            var stepResults = ArrayPool.Rent<StepExecutionResult<TKey>>(stepCount);
            var executionContext = new ExecutionContext(tasks, stepResults);
            var sw = Stopwatch.StartNew();

            try
            {
                for (int i = 0; i < stepCount; i++)
                    tasks[i] = RunStepAsync(i, executionContext, context, token);

                await Task.WhenAll(tasks);
                sw.Stop();

                var validCount = 0;
                for (int i = 0; i < stepCount; i++)
                {
                    if (stepResults[i].StepKey.HasValue)
                        validCount++;
                }

                var executedResults = new StepExecutionResult<TKey>[validCount];
                int idx = 0;
                for (int i = 0; i < stepCount; i++)
                {
                    if (stepResults[i].StepKey.HasValue)
                        executedResults[idx++] = stepResults[i];
                }

                return new ExecutionResult<TKey>(!executionContext.IsGlobalBroken, executedResults, sw.Elapsed);
            }
            catch
            {
                if (sw.IsRunning) sw.Stop();
                throw;
            }
            finally 
            {
                ArrayPool.Return(tasks, clearArray: true);
                ArrayPool.Return(stepResults, clearArray: true);
            }
        }

        private async Task<StepResult> RunStepAsync(int index, ExecutionContext ctx, ITypedPipelineContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var stepEntry = plan.Steps[index];
            var depIndices = stepEntry.DependencyIndices;

            if (depIndices.Length > 0)
            {
                var depTasks = ArrayPool.Rent<Task<StepResult>>(depIndices.Length);
                try
                {
                    for (int j = 0; j < depIndices.Length; j++)
                        depTasks[j] = ctx.tasks[depIndices[j]];

                    var depResults = await Task.WhenAll(depTasks);

                    if (policy == InterruptionPolicy.DependencyBased) 
                    {
                        for (int j = 0; j < depResults.Length; j++)
                        {
                            if (depResults[j].Flow != StepFlow.Continue)
                                return StepResult.Break();
                        }
                    }
                }
                finally 
                {
                    ArrayPool.Return(depTasks, clearArray: true);
                }
            }

            token.ThrowIfCancellationRequested();

            if (policy == InterruptionPolicy.Strict && ctx.IsGlobalBroken)
                return StepResult.Break();

            if (concurrencySemaphore != null)
                await concurrencySemaphore.WaitAsync(token);

            var stepSw = Stopwatch.StartNew();
            StepResult result;
            try
            {
                var stepper = new TaskBehaviorStepper<TKey>(stepEntry.Behaviors, 0, stepEntry.Step, context);
                result = await stepper.NextAsync(token);
            }
            catch (OperationCanceledException){ throw; }
            catch (Exception ex){ result = StepResult.Fail(ex); }
            finally
            {
                if(concurrencySemaphore != null) concurrencySemaphore.Release();
                stepSw.Stop();
            }

            if (result.Flow != StepFlow.Continue)
                ctx.IsGlobalBroken = true;

            ctx.stepResults[index] = new StepExecutionResult<TKey>(
                stepEntry.Step.Key,
                result.Flow == StepFlow.Continue,
                result.Flow,
                result.Exception,
                stepSw.Elapsed);

            return result;
        }

        private class ExecutionContext
        {
            public readonly Task<StepResult>[] tasks;
            public readonly StepExecutionResult<TKey>[] stepResults;
            public bool IsGlobalBroken;

            public ExecutionContext(Task<StepResult>[] tasks, StepExecutionResult<TKey>[] stepResults)
            {
                this.tasks = tasks;
                this.stepResults = stepResults;
            }
        }

        private readonly struct ExecutionPlan
        {
            public StepEntry[] Steps { get; }
            public Dictionary<IStep<TKey>, int> StepIndexMap { get; }

            public ExecutionPlan(StepEntry[] steps, Dictionary<IStep<TKey>, int> stepIndexMap)
            {
                Steps = steps;
                StepIndexMap = stepIndexMap;
            }
        }

        private readonly struct StepEntry
        {
            public ITaskStep<TKey> Step { get; }
            public int[] DependencyIndices { get; }
            public ITaskBehavior<TKey>[] Behaviors { get; }

            public StepEntry(ITaskStep<TKey> step, int[] dependencyIndices, ITaskBehavior<TKey>[] behaviors)
            {
                Step = step;
                DependencyIndices = dependencyIndices;
                Behaviors = behaviors;
            }
        }

        /// <summary>编排器构建器</summary>
        public class Builder
        {
            private readonly List<ITaskStep<TKey>> steps;
            private readonly Dictionary<Type, List<ITaskBehavior<TKey>>> behaviors;
            private InterruptionPolicy policy = InterruptionPolicy.DependencyBased;
            private int maxDegreeOfParallelism;
            private bool hasMaxDegreeOfParallelismSet;
            private bool built;

            private Builder() 
            {
                steps = ListPool.Rent<ITaskStep<TKey>>();
                behaviors = DictionaryPool.Rent<Type, List<ITaskBehavior<TKey>>>();
            }

            /// <summary>创建构建器</summary>
            /// <returns>新的构建器实例</returns>
            /// <remarks>
            /// <para>静态工厂方法，每次调用返回独立的构建器。</para>
            /// </remarks>
            public static Builder Create() { return new Builder(); }

            /// <summary>添加步骤</summary>
            /// <param name="step">步骤实例</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>步骤将根据其内部定义的依赖关系自动决定执行顺序，添加顺序不代表执行顺序。</para>
            /// <para>不能添加 null 步骤。</para>
            /// </remarks>
            public Builder AddStep(ITaskStep<TKey> step)
            { 
                ThrowIfBuilt();
                if (step == null) throw new ArgumentNullException(nameof(step));
                steps.Add(step); 
                return this; 
            }

            /// <summary>为指定步骤添加行为</summary>
            /// <typeparam name="TStep">步骤类型</typeparam>
            /// <param name="behavior">行为实例</param>
            /// <returns>构建器实例</returns>
            public Builder AddBehavior<TStep>(ITaskBehavior<TKey> behavior) where TStep : ITaskStep<TKey>
            { 
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                AddBehaviorInternal(typeof(TStep), behavior);
                return this; 
            }

            /// <summary>批量为多种步骤添加行为</summary>
            /// <typeparam name="TStep1">步骤类型1</typeparam>
            /// <typeparam name="TStep2">步骤类型2</typeparam>
            /// <param name="behavior">行为实例</param>
            /// <returns>构建器实例</returns>
            public Builder AddBehavior<TStep1, TStep2>(ITaskBehavior<TKey> behavior)
            where TStep1 : ITaskStep<TKey>
            where TStep2 : ITaskStep<TKey>
            {
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                AddBehaviorInternal(typeof(TStep1), behavior);
                AddBehaviorInternal(typeof(TStep2), behavior);
                return this;
            }

            /// <summary>批量为多种步骤添加行为</summary>
            /// <typeparam name="TStep1">步骤类型1</typeparam>
            /// <typeparam name="TStep2">步骤类型2</typeparam>
            /// <typeparam name="TStep3">步骤类型3</typeparam>
            /// <param name="behavior">行为实例</param>
            /// <returns>构建器实例</returns>
            public Builder AddBehavior<TStep1, TStep2, TStep3>(ITaskBehavior<TKey> behavior)
            where TStep1 : ITaskStep<TKey>
            where TStep2 : ITaskStep<TKey>
            where TStep3 : ITaskStep<TKey>
            {
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                AddBehaviorInternal(typeof(TStep1), behavior);
                AddBehaviorInternal(typeof(TStep2), behavior);
                AddBehaviorInternal(typeof(TStep3), behavior);
                return this;
            }

            /// <summary>
            /// 为指定步骤添加行为
            /// </summary>
            /// <param name="behavior">行为实例</param>
            /// <param name="stepType">步骤类型</param>
            /// <returns>构建器实例</returns>
            public Builder AddBehavior(ITaskBehavior<TKey> behavior, Type stepType) 
            {
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                AddBehaviorInternal(stepType, behavior);
                return this;
            }

            /// <summary>
            /// 批量为多种步骤添加行为
            /// </summary>
            /// <param name="behavior">行为实例</param>
            /// <param name="stepType1">步骤类型1</param>
            /// <param name="stepType2">步骤类型2</param>
            /// <returns>构建器实例</returns>
            public Builder AddBehavior(ITaskBehavior<TKey> behavior, Type stepType1, Type stepType2)
            {
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                AddBehaviorInternal(stepType1, behavior);
                AddBehaviorInternal(stepType2, behavior);
                return this;
            }

            /// <summary>
            /// 批量为多种步骤添加行为
            /// </summary>
            /// <param name="behavior">行为实例</param>
            /// <param name="stepType1">步骤类型1</param>
            /// <param name="stepType2">步骤类型2</param>
            /// <param name="stepType3">步骤类型3</param>
            /// <returns>构建器实例</returns>
            public Builder AddBehavior(ITaskBehavior<TKey> behavior, Type stepType1, Type stepType2, Type stepType3)
            {
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                AddBehaviorInternal(stepType1, behavior);
                AddBehaviorInternal(stepType2, behavior);
                AddBehaviorInternal(stepType3, behavior);
                return this;
            }

            /// <summary>批量为多种步骤添加行为</summary>
            /// <param name="behavior">行为实例</param>
            /// <param name="stepTypes">步骤行为数组</param>
            /// <returns>构建器实例</returns>
            public Builder AddBehavior(ITaskBehavior<TKey> behavior, params Type[] stepTypes)
            {
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                if (stepTypes == null || stepTypes.Length == 0) throw new ArgumentException($"[Orchestrator] The parameter '{nameof(stepTypes)}' cannot be null or empty.");
                for (int i = 0; i < stepTypes.Length; i++) 
                    AddBehaviorInternal(stepTypes[i], behavior);
                return this;
            }

            /// <summary>为当前所有步骤添加行为</summary>
            /// <param name="behavior">行为实例</param>
            /// <returns>构建器实例</returns>
            public Builder AddBehaviorForAll(ITaskBehavior<TKey> behavior)
            {
                ThrowIfBuilt();
                if (behavior == null) throw new ArgumentNullException(nameof(behavior));
                for (int i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    var stepType = step.GetType();
                    AddBehaviorInternal(stepType, behavior);
                }
                return this;
            }

            /// <summary>设置中断策略</summary>
            /// <param name="policy">策略</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>默认策略为 <see cref="InterruptionPolicy.DependencyBased"/>。</para>
            /// <para>仅在步骤返回非 <see cref="StepFlow.Continue"/> 时生效。</para>
            /// </remarks>
            public Builder UsePolicy(InterruptionPolicy policy) 
            { 
                ThrowIfBuilt(); 
                this.policy = policy; 
                return this; 
            }

            /// <summary>设置最大并发数</summary>
            /// <param name="count">最大并发数（必须大于0）</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>限制同时执行的步骤数量，超出数量时后续步骤会等待直到有可用资源。</para>
            /// <para>若未设置，则不限制并发量（但同时受 <see cref="SemaphoreSlim"/> 和任务调度器影响）。</para>
            /// </remarks>
            public Builder WithMaxConcurrency(int count) 
            {
                ThrowIfBuilt();
                maxDegreeOfParallelism = count > 0 ? count : 1;
                hasMaxDegreeOfParallelismSet = true;
                return this;
            }

            /// <summary>构建编排器</summary>
            /// <returns>配置完成的编排器实例</returns>
            /// <exception cref="InvalidOperationException">步骤集合为空、检测到循环依赖、无法确定最终步骤或检测到多个汇点步骤时抛出</exception>
            /// <remarks>
            /// <para>根据当前配置完成依赖分析、拓扑排序、管道编译和并发控制初始化。</para>
            /// <para>每个构建器实例只能调用一次 <see cref="Build"/>，之后不能再修改配置。</para>
            /// </remarks>
            public TaskOrchestrator<TKey> Build() 
            {
                try
                {
                    ThrowIfBuilt();
                    built = true;

                    if (steps.Count == 0)
                        throw new InvalidOperationException("[Orchestrator] Cannot build an orchestrator without any steps.");

                    if (!OrchestratorUtility.ValidateNoCycles(steps.Cast<IStep<TKey>>(), out var cycleSteps))
                        throw new InvalidOperationException($"[Orchestrator] Cycle detected in step dependencies. Involved steps: {string.Join(", ", cycleSteps)}");

                    var sortedSteps = OrchestratorUtility.TopologicalSort(steps.Cast<IStep<TKey>>()).Cast<ITaskStep<TKey>>().ToArray();
                    var stepIndexMap = new Dictionary<IStep<TKey>, int>();
                    for (int i = 0; i < sortedSteps.Length; i++)
                        stepIndexMap[sortedSteps[i]] = i;

                    var stepEntries = new StepEntry[sortedSteps.Length];
                    for (int i = 0; i < sortedSteps.Length; i++)
                    {
                        var step = sortedSteps[i];
                        int[] depIndices = Array.Empty<int>();
                        if (step.Dependencies != null && step.Dependencies.Count > 0)
                            depIndices = step.Dependencies.Select(d => stepIndexMap[d]).ToArray();

                        var behaviorsArray = behaviors.TryGetValue(step.GetType(), out var list)
                        ? list.ToArray()
                        : Array.Empty<ITaskBehavior<TKey>>();

                        stepEntries[i] = new StepEntry(step, depIndices, behaviorsArray);
                    }

                    var plan = new ExecutionPlan(stepEntries, stepIndexMap);
                    var semaphore = hasMaxDegreeOfParallelismSet ? new SemaphoreSlim(maxDegreeOfParallelism) : null;

                    return new TaskOrchestrator<TKey>()
                    {
                        plan = plan,
                        policy = policy,
                        concurrencySemaphore = semaphore
                    };
                }
                finally 
                {
                    ListPool.Return(steps);
                    foreach (var list in behaviors.Values)
                        ListPool.Return(list);
                    behaviors.Clear();
                    DictionaryPool.Return(behaviors);
                }
            }

            private void AddBehaviorInternal(Type stepType, ITaskBehavior<TKey> behavior)
            {
                if(stepType == null) throw new ArgumentNullException(nameof(stepType));
                if (!behaviors.TryGetValue(stepType, out var list))
                {
                    list = ListPool.Rent<ITaskBehavior<TKey>>();
                    behaviors[stepType] = list;
                }
                list.Add(behavior);
            }

            private void ThrowIfBuilt()
            {
                if (built)
                    throw new InvalidOperationException("[Orchestrator] The builder cannot be reused.");
            }
        }
    }
}