using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图管道执行器
    /// </summary>
    public readonly struct ViewPipelineExecutor
    {
        /// <summary>
        /// 当前索引
        /// </summary>
        public int CurrentIndex => currentIndex;

        /// <summary>
        /// 管道上下文
        /// </summary>
        public IPipelineContext Context => context;

        /// <summary>
        /// 管道会话实例
        /// </summary>
        public IPipelineSession Session => pipelineSession;

        private readonly IViewMiddleware[] flatArray;
        private readonly int validLength;
        private readonly int currentIndex;
        private readonly IPipelineContext context;
        private readonly PipelineSession pipelineSession;

        internal ViewPipelineExecutor(IViewMiddleware[] flatArray, int validLength, int currentIndex, IPipelineContext context, PipelineSession pipelineSession)
        {
            this.flatArray = flatArray;
            this.validLength = validLength;
            this.currentIndex = currentIndex;
            this.context = context;
            this.pipelineSession = pipelineSession;
        }

        /// <summary>
        /// 异步步进到下一个阶段
        /// </summary>
        /// <param name="view">视图实例</param>
        /// <param name="token">异步取消令牌</param>
        /// <returns>异步任务句柄</returns>
        public async UniTask NextAsync(IView view, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (pipelineSession != null && currentIndex > pipelineSession.MaxExecutedIndex)
                pipelineSession.MaxExecutedIndex = currentIndex;

            if (currentIndex >= validLength)
            {
                if (pipelineSession != null) pipelineSession.Complete();

                if (pipelineSession.Direction == PipelineDirection.Close)
                {
                    if (ViewPipelineUtility.ShouldTerminate(pipelineSession.Key, view)) return;
                    await view.HideAsync(token);
                }
                else
                {
                    if (ViewPipelineUtility.ShouldTerminate(pipelineSession.Key, view)) return;
                    await view.ShowAsync(token);
                }
                return;
            }

            var currentMiddleware = flatArray[currentIndex];
            var nextExecutor = new ViewPipelineExecutor(flatArray, validLength, currentIndex + 1, context, pipelineSession);
            int targetNextIndex = nextExecutor.CurrentIndex;

            try
            {
                if (ViewPipelineUtility.ShouldTerminate(pipelineSession.Key, view) || ViewPipelineUtility.ShouldTerminate(pipelineSession.Key, currentMiddleware))
                {
                    pipelineSession.Abort();
                    return;
                }

                if (ViewPipelineUtility.ShouldSkipView(pipelineSession.Key, currentMiddleware, view) 
                || ViewPipelineUtility.ShouldSkipMiddleware(pipelineSession.Key, view, currentMiddleware)) 
                { 
                    await nextExecutor.NextAsync(view, token);
                    return;
                }

                await currentMiddleware.InvokeAsync(view, nextExecutor, token);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Logger.Error($"[ViewPipeline] Caught a runtime hard crash in third-party extension aspect [{currentMiddleware.GetType().Name}]:\n{ex}");
                throw;
            }

            if (pipelineSession != null && pipelineSession.MaxExecutedIndex < targetNextIndex)
            {
                if (pipelineSession.IsAborted) return;
                throw new InvalidOperationException($"[ViewPipeline] Detected that extension package [{currentMiddleware.GetType().Name}] did not explicitly interrupt nor call NextAsync() to proceed!");
            }
        }

        /// <summary>
        /// 中断执行
        /// </summary>
        public void Abort() 
        {
            if (pipelineSession == null) return;
            pipelineSession.Abort();
        }

        /// <summary>
        /// 获取完整快照
        /// </summary>
        /// <returns>完整快照</returns>
        public ViewPipelineExecutorSnapshot GetFullSnapshot()
        {
            var snapshot = new ViewPipelineExecutorSnapshot(
                currentIndex,
                pipelineSession is IFullSnapshotable<PipelineSessionSnapshot> typed ? typed.GetFullSnapshot() : PipelineSessionSnapshot.Empty,
                flatArray.Where(m => m is IFullSnapshotable<MiddlewareSnapshot>).Select(m => ((IFullSnapshotable<MiddlewareSnapshot>)m).GetFullSnapshot()).ToArray(),
                validLength
            );
            return snapshot;
        }
    }
}
