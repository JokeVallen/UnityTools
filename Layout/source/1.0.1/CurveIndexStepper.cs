using System;
using UnityEngine;

namespace UGUI.Layout.Extension
{
    /// <summary>
    /// 基于 <see cref="WrapMode"/> 的曲线关键帧索引步进器
    /// </summary>
    /// <remarks>
    /// <para>
    /// 封装了不同 <see cref="WrapMode"/> 下的关键帧索引步进逻辑。
    /// <see cref="WrapMode.PingPong"/> 模式持有可变方向状态，因此每次布局计算必须通过
    /// <see cref="Create"/> 工厂方法获取新实例，不可跨布局复用同一实例。
    /// </para>
    /// <para>
    /// 同时提供无状态的 <see cref="Resolve"/> 静态方法，用于直接将布局元素序号映射到
    /// 关键帧索引（Direct 模式），无需逐步步进。
    /// </para>
    /// </remarks>
    public struct CurveIndexStepper
    {
        private int dir;
        private readonly int minIndex;
        private readonly int maxIndex;
        private readonly WrapMode mode;
        private readonly int range;

        private CurveIndexStepper(WrapMode mode, int minIndex, int maxIndex)
        {
            this.mode = mode;
            this.minIndex = minIndex;
            this.maxIndex = maxIndex;
            range = maxIndex - minIndex + 1;
            dir = 1;
        }

        /// <summary>
        /// 创建一个新的步进器实例
        /// </summary>
        /// <param name="mode">曲线行为模式</param>
        /// <param name="minIndex">最小关键帧索引（含）</param>
        /// <param name="maxIndex">最大关键帧索引（含）</param>
        /// <returns>新的步进器实例</returns>
        /// <exception cref="ArgumentException">minIndex &gt; maxIndex</exception>
        public static CurveIndexStepper Create(WrapMode mode, int minIndex, int maxIndex)
        {
            if (minIndex > maxIndex)
                throw new ArgumentException($"{nameof(minIndex)} 不能大于 {nameof(maxIndex)}");
            return new CurveIndexStepper(mode, minIndex, maxIndex);
        }

        /// <summary>
        /// 根据当前索引步进到下一个索引
        /// </summary>
        /// <param name="current">当前索引，必须在 [minIndex, maxIndex] 内</param>
        /// <returns>下一个索引</returns>
        /// <exception cref="ArgumentOutOfRangeException">current 不在合法范围内</exception>
        public int Next(int current)
        {
            if (current < minIndex || current > maxIndex)
                throw new ArgumentOutOfRangeException(nameof(current), current,
                    $"索引必须处于 [{minIndex}, {maxIndex}]");

            switch (mode)
            {
                case WrapMode.Loop:
                    {
                        int offset = (current - minIndex + 1) % range;
                        return minIndex + offset;
                    }
                case WrapMode.PingPong:
                    {
                        int next = current + dir;
                        if (next > maxIndex || next < minIndex)
                        {
                            dir = -dir;
                            next = current + dir;
                        }
                        return Mathf.Clamp(next, minIndex, maxIndex);
                    }
                default:
                    return Mathf.Clamp(current + 1, minIndex, maxIndex);
            }
        }

        /// <summary>
        /// 无状态地将布局元素序号直接映射到关键帧索引（Direct 模式专用）
        /// </summary>
        /// <param name="childIndex">布局元素序号（从 0 开始）</param>
        /// <param name="keyCount">关键帧总数</param>
        /// <param name="mode">曲线行为模式</param>
        /// <returns>对应的关键帧索引</returns>
        public static int Resolve(int childIndex, int keyCount, WrapMode mode)
        {
            if (keyCount <= 0) return 0;

            switch (mode)
            {
                case WrapMode.Loop:
                    return childIndex % keyCount;

                case WrapMode.PingPong:
                    if (keyCount == 1) return 0;
                    int period = 2 * (keyCount - 1);
                    int pos = childIndex % period;
                    return pos < keyCount ? pos : period - pos;

                default:
                    return Mathf.Clamp(childIndex, 0, keyCount - 1);
            }
        }
    }
}
