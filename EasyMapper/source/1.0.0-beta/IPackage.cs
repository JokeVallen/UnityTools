namespace EasyMapper
{
    /// <summary>
    /// 映射架构的令牌包装和解包接口
    /// </summary>
    /// <typeparam name="TToken">令牌类型</typeparam>
    /// <remarks>
    /// <para>负责令牌的序列化和反序列化，使得令牌可以作为二进制数据进行传输。</para>
    /// </remarks>
    public interface IPackage<TToken>
    {
        /// <summary>
        /// 将令牌包装为二进制数据
        /// </summary>
        /// <param name="token">令牌</param>
        /// <returns>二进制数据</returns>
        byte[] Wrap(TToken token);

        /// <summary>
        /// 从二进制数据解包出令牌
        /// </summary>
        /// <param name="bytes">二进制数据</param>
        /// <returns>令牌</returns>
        TToken Unwrap(byte[] bytes);
    }
}