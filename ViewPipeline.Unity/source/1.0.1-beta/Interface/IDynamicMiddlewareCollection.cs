using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 动态中间件可增集合接口
    /// </summary>
    public interface IDynamicMiddlewareCollection : IEnumerable<IViewMiddleware>
    {
        /// <summary>
        /// 添加中间件
        /// </summary>
        /// <param name="middleware">中间件</param>
        void Add(IViewMiddleware middleware);
    }
}
