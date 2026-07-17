using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 扩展包接口
    /// </summary>
    public interface IExtension
    {
        /// <summary>
        /// 获取静态中间件
        /// </summary>
        /// <param name="direction">管线流转方向</param>
        IEnumerable<IViewMiddleware> GetMiddlewares(PipelineDirection direction);

        /// <summary>
        /// 获取动态中间件供应器
        /// </summary>
        /// <param name="direction">管线流转方向</param>
        IEnumerable<IDynamicMiddlewareProvider> GetDynamicProviders(PipelineDirection direction);

        /// <summary>
        /// 获取中间件验证器
        /// </summary>
        IEnumerable<IMiddlewareValidator> GetMiddlewareValidators();

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
    }
}
