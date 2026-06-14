using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator
{
    /// <summary>横切关注点行为</summary>
    /// <remarks>
    /// <para>用于定义工作流中的通用处理逻辑，如日志记录、性能监控、错误重试或权限校验。</para>
    /// <para>通过装饰器模式或拦截器模式实现，允许在不修改步骤逻辑的情况下扩展功能。</para>
    /// </remarks>
    public interface IValueTaskBehavior<TIn, TOut>
    {
        /// <summary>异步环绕处理</summary>
        /// <param name="input">工作流输入</param>
        /// <param name="next">后续逻辑委托</param>
        /// <param name="token">取消令牌</param>
        ValueTask<StepResult<TOut>> HandleAsync(TIn input, Func<ValueTask<StepResult<TOut>>> next, CancellationToken token);
    }
}
