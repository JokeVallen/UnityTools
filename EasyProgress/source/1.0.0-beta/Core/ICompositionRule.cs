using System;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 组合规则非泛型接口
    /// </summary>
    public interface ICompositeRule { }

    /// <summary>
    /// 组合规则泛型接口
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>定义如何从子节点列表计算父节点进度。</para>
    /// <para><paramref name="getWeight"/> 委托用于获取每个子节点的权重，权重的含义由规则本身定义。</para>
    /// </remarks>
    public interface ICompositionRule<T> : ICompositeRule
    {
        /// <summary>计算父节点进度</summary>
        /// <param name="children">子节点列表</param>
        /// <param name="getWeight">获取子节点权重的委托</param>
        T Compute(IReadOnlyList<IProgressNode<T>> children, Func<IProgressNode<T>, float> getWeight);
    }
}
