using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 管道执行器
    /// </summary>
    public readonly struct UIPipelineExecutor
    {
        /// <summary>
        /// 当前索引
        /// </summary>
        public int CurrentIndex => index;

        /// <summary>
        /// 执行管道上下文
        /// </summary>
        public IPipelineContext Context => context;

        /// <summary>
        /// 管道会话实例
        /// </summary>
        public IPipelineSession Session => session;

        private readonly Guid key;
        private readonly IViewMiddleware[] flatArray;
        private readonly int validLength;
        private readonly int index;
        private readonly IPipelineContext context;
        private readonly PipelineSession session;

        /// <param name="flatArray">扁平化中间件数组</param>
        /// <param name="validLength">有效长度</param>
        /// <param name="index">起始索引</param>
        /// <param name="context">管道执行上下文</param>
        internal UIPipelineExecutor(Guid key, IViewMiddleware[] flatArray, int validLength, int index, IPipelineContext context, PipelineSession session)
        {
            this.key = key;
            this.flatArray = flatArray;
            this.validLength = validLength;
            this.index = index;
            this.context = context;
            this.session = session;
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

            if (session != null && index > session.MaxExecutedIndex)
                session.MaxExecutedIndex = index;

            if (index >= validLength)
            {
                if (session != null) session.Complete();

                if (session.Direction == PipelineDirection.Close)
                {
                    await view.HideAsync(token);
                }
                else
                {
                    await view.ShowAsync(token);
                }
                return;
            }

            var currentMiddleware = flatArray[index];
            var nextExecutor = new UIPipelineExecutor(key, flatArray, validLength, index + 1, context, session);
            int targetNextIndex = nextExecutor.CurrentIndex;

            try
            {
                if (ExecutionPolicy.ShouldSkip(key, view, currentMiddleware)) 
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

            if (session != null && session.MaxExecutedIndex < targetNextIndex)
            {
                if (session.IsAborted) return;
                throw new InvalidOperationException($"[ViewPipeline] Detected that extension package [{currentMiddleware.GetType().Name}] did not explicitly interrupt nor call NextAsync() to proceed!");
            }
        }

        /// <summary>
        /// 中断执行
        /// </summary>
        public void Abort() 
        {
            if (session == null) return;
            session.Abort();
        }
    }
}
