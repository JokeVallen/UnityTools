namespace EasyProgress.Core
{
    /// <summary>
    /// 加权组合节点泛型接口
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>扩展自 <see cref="IProgressComposite{T}"/>，为每个子节点提供权重管理。</para>
    /// </remarks>
    public interface IWeightedProgressComposite<T> : IProgressComposite<T>
    {
        /// <summary>添加带权重的子节点</summary>
        /// <param name="node">子节点</param>
        /// <param name="weight">权重</param>
        void AddChild(IProgressNode<T> node, float weight);

        /// <summary>设置子节点的权重</summary>
        /// <param name="node">子节点</param>
        /// <param name="weight">权重</param>
        void SetWeight(IProgressNode<T> node, float weight);

        /// <summary>获取子节点的权重</summary>
        /// <param name="node">子节点</param>
        float GetWeight(IProgressNode<T> node);
    }
}
