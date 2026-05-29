namespace EasyProgress.Core
{
    /// <summary>
    /// 进度管理器非泛型接口
    /// </summary>
    public interface IProgressManager { }

    /// <summary>
    /// 进度管理器泛型接口
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>整合叶子节点管理和组合节点管理。</para>
    /// </remarks>
    public interface IProgressManager<T> : IProgressManager, ILeafManager<T>, ICompositeManager<T> { }
}
