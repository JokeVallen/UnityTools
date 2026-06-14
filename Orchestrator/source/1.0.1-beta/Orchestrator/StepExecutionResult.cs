using System;

namespace Orchestrator
{
    /// <summary>步骤结果</summary>
    /// <typeparam name="TKey">步骤唯一标识类型</typeparam>
    public readonly struct StepExecutionResult<TKey>
    {
        /// <summary>执行步骤的唯一标识</summary>
        public Optional<TKey> StepKey { get; }

        /// <summary>步骤是否执行成功</summary>
        public bool Success { get; }

        /// <summary>步骤流转状态</summary>
        public StepFlow Flow { get; }

        /// <summary>执行过程中发生的异常</summary>
        public Exception Exception { get; }

        /// <summary>该步骤执行所消耗的总时长</summary>
        public TimeSpan Duration { get; }

        /// <param name="stepKey">执行步骤的唯一标识</param>
        /// <param name="success">步骤是否执行成功</param>
        /// <param name="flow">步骤流转状态</param>
        /// <param name="exception">执行过程中发生的异常</param>
        /// <param name="duration">该步骤执行所消耗的总时长</param>
        public StepExecutionResult(TKey stepKey, bool success, StepFlow flow, Exception exception, TimeSpan duration)
        {
            StepKey = stepKey;
            Success = success;
            Flow = flow;
            Exception = exception;
            Duration = duration;
        }
    }
}