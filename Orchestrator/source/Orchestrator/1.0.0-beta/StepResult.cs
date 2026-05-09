using System;

namespace Orchestrator
{
    /// <summary>步骤执行结果</summary>
    /// <remarks>
    /// <para>用于封装单步执行后的产出数据、流转状态以及可能发生的异常。</para>
    /// <para>通过内置的静态工厂方法 <see cref="Continue"/>, <see cref="Break"/> 或 <see cref="Fail"/> 进行实例化。</para>
    /// </remarks>
    public readonly struct StepResult<T>
    {
        /// <summary>流转状态</summary>
        /// <remarks>
        /// <para>决定执行引擎下一步的操作（继续、中断或报错）。</para>
        /// </remarks>
        public StepFlow Flow { get; }

        /// <summary>输出数据</summary>
        /// <remarks>
        /// <para>当前步骤计算或获取的业务对象。</para>
        /// </remarks>
        public T Output { get; }

        /// <summary>捕获异常</summary>
        /// <remarks>
        /// <para>当 <see cref="Flow"/> 为 <see cref="StepFlow.Fail"/> 时，包含具体的异常实例。</para>
        /// </remarks>
        public Exception Exception { get; }

        private StepResult(StepFlow flow, T output, Exception exception)
        {
            Flow = flow;
            Output = output;
            Exception = exception;
        }

        /// <summary>创建成功结果</summary>
        /// <param name="output">产出数据</param>
        /// <returns>继续执行的结果</returns>
        /// <remarks>
        /// <para>标记步骤成功，并通知引擎继续执行后续节点。</para>
        /// <para>示例：<code>return StepResult&lt;string&gt;.Continue("处理完成");</code></para>
        /// </remarks>
        public static StepResult<T> Continue(T output) => new StepResult<T>(StepFlow.Continue, output, null);

        /// <summary>创建中断结果</summary>
        /// <param name="output">产出数据</param>
        /// <returns>中断执行的结果</returns>
        /// <remarks>
        /// <para>标记业务逻辑正常中断，引擎将停止当前路径的后续执行，但不视为错误。</para>
        /// <para>若不提供 <paramref name="output"/>，则使用默认值。</para>
        /// </remarks>
        public static StepResult<T> Break(T output = default) => new StepResult<T>(StepFlow.Break, output, null);

        /// <summary>创建失败结果</summary>
        /// <param name="ex">异常实例</param>
        /// <returns>失败结果</returns>
        /// <remarks>
        /// <para>标记执行发生故障，通常会触发执行引擎的异常处理流程。</para>
        /// <para>示例：<code>return StepResult&lt;int&gt;.Fail(new InvalidOperationException("数据无效"));</code></para>
        /// </remarks>
        public static StepResult<T> Fail(Exception ex) => new StepResult<T>(StepFlow.Fail, default, ex);
    }
}