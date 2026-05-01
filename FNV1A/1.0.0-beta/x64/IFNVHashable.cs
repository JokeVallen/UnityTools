namespace FNV1A.x64
{
    /// <summary>
    /// 自定义哈希扩展接口
    /// </summary>
    /// <remarks>
    /// <para>实现此接口的类型可自定义如何将自身内部状态合并入 FNV-1a 64 位累积哈希中。</para>
    /// <para>库中的泛型缓存机制会优先检查类型是否实现该接口，若实现则直接调用 <see cref="AppendHash"/> 方法。</para>
    /// <para>示例：</para>
    /// <code>
    /// public class MyData : IFNVHashable
    /// {
    ///     public int Id;
    ///     public string Name;
    /// 
    ///     public ulong AppendHash(ulong hash)
    ///     {
    ///         hash = FNV1AUtility.AppendInt32(hash, Id);
    ///         hash = FNV1AUtility.AppendString(hash, Name);
    ///         return hash;
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public interface IFNVHashable
    {
        /// <summary>
        /// 追加哈希
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <returns>合并后的新哈希值</returns>
        /// <remarks>
        /// <para>实现类应在方法内按顺序将自身的字段追加到 <paramref name="hash"/> 中，并返回最终结果。</para>
        /// <para>建议使用 <see cref="FNV1AUtility"/> 提供的 Append 系列方法进行组合。</para>
        /// </remarks>
        ulong AppendHash(ulong hash);
    }
}