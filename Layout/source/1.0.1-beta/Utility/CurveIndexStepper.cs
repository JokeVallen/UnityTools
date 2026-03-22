using System;
using UnityEngine;

namespace UGUI.Layout.Extension
{
    /// <summary>
    /// 基于 <see cref="WrapMode"/> 的曲线关键帧索引步进器
    /// </summary>
    /// <remarks>
    /// <para>
    /// 封装了不同 <see cref="WrapMode"/> 下的关键帧索引步进逻辑，支持任意整数步长。
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
        private readonly int step;

        private CurveIndexStepper(WrapMode mode, int minIndex, int maxIndex, int step)
        {
            this.mode = mode;
            this.minIndex = minIndex;
            this.maxIndex = maxIndex;
            range = maxIndex - minIndex + 1;
            this.step = step;
            dir = step >= 0 ? 1 : -1;
        }

        /// <summary>
        /// 创建一个新的步进器实例
        /// </summary>
        /// <param name="mode">曲线行为模式</param>
        /// <param name="minIndex">最小关键帧索引（含）</param>
        /// <param name="maxIndex">最大关键帧索引（含）</param>
        /// <param name="step">每次步进的步长，不可为 0，支持负数（反向步进）</param>
        /// <returns>新的步进器实例</returns>
        /// <exception cref="ArgumentException">minIndex &gt; maxIndex 或 step 为 0</exception>
        public static CurveIndexStepper Create(WrapMode mode, int minIndex, int maxIndex, int step = 1)
        {
            if (minIndex > maxIndex)
                throw new ArgumentException($"{nameof(minIndex)} 不能大于 {nameof(maxIndex)}");
            if (step == 0)
                throw new ArgumentException($"{nameof(step)} 不能为 0");
            return new CurveIndexStepper(mode, minIndex, maxIndex, step);
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

            int absStep = Math.Abs(step);

            switch (mode)
            {
                case WrapMode.Loop:
                    {
                        // 在 [0, range) 空间内做模运算，再映射回 [minIndex, maxIndex]
                        int offset = (current - minIndex + dir * absStep % range + range) % range;
                        return minIndex + offset;
                    }
                case WrapMode.PingPong:
                    {
                        if (range == 1) return minIndex;

                        // period = 一个完整 PingPong 周期的步数（去程 + 回程，不重复端点）
                        int period = 2 * (range - 1);

                        // 将当前位置折算为相对 minIndex 的绝对偏移，加上本次步进量
                        int absPos = (current - minIndex) + dir * absStep;

                        // 用模运算将绝对偏移折叠到 [0, period)，支持正负偏移
                        int wrapped = ((absPos % period) + period) % period;

                        // [0, range-1]：正向段；[range, period-1]：反向段
                        int result = wrapped < range
                            ? minIndex + wrapped
                            : minIndex + period - wrapped;

                        // 更新方向状态：触达端点时翻转，供下一次 Next 使用
                        if (result == maxIndex) dir = -1;
                        else if (result == minIndex) dir = 1;

                        return result;
                    }
                default:
                    return Mathf.Clamp(current + dir * absStep, minIndex, maxIndex);
            }
        }

        /// <summary>
        /// 无状态地将布局元素序号直接映射到关键帧索引（Direct 模式专用）
        /// </summary>
        /// <param name="childIndex">布局元素序号（从 0 开始）</param>
        /// <param name="keyCount">关键帧总数</param>
        /// <param name="mode">曲线行为模式</param>
        /// <param name="step">步长，不可为 0，默认为 1</param>
        /// <returns>对应的关键帧索引</returns>
        public static int Resolve(int childIndex, int keyCount, WrapMode mode, int step = 1)
        {
            if (keyCount <= 0) return 0;
            if (step == 0) return 0;

            int absStep = Math.Abs(step);
            // 实际偏移量（支持负步长：负步长等价于从末尾倒序映射）
            int effectiveIndex = step > 0
                ? childIndex * absStep
                : (keyCount - 1) - childIndex * absStep;

            switch (mode)
            {
                case WrapMode.Loop:
                    return ((effectiveIndex % keyCount) + keyCount) % keyCount;

                case WrapMode.PingPong:
                    if (keyCount == 1) return 0;
                    int period = 2 * (keyCount - 1);
                    int pos = ((effectiveIndex % period) + period) % period;
                    return pos < keyCount ? pos : period - pos;

                default:
                    return Mathf.Clamp(effectiveIndex, 0, keyCount - 1);
            }
        }
    }
}