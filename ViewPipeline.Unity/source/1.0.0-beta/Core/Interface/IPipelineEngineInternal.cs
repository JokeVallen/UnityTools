using Cysharp.Threading.Tasks;
using System.Threading;

namespace ViewPipeline.Unity.Core
{
    internal interface IPipelineEngineInternal : IPipelineEngine
    {
        /// <summary>
        /// 异步驱动管线引擎
        /// </summary>
        /// <param name="view">当前操作的视图实例</param>
        /// <param name="context">管道上下文</param>
        /// <param name="token">异步取消令牌</param>
        /// <returns>异步任务句柄</returns>
        UniTask ExecuteAsync(IView view, IPipelineContext context, IPipelineSession session, CancellationToken token);
    }
}
