using System;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 顺序规则
    /// </summary>
    /// <remarks>
    /// <para>适用于串行任务链：子节点按顺序执行，每个子节点完成后才进入下一个。</para>
    /// <para>权重表示每个阶段占总进度的比例。若总权重超过1，自动归一化。</para>
    /// <para>子节点进度 >= 1-1e-9 视为已完成。</para>
    /// <para>该规则无状态，可通过 <see cref="Create"/> 获取单例。</para>
    /// <para>示例：</para>
    /// <code>
    /// var rule = SequentialRule.Create();
    /// double result = rule.Compute(children, getWeight);
    /// </code>
    /// </remarks>
    public sealed class SequentialRule : ICompositionRule<double>
    {
        private class Handler { public static readonly SequentialRule instance = new SequentialRule(); }
        private SequentialRule() { }

        /// <summary>
        /// 静态工厂方法
        /// </summary>
        /// <returns>规则实例</returns>
        public static SequentialRule Create() { return Handler.instance; }

        /// <inheritdoc/>
        public double Compute(IReadOnlyList<IProgressNode<double>> children, Func<IProgressNode<double>, float> getWeight)
        {
            float totalWeight = 0;
            for (int i = 0; i < children.Count; i++)
            {
                float w = getWeight(children[i]);
                if (w > 0) totalWeight += w;
            }

            if (totalWeight <= 0) return 0;
            float scale = totalWeight > 1 ? 1f / totalWeight : 1f;

            double accumulated = 0;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                float w = getWeight(child);
                if (w <= 0) continue;
                float scaledWeight = w * scale;

                if (child.Progress >= 1.0 - 1e-9)
                {
                    accumulated += scaledWeight;
                }
                else
                {
                    return accumulated + child.Progress * scaledWeight;
                }
            }
            return Math.Min(1.0, accumulated);
        }
    }
}
