namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 管道会话接口
    /// </summary>
    public interface IPipelineSession : ISessionKeyGetter
    {
        /// <summary>
        /// 管道是否已执行完成
        /// </summary>
        bool IsTerminalReached { get; }

        /// <summary>
        /// 管道是否已中断执行
        /// </summary>
        bool IsAborted { get; }

        /// <summary>
        /// 管道执行方向
        /// </summary>
        PipelineDirection Direction { get; }
    }
}
