using System;

namespace Timer
{
    /// <summary>
    /// 计时器句柄
    /// </summary>
    public readonly struct TimerHandle : IEquatable<TimerHandle>
    {
        /// <summary>
        /// 索引
        /// </summary>
        internal readonly int SlotIndex;

        /// <summary>
        /// 代际
        /// </summary>
        internal readonly int Generation;

        internal TimerHandle(int slotIndex, int generation)
        {
            SlotIndex = slotIndex;
            Generation = generation;
        }

        /// <summary>
        /// 缺省值单例
        /// </summary>
        public static readonly TimerHandle Null = new TimerHandle(-1, 0);

        /// <summary>
        /// 是否为缺省值
        /// </summary>
        public bool IsNull => SlotIndex == -1 && Generation == 0;

        /// <inheritdoc/>
        public bool Equals(TimerHandle other) => SlotIndex == other.SlotIndex && Generation == other.Generation;
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TimerHandle other && Equals(other);
        /// <inheritdoc/>
        public override int GetHashCode() => (SlotIndex, Generation).GetHashCode();
        /// <inheritdoc/>
        public static bool operator ==(TimerHandle left, TimerHandle right) => left.Equals(right);
        /// <inheritdoc/>
        public static bool operator !=(TimerHandle left, TimerHandle right) => !left.Equals(right);
    }
}