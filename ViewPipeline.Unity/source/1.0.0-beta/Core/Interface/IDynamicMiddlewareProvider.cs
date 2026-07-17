using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 动态中间件流式供应器接口
    /// </summary>
    public interface IDynamicMiddlewareProvider
    {
        /// <summary>
        /// 根据当前操作的视图上下文，向运行时中间件可动态增删集合中追加属于本扩展包的动态切面组件
        /// </summary>
        /// <param name="view">当前操作的视图实例</param>
        /// <param name="staticMiddlewares">静态中间价只读集合</param>
        /// <param name="dynamicMiddlewares">动态中间件收纳集合</param>
        void PopulateMiddlewares(IView view, IReadOnlyList<IViewMiddleware> staticMiddlewares, IDynamicMiddlewareCollection dynamicMiddlewares);
    }
}
