namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 管线引擎接口
    /// </summary>
    internal interface IPipelineEngine
    {
        /// <summary>
        /// 注册动态中间件流式供应器
        /// </summary>
        /// <param name="provider">动态中间件流式供应器</param>
        void RegisterDynamicProvider(IDynamicMiddlewareProvider provider);

        /// <summary>
        /// 注销动态中间件流式供应器
        /// </summary>
        /// <param name="provider">动态中间件流式供应器</param>
        void UnregisterDynamicProvider(IDynamicMiddlewareProvider provider);
    }
}
