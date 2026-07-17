namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图管线执行器的快照
    /// </summary>
    public readonly struct ViewPipelineExecutorSnapshot
    {
        /// <summary>
        /// 当前索引
        /// </summary>
        public int CurrentIndex { get; }

        /// <summary>
        /// 管线会话快照
        /// </summary>
        public PipelineSessionSnapshot PipelineSession { get; }

        /// <summary>
        /// 中间件快照数组
        /// </summary>
        public MiddlewareSnapshot[] Middlewares { get; }

        /// <summary>
        /// 有效长度
        /// </summary>
        public int ValidLength { get; }

        internal static readonly ViewPipelineExecutorSnapshot Empty = new ViewPipelineExecutorSnapshot();

        internal ViewPipelineExecutorSnapshot(int currentIndex, PipelineSessionSnapshot pipelineSession, MiddlewareSnapshot[] middlewares, int validLength)
        {
            CurrentIndex = currentIndex;
            PipelineSession = pipelineSession;
            Middlewares = middlewares;
            ValidLength = validLength;
        }
    }
}
