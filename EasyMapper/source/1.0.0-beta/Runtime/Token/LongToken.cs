namespace EasyMapper.Runtime
{
    /// <summary> 64 位长整型令牌 </summary>
    /// <remarks>
    /// <para> 默认令牌实现，内部存储一个 <see cref="long"/> 值，支持与 <c>long</c> 的双向隐式转换。 </para>
    /// <para> 常用于短字符串编码、自增 ID 等场景。高位（bit63）可由 <see cref="SmartDistributor"/> 用作路径标记。 </para>
    /// </remarks>
    public readonly struct LongToken : IIdentity<LongToken>
    {
        /// <summary> 令牌值 </summary>
        public long Value => value;
        private readonly long value;
        public LongToken(long value) => this.value = value;

        public bool Equals(LongToken other) => value == other.value;
        public override bool Equals(object obj) => obj is LongToken other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();

        public static implicit operator long(LongToken token) => token.value;
        public static implicit operator LongToken(long value) => new LongToken(value);
    }
}