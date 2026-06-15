using System;

namespace Orchestrator
{
    /// <summary>编排结果</summary>
    public readonly struct ExecutionResult
    {
        /// <summary>
        /// 整体执行是否成功
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 总执行耗时
        /// </summary>
        public TimeSpan Duration { get; }

        /// <param name="success">整体执行是否成功</param>
        /// <param name="duration">总执行耗时</param>
        public ExecutionResult(bool success, TimeSpan duration)
        {
            Success = success;
            Duration = duration;
        }
    }
}