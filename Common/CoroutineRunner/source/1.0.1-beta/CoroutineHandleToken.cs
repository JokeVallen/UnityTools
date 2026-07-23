using System;

namespace CoroutineRunner
{
    /// <summary>
    /// 协程句柄 Token
    /// </summary>
    public readonly struct CoroutineHandleToken : IEquatable<CoroutineHandleToken>
    {
        internal readonly int Id;
        internal readonly long Version;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid => Id > 0;

        /// <summary>
        /// 默认缺省值单例
        /// </summary>
        public static readonly CoroutineHandleToken None = new CoroutineHandleToken();

        internal CoroutineHandleToken(int id, long version)
        {
            Id = id;
            Version = version;
        }

        /// <inheritdoc/>
        public bool Equals(CoroutineHandleToken other) => Id == other.Id && Version == other.Version;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is CoroutineHandleToken other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => (Id, Version).GetHashCode();

        /// <inheritdoc/>
        public static bool operator ==(CoroutineHandleToken left, CoroutineHandleToken right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(CoroutineHandleToken left, CoroutineHandleToken right) => !left.Equals(right);
    }
}
