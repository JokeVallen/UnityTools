namespace EasyProgress.Core
{
    /// <summary>
    /// 组合节点管理器非泛型接口
    /// </summary>
    public interface ICompositeManager { }

    /// <summary>
    /// 组合节点管理器泛型接口
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>负责组合节点的获取与归还，通常配合对象池使用。</para>
    /// </remarks>
    public interface ICompositeManager<T> : ICompositeManager
    {
        /// <summary>获取组合节点</summary>
        /// <param name="rule">组合规则</param>
        IProgressComposite<T> AcquireComposite(ICompositionRule<T> rule);

        /// <summary>归还组合节点</summary>
        /// <param name="composite">组合节点</param>
        void ReleaseComposite(IProgressComposite<T> composite);
    }
}
