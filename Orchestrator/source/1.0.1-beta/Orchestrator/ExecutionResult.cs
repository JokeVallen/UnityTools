using System;
using System.Collections.Generic;

namespace Orchestrator
{
    /// <summary>编排结果</summary>
    /// <typeparam name="TKey">步骤唯一标识类型</typeparam>
    public readonly struct ExecutionResult<TKey>
    {
        /// <summary>
        /// 整体执行是否成功
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 步骤结果列表
        /// </summary>
        public IReadOnlyCollection<StepExecutionResult<TKey>> StepResults { get; }

        /// <summary>
        /// 总执行耗时
        /// </summary>
        public TimeSpan Duration { get; }

        /// <param name="success">整体执行是否成功</param>
        /// <param name="stepResults">步骤结果列表</param>
        /// <param name="duration">总执行耗时</param>
        public ExecutionResult(bool success, IReadOnlyCollection<StepExecutionResult<TKey>> stepResults, TimeSpan duration)
        {
            Success = success;
            StepResults = stepResults;
            Duration = duration;
        }
    }
}