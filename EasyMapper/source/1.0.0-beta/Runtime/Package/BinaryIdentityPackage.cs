namespace EasyMapper.Runtime
{
    /// <summary> LongToken 二进制序列化器 </summary>
    /// <remarks>
    /// <para> 将 <see cref="LongToken"/> 按小端序转换为 8 字节数组，或反向还原。 </para>
    /// <para> 若输入字节数组长度不足 8，返回值为 0 的令牌。 </para>
    /// </remarks>
    public sealed class BinaryIdentityPackage : IPackage<LongToken>
    {
        public byte[] Wrap(LongToken token) => System.BitConverter.GetBytes(token.Value);

        public LongToken Unwrap(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 8) return 0;
            return System.BitConverter.ToInt64(bytes, 0);
        }
    }
}