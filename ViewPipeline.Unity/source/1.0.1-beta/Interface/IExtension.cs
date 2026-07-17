using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 扩展包接口
    /// </summary>
    public interface IExtension
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

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
        /// 初始化
        /// </summary>
        void Initialize();
    }
}
