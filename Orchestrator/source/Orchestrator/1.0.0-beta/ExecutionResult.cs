using System;
using System.Collections.Generic;

namespace Orchestrator
{
    /// <summary>编排结果</summary>
    /// <remarks>
    /// <para>记录了从开始到结束的所有步骤快照及最终产出物。</para>
    /// <para>由执行引擎在工作流结束时统一实例化。</para>
    /// </remarks>
    public readonly struct ExecutionResult<T> : IExecutionResult<T, StepExecutionResult>
    {
        /// <summary>整体执行状态</summary>
        /// <inheritdoc cref="IExecutionResult{T, TStepResult}.Success"/>
        public bool Success { get; }

        /// <summary>最终产出数据</summary>
        /// <inheritdoc cref="IExecutionResult{T, TStepResult}.Output"/>
        public T Output { get; }

        /// <summary>步骤结果列表</summary>
        /// <inheritdoc cref="IExecutionResult{T, TStepResult}.StepResults"/>
        public IReadOnlyCollection<StepExecutionResult> StepResults { get; }

        /// <summary>总执行耗时</summary>
        /// <inheritdoc cref="IExecutionResult{T, TStepResult}.Duration"/>
        public TimeSpan Duration { get; }

        /// <summary>初始化流程结果</summary>
        /// <param name="success">成功状态</param>
        /// <param name="output">最终产出</param>
        /// <param name="stepResults">步骤快照</param>
        /// <param name="duration">总耗时</param>
        /// <remarks>
        /// <para>由执行引擎在工作流结束时统一实例化。</para>
        /// </remarks>
        public ExecutionResult(bool success, T output, IReadOnlyCollection<StepExecutionResult> stepResults, TimeSpan duration)
        {
            Success = success;
            Output = output;
            StepResults = stepResults;
            Duration = duration;
        }
    }
}