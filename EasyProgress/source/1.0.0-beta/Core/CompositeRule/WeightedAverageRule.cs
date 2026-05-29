using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 加权平均规则
    /// </summary>
    /// <remarks>
    /// <para>计算所有子节点进度的加权平均值，权重为 <see cref="getWeight"/> 委托返回的值。</para>
    /// <para>若所有子节点权重 ≤ 0，则返回 0。</para>
    /// <para>该规则无状态，可通过 <see cref="Create"/> 获取单例。</para>
    /// <para>示例：</para>
    /// <code>
    /// var rule = WeightedAverageRule.Create();
    /// double result = rule.Compute(children, getWeight);
    /// </code>
    /// </remarks>
    public sealed class WeightedAverageRule : ICompositionRule<double>
    {
        private class Handler { public static readonly WeightedAverageRule instance = new WeightedAverageRule(); }
        private WeightedAverageRule() { }

        /// <summary>
        /// 静态工厂方法
        /// </summary>
        /// <returns>规则实例</returns>
        public static WeightedAverageRule Create() { return Handler.instance; }

        /// <inheritdoc/>
        public double Compute(IReadOnlyList<IProgressNode<double>> children, System.Func<IProgressNode<double>, float> getWeight)
        {
            double totalWeight = 0;
            double weightedSum = 0;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                float w = getWeight(child);
                if (w <= 0) continue;
                totalWeight += w;
                weightedSum += child.Progress * w;
            }
            return totalWeight > 0 ? weightedSum / totalWeight : 0;
        }
    }
}
