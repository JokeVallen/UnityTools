using Cysharp.Threading.Tasks;
using System.Threading;

namespace Orchestrator.UniTasks
{
    /// <summary>横切行为</summary>
    /// <typeparam name="TKey">步骤唯一标识的类型</typeparam>
    public interface IUniTaskBehavior<TKey>
    {
        /// <summary>异步环绕处理</summary>
        /// <param name="context">上下文</param>
        /// <param name="stepper">步进器</param>
        /// <param name="token">取消令牌</param>
        /// <returns>步骤执行结果</returns>
        UniTask<StepResult> HandleAsync(ITypedPipelineContext context, UniTaskBehaviorStepper<TKey> stepper, CancellationToken token);
    }
}
