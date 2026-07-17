using System;

namespace EasyMapper.Runtime
{
    /// <summary> 128 位 GUID 令牌 </summary>
    /// <remarks>
    /// <para> 封装 <see cref="System.Guid"/> 作为全局唯一标识，支持隐式转换。 </para>
    /// </remarks>
    public readonly struct GuidToken : IIdentity<GuidToken>, IEquatable<GuidToken>
    {
        /// <summary>令牌值</summary>
        public Guid Value => value;
        private readonly Guid value;
        public GuidToken(Guid value) => this.value = value;

        public bool Equals(GuidToken other) => value.Equals(other.value);
        public override bool Equals(object obj) => obj is GuidToken other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();

        public static implicit operator Guid(GuidToken token) => token.value;
        public static implicit operator GuidToken(Guid value) => new GuidToken(value);
    }
}