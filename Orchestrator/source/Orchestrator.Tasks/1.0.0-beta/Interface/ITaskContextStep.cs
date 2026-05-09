using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tasks
{
    /// <summary>上下文步骤</summary>
    /// <typeparam name="TContext">上下文类型</typeparam>
    /// <remarks>
    /// <para>基于共享上下文的异构步骤接口。多个步骤共享并修改同一个 <typeparamref name="TContext"/> 对象。</para>
    /// <para>与 <see cref="ITaskStep{TIn, TOut}"/> 不同，此接口不产生独立的输出，而是通过副作用修改上下文。</para>
    /// </remarks>
    public interface ITaskContextStep<TContext> : IStep
    {
        /// <summary>异步执行</summary>
        /// <param name="context">共享上下文</param>
        /// <param name="token">取消令牌</param>
        /// <returns>流转状态</returns>
        /// <remarks>
        /// <para>执行时直接操作 <paramref name="context"/> 对象，引擎据此决定后续执行。</para>
        /// <para>示例：<code>public async Task&lt;StepFlow&gt; ExecuteAsync(MyContext ctx, CancellationToken token)</code></para>
        /// </remarks>
        Task<StepFlow> ExecuteAsync(TContext context, CancellationToken token);
    }
}