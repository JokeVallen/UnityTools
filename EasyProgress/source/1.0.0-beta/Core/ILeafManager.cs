namespace EasyProgress.Core
{
    /// <summary>
    /// 叶子节点管理器非泛型接口
    /// </summary>
    public interface ILeafManager { }

    /// <summary>
    /// 叶子节点管理器泛型接口
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>负责叶子节点的获取与归还，通常配合对象池使用。</para>
    /// </remarks>
    public interface ILeafManager<T> : ILeafManager
    {
        /// <summary>获取叶子节点</summary>
        IProgressLeaf<T> AcquireLeaf();

        /// <summary>归还叶子节点</summary>
        /// <param name="leaf">叶子节点</param>
        void ReleaseLeaf(IProgressLeaf<T> leaf);
    }
}
