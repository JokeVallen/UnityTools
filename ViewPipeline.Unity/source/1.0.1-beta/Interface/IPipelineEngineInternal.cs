using System.Threading;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    internal interface IPipelineEngineInternal : IPipelineEngine, ISessionKeyGetter
    {
        PipelineDirection Direction { get; }
        int ActiveOperations { get; }
        UniTask ExecuteAsync(IView view, IPipelineContext context, IPipelineSession session, CancellationToken token);
    }
}
