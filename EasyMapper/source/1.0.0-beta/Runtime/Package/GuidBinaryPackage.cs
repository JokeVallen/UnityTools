namespace EasyMapper.Runtime
{
    /// <summary> GuidToken 二进制序列化器 </summary>
    /// <remarks>
    /// <para> 将 <see cref="GuidToken"/> 转换为 16 字节数组，或反向还原。 </para>
    /// <para> 无效输入返回 <see cref="System.Guid.Empty"/>。 </para>
    /// </remarks>
    public sealed class GuidBinaryPackage : IPackage<GuidToken>
    {
        public byte[] Wrap(GuidToken token) => token.Value.ToByteArray();

        public GuidToken Unwrap(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 16)
                return new GuidToken(System.Guid.Empty);
            return new GuidToken(new System.Guid(bytes));
        }
    }
}