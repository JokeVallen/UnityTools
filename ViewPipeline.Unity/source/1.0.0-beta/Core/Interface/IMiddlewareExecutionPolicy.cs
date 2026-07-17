namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 中间件执行策略
    /// </summary>
    public interface IMiddlewareExecutionPolicy
    {
        /// <summary>
        /// 判断指定视图是否应跳过指定中间件
        /// </summary>
        bool ShouldSkip(IView view, IViewMiddleware middleware);
    }
}
