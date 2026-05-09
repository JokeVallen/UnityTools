using System;

namespace Orchestrator
{
    /// <summary>单个步骤执行结果</summary>
    /// <remarks>
    /// <para>定义单个步骤执行后的结果快照，包含步骤名称、成功状态、流向、输出、异常及耗时。</para>
    /// </remarks>
    public interface IStepExecutionResult
    {
        /// <summary>执行步骤的名称</summary>
        string StepName { get; }

        /// <summary>步骤是否执行成功</summary>
        /// <remarks>
        /// <para>当步骤未抛出异常且未标记为 <see cref="StepFlow.Fail"/> 时返回 true。</para>
        /// </remarks>
        bool Success { get; }

        /// <summary>步骤流转状态</summary>
        StepFlow Flow { get; }

        /// <summary>该步骤产生的原始输出对象</summary>
        object Output { get; }

        /// <summary>执行过程中发生的异常</summary>
        /// <remarks><para>如果未发生异常，则为 null。</para></remarks>
        Exception Exception { get; }

        /// <summary>该步骤执行所消耗的总时长</summary>
        TimeSpan Duration { get; }
    }
}