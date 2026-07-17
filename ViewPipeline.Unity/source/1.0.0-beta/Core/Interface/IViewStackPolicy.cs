using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图层级组织与导航栈管理策略接口。
    /// </summary>
    public interface IViewStackPolicy : IReadOnlyCollection<IView>
    {
        /// <summary>
        /// 视图入栈
        /// </summary>
        /// <param name="view">视图</param>
        void Push(IView view);

        /// <summary>
        /// 视图出栈
        /// </summary>
        /// <param name="view">视图</param>
        void Pop(IView view);
    }
}
