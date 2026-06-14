using System;
using System.Collections.Generic;

namespace Orchestrator
{
    /// <summary>编排任务最终结果</summary>
    /// <remarks>
    /// <para>描述整个工作流执行完毕后的最终快照。</para>
    /// <para>包含总体的成功状态、最终产出、各步骤的详细执行过程以及总耗时统计。</para>
    /// </remarks>
    public interface IExecutionResult<out T, TStepResult> where TStepResult : IStepExecutionResult
    {
        /// <summary>整体执行状态</summary>
        /// <remarks>
        /// <para>若工作流中所有必须步骤均成功完成，则返回 true。</para>
        /// </remarks>
        bool Success { get; }

        /// <summary>最终产出数据</summary>
        /// <remarks>
        /// <para>通常指工作流中最后一个成功步骤的输出结果。</para>
        /// </remarks>
        T Output { get; }

        /// <summary>步骤结果列表</summary>
        /// <remarks>
        /// <para>按执行顺序记录的每个步骤执行详情，用于审计或回溯。</para>
        /// </remarks>
        IReadOnlyCollection<TStepResult> StepResults { get; }

        /// <summary>总执行耗时</summary>
        /// <remarks>
        /// <para>从启动第一个步骤到最后一个步骤结束的 TimeSpan 时间跨度。</para>
        /// </remarks>
        TimeSpan Duration { get; }
    }
}