using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 组合节点非泛型接口
    /// </summary>
    public interface IProgressComposite { }

    /// <summary>
    /// 组合节点泛型接口
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>聚合多个子节点，通过组合规则计算自身进度。</para>
    /// </remarks>
    public interface IProgressComposite<T> : IProgressNode<T>, IProgressComposite
    {
        /// <summary>当前使用的组合规则</summary>
        ICompositionRule<T> Rule { get; }

        /// <summary>子节点只读列表</summary>
        IReadOnlyCollection<IProgressNode<T>> Children { get; }

        /// <summary>添加子节点</summary>
        /// <param name="node">子节点</param>
        void AddChild(IProgressNode<T> node);

        /// <summary>移除子节点</summary>
        /// <param name="node">子节点</param>
        bool RemoveChild(IProgressNode<T> node);

        /// <summary>设置组合规则</summary>
        /// <param name="rule">规则</param>
        void SetRule(ICompositionRule<T> rule);
    }
}
