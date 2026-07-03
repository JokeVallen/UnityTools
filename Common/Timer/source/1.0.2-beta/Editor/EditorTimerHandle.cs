#if UNITY_EDITOR

using System;

namespace Timer
{
    /// <summary>
    /// 编辑器计时器句柄
    /// </summary>
    public readonly struct EditorTimerHandle : IEquatable<EditorTimerHandle>
    {
        /// <summary>
        /// 索引
        /// </summary>
        internal readonly int SlotIndex;

        /// <summary>
        /// 代际
        /// </summary>
        internal readonly int Generation;

        internal EditorTimerHandle(int slotIndex, int generation)
        {
            SlotIndex = slotIndex;
            Generation = generation;
        }

        /// <summary>
        /// 缺省值单例
        /// </summary>
        public static readonly EditorTimerHandle Null = new EditorTimerHandle(-1, 0);

        /// <summary>
        /// 是否为缺省值
        /// </summary>
        public bool IsNull => SlotIndex == -1 && Generation == 0;

        /// <inheritdoc/>
        public bool Equals(EditorTimerHandle other) => SlotIndex == other.SlotIndex && Generation == other.Generation;
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is EditorTimerHandle other && Equals(other);
        /// <inheritdoc/>
        public override int GetHashCode() => (SlotIndex, Generation).GetHashCode();
        /// <inheritdoc/>
        public static bool operator ==(EditorTimerHandle left, EditorTimerHandle right) => left.Equals(right);
        /// <inheritdoc/>
        public static bool operator !=(EditorTimerHandle left, EditorTimerHandle right) => !left.Equals(right);
    }
}

#endif