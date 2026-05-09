using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tasks
{
    /// <summary>异步步骤</summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <typeparam name="TOut">输出类型</typeparam>
    /// <remarks>
    /// <para>定义了一个可异步执行的工作流步骤，具有明确的输入和输出类型。</para>
    /// <para>继承自 <see cref="IStep"/>，因此也具备名称和依赖关系等元数据。</para>
    /// </remarks>
    public interface ITaskStep<TIn, TOut> : IStep
    {
        /// <summary>异步执行</summary>
        /// <param name="input">输入数据</param>
        /// <param name="token">取消令牌</param>
        /// <returns>步骤执行结果</returns>
        /// <remarks>
        /// <para>包含该步骤的具体业务实现。执行完成后应返回 <see cref="StepResult{T}"/> 以指示后续流转状态。</para>
        /// <para>示例：<code>public async Task&lt;StepResult&lt;string&gt;&gt; ExecuteAsync(string input, CancellationToken token)</code></para>
        /// </remarks>
        Task<StepResult<TOut>> ExecuteAsync(TIn input, CancellationToken token);
    }
}