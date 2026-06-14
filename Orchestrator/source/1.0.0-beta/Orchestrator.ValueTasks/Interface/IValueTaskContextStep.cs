using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.ValueTasks
{
    /// <summary>
    /// 基于共享上下文的异构步骤接口。
    /// </summary>
    /// <typeparam name="TContext">上下文类型，步骤间共享并修改</typeparam>
    public interface IValueTaskContextStep<TContext> : IStep
    {
        /// <summary>异步执行，直接操作上下文</summary>
        /// <param name="context">共享上下文</param>
        /// <param name="token">取消令牌（必须显式提供）</param>
        /// <returns>流转状态，引擎据此决定后续执行。</returns>
        ValueTask<StepFlow> ExecuteAsync(TContext context, CancellationToken token);
    }
}
