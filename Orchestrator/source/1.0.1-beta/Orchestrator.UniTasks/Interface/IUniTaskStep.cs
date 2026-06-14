using Cysharp.Threading.Tasks;
using System.Threading;

namespace Orchestrator.UniTasks
{
    /// <summary>异步步骤</summary>
    /// <typeparam name="TKey">步骤唯一标识的类型</typeparam>
    public interface IUniTaskStep<TKey> : IStep<TKey>
    {
        /// <summary>异步执行</summary>
        /// <param name="context">上下文</param>
        /// <param name="token">取消令牌</param>
        /// <returns>步骤执行结果</returns>
        UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token);
    }
}
