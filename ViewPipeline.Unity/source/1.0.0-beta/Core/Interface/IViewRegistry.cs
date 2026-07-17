using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图注册表接口
    /// </summary>
    public interface IViewRegistry : IReadOnlyCollection<IView>
    {
        /// <summary>
        /// 注册视图
        /// </summary>
        /// <param name="view">视图</param>
        void Register(IView view);

        /// <summary>
        /// 注销视图
        /// </summary>
        /// <param name="view">视图</param>
        void Unregister(IView view);
    }
}
