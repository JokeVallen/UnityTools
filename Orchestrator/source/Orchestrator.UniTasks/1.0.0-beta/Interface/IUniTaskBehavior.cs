using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Orchestrator.UniTasks
{
    /// <summary>横切关注点行为</summary>
    /// <remarks>
    /// <para>用于定义工作流中的通用处理逻辑，如日志记录、性能监控、错误重试或权限校验。</para>
    /// <para>通过装饰器模式或拦截器模式实现，允许在不修改步骤逻辑的情况下扩展功能。</para>
    /// </remarks>
    public interface IUniTaskBehavior<TIn, TOut>
    {
        /// <summary>异步环绕处理</summary>
        /// <param name="input">工作流输入</param>
        /// <param name="next">后续逻辑委托</param>
        /// <param name="token">取消令牌</param>
        /// <remarks>
        /// <para>该方法包裹了下一个行为或最终步骤的执行过程。</para>
        /// <code>
        /// public async Task&lt;StepResult&lt;TOut&gt;&gt; HandleAsync(TIn input, Func&lt;Task&lt;StepResult&lt;TOut&gt;&gt;&gt; next, CancellationToken token)
        /// {
        ///     // 前置处理：例如记录日志
        ///     var result = await next(); 
        ///     // 后置处理：例如统计耗时
        ///     return result;
        /// }
        /// </code>
        /// </remarks>
        UniTask<StepResult<TOut>> HandleAsync(TIn input, Func<UniTask<StepResult<TOut>>> next, CancellationToken token);
    }
}
