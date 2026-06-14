using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tasks
{
    /// <summary>横切行为</summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <typeparam name="TOut">输出类型</typeparam>
    /// <remarks>
    /// <para>用于定义工作流中的通用处理逻辑，如日志记录、性能监控、错误重试或权限校验。</para>
    /// <para>通过装饰器模式或拦截器模式实现，允许在不修改步骤逻辑的情况下扩展功能。</para>
    /// </remarks>
    public interface ITaskBehavior<TIn, TOut>
    {
        /// <summary>环绕处理</summary>
        /// <param name="input">输入数据</param>
        /// <param name="next">后续委托</param>
        /// <param name="token">取消令牌</param>
        /// <returns>步骤执行结果</returns>
        /// <remarks>
        /// <para>该方法包裹了下一个行为或最终步骤的执行过程，在调用 <paramref name="next"/> 前后可插入前置/后置逻辑。</para>
        /// <code>
        /// public async Task&lt;StepResult&lt;TOut&gt;&gt; HandleAsync(TIn input, Func&lt;Task&lt;StepResult&lt;TOut&gt;&gt;&gt; next, CancellationToken token)
        /// {
        ///     // 前置处理：日志开始
        ///     var result = await next();
        ///     // 后置处理：记录耗时
        ///     return result;
        /// }
        /// </code>
        /// </remarks>
        Task<StepResult<TOut>> HandleAsync(TIn input, Func<Task<StepResult<TOut>>> next, CancellationToken token);
    }
}