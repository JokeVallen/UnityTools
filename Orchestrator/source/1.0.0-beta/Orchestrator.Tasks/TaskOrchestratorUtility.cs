using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tasks
{
    /// <summary>任务编排工具类</summary>
    /// <remarks>
    /// <para>辅助类，用于编译行为管道。</para>
    /// </remarks>
    public static class TaskOrchestratorUtility
    {
        /// <summary>编译行为管道</summary>
        /// <param name="step">步骤实例</param>
        /// <param name="behaviors">行为列表</param>
        /// <returns>编译后的执行委托</returns>
        /// <remarks>
        /// <para>将一系列 <see cref="ITaskBehavior{TIn, TOut}"/> 按添加顺序包装在步骤执行之外，形成管道。</para>
        /// <para>若行为列表为空，则直接返回步骤的 <see cref="ITaskStep{TIn, TOut}.ExecuteAsync"/> 方法。</para>
        /// </remarks>
        public static Func<TIn, CancellationToken, Task<StepResult<TOut>>> CompilePipeline<TIn, TOut>(
            ITaskStep<TIn, TOut> step,
            List<ITaskBehavior<TIn, TOut>> behaviors)
        {
            Func<TIn, CancellationToken, Task<StepResult<TOut>>> inner = (input, ct) => step.ExecuteAsync(input, ct);

            if (behaviors == null || behaviors.Count == 0)
                return inner;

            for (int i = behaviors.Count - 1; i >= 0; i--)
            {
                var behavior = behaviors[i];
                var currentInner = inner;
                inner = (input, ct) => behavior.HandleAsync(input, () => currentInner(input, ct), ct);
            }

            return inner;
        }
    }
}