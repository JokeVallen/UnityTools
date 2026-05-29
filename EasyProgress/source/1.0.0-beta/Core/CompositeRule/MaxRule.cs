using System;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 最大值规则
    /// </summary>
    /// <remarks>
    /// <para>取所有子节点进度的最大值。</para>
    /// <para>忽略权重参数。</para>
    /// <para>该规则无状态，可通过 <see cref="Create"/> 获取单例。</para>
    /// <para>示例：</para>
    /// <code>
    /// var rule = MaxRule.Create();
    /// double result = rule.Compute(children, getWeight);
    /// </code>
    /// </remarks>
    public sealed class MaxRule : ICompositionRule<double>
    {
        private class Handler { public static readonly MaxRule instance = new MaxRule(); }
        private MaxRule() { }

        /// <summary>
        /// 静态工厂方法
        /// </summary>
        /// <returns>规则实例</returns>
        public static MaxRule Create() { return Handler.instance; }

        /// <inheritdoc/>
        public double Compute(IReadOnlyList<IProgressNode<double>> children, Func<IProgressNode<double>, float> getWeight)
        {
            if (children.Count == 0) return 0;
            double max = children[0].Progress;
            for (int i = 1; i < children.Count; i++)
                if (children[i].Progress > max) max = children[i].Progress;
            return max;
        }
    }
}
