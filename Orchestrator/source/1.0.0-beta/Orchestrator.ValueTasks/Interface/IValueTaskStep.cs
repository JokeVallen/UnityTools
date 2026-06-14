using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.ValueTasks
{
    /// <summary>编排执行单元</summary>
    public interface IValueTaskStep<TIn, TOut> : IStep
    {
        /// <summary>异步执行业务</summary>
        /// <param name="input">原始输入数据</param>
        /// <param name="token">取消令牌</param>
        /// <remarks>
        /// <para>包含该步骤的具体业务实现。执行完成后应返回 <see cref="StepResult{T}"/> 以指示后续流转状态。</para>
        /// </remarks>
        ValueTask<StepResult<TOut>> ExecuteAsync(TIn input, CancellationToken token);
    }
}
