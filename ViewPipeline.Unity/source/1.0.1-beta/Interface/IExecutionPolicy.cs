namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 执行策略接口
    /// </summary>
    public interface IExecutionPolicy
    {
        /// <summary>
        /// 判断指定视图是否应跳过指定中间件的处理
        /// </summary>
        /// <param name="view">视图</param>
        /// <param name="middleware">中间件</param>
        /// <returns>跳过则返回 true，否则返回 false。</returns>
        bool ShouldSkipMiddleware(IView view, IViewMiddleware middleware);

        /// <summary>
        /// 判断指定中间件是否应跳过对指定视图的处理
        /// </summary>
        /// <param name="middleware">中间件</param>
        /// <param name="view">视图</param>
        /// <returns>跳过则返回 true，否则返回 false。</returns>
        bool ShouldSkipView(IViewMiddleware middleware, IView view);

        /// <summary>
        /// 是否终止执行流程
        /// </summary>
        /// <param name="view">视图</param>
        /// <returns>终止则返回 true，否则返回 false。</returns>
        bool ShouldTerminate(IView view);

        /// <summary>
        /// 是否终止执行流程
        /// </summary>
        /// <param name="middleware">中间件</param>
        /// <returns>终止则返回 true，否则返回 false。</returns>
        bool ShouldTerminate(IViewMiddleware middleware);
    }
}
