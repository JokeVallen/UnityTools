namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图会话扩展接口
    /// </summary>
    public interface IExtendedViewSession : IViewSession, IAsyncDisposable, ISessionKeyGetter
    {
        /// <summary>
        /// 注册动态中间件流式供应器
        /// </summary>
        /// <param name="direction">管线执行方向</param>
        /// <param name="provider">动态中间件流式供应器</param>
        void RegisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider);

        /// <summary>
        /// 注销动态中间件流式供应器
        /// </summary>
        /// <param name="direction">管线执行方向</param>
        /// <param name="provider">动态中间件流式供应器</param>
        void UnregisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider);
    }
}
