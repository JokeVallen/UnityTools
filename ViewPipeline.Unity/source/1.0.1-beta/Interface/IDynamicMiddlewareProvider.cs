namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 动态中间件流式供应器接口
    /// </summary>
    public interface IDynamicMiddlewareProvider
    {
        /// <summary>
        /// 运行时向动态中间件可增集合追加中间件
        /// </summary>
        /// <param name="view">当前操作的视图实例</param>
        /// <param name="dynamicMiddlewares">动态中间件可增集合</param>
        void PopulateMiddlewares(IView view, IDynamicMiddlewareCollection dynamicMiddlewares);
    }
}
