using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tasks
{
    /// <summary>异步步骤</summary>
    /// <typeparam name="TKey">步骤唯一标识的类型</typeparam>
    public interface ITaskStep<TKey> : IStep<TKey>
    {
        /// <summary>异步执行</summary>
        /// <param name="context">上下文</param>
        /// <param name="token">取消令牌</param>
        /// <returns>步骤执行结果</returns>
        Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token);
    }
}