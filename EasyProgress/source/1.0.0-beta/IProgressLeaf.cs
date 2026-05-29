namespace EasyProgress
{
    /// <summary>
    /// 叶子节点非泛型接口
    /// </summary>
    public interface IProgressLeaf { }

    /// <summary>
    /// 叶子节点泛型接口
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>可主动报告进度。</para>
    /// </remarks>
    public interface IProgressLeaf<T> : IProgressNode<T>, IProgressLeaf
    {
        /// <summary>报告进度</summary>
        /// <param name="value">进度值</param>
        void Report(T value);

        /// <summary>标记完成</summary>
        void Complete();
    }
}
