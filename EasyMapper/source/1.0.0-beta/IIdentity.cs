namespace EasyMapper
{
    /// <summary>
    /// 令牌扩展接口
    /// </summary>
    /// <typeparam name="TToken">令牌类型</typeparam>
    /// <remarks>
    /// <para>通过实现该接口扩展不同类型的令牌。</para>
    /// </remarks>
    public interface IIdentity<TToken> : System.IEquatable<TToken> where TToken : struct
    {

    }
}