using System;

namespace Orchestrator
{
    /// <summary>步骤结果</summary>
    /// <remarks>
    /// <para>该类包含单次步骤执行的审计信息，如耗时、是否成功以及产生的异常（如果有）。</para>
    /// <para>通常由执行引擎内部创建，用于填充 <see cref="ExecutionResult{T}"/> 的步骤列表。</para>
    /// </remarks>
    public readonly struct StepExecutionResult : IStepExecutionResult
    {
        /// <summary>执行步骤的名称</summary>
        /// <inheritdoc cref="IStepExecutionResult.StepName"/>
        public string StepName { get; }

        /// <summary>步骤是否执行成功</summary>
        /// <inheritdoc cref="IStepExecutionResult.Success"/>
        public bool Success { get; }

        /// <summary>步骤流转状态</summary>
        /// <inheritdoc cref="IStepExecutionResult.Flow"/>
        public StepFlow Flow { get; }

        /// <summary>该步骤产生的原始输出对象</summary>
        /// <inheritdoc cref="IStepExecutionResult.Output"/>
        public object Output { get; }

        /// <summary>执行过程中发生的异常</summary>
        /// <inheritdoc cref="IStepExecutionResult.Exception"/>
        public Exception Exception { get; }

        /// <summary>该步骤执行所消耗的总时长</summary>
        /// <inheritdoc cref="IStepExecutionResult.Duration"/>
        public TimeSpan Duration { get; }

        /// <summary>初始化步骤结果</summary>
        /// <param name="stepName">步骤名称</param>
        /// <param name="success">成功状态</param>
        /// <param name="flow">流向状态</param>
        /// <param name="output">输出数据</param>
        /// <param name="exception">异常信息</param>
        /// <param name="duration">单步耗时</param>
        /// <remarks>
        /// <para>用于在流水线完成后进行数据回溯或日志审计。</para>
        /// <para>示例：</para>
        /// <code>
        /// var result = new StepExecutionResult(
        ///     "DataLoader",
        ///     true,
        ///     StepFlow.Continue,
        ///     loadedData,
        ///     null,
        ///     TimeSpan.FromMilliseconds(120)
        /// );
        /// </code>
        /// </remarks>
        public StepExecutionResult(string stepName, bool success, StepFlow flow, object output, Exception exception, TimeSpan duration)
        {
            StepName = stepName;
            Success = success;
            Flow = flow;
            Output = output;
            Exception = exception;
            Duration = duration;
        }
    }
}