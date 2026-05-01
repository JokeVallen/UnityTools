namespace FNV1A.x64
{
    /// <summary>
    /// FNV-1a 64 位哈希工具
    /// </summary>
    /// <remarks>
    /// <para>提供基于 FNV-1a 算法的 64 位哈希计算功能，支持 .NET 基础类型、Unity 常用类型以及集合类型的哈希合并。</para>
    /// <para>通过静态泛型缓存实现零反射开销的运行时类型解析，并允许外部注册自定义哈希器。</para>
    /// <para>使用前请通过 <see cref="Start"/> 获取初始哈希值，再调用相应的 Append 方法逐步累积。</para>
    /// </remarks>
    public static partial class FNV1AUtility
    {
        /// <summary>
        /// 初始偏移量
        /// </summary>
        /// <remarks>
        /// <para>FNV-1a 64 位算法的标准偏移常量：<c>0xcbf29ce484222325</c>。</para>
        /// </remarks>
        public const ulong OFFSET = 0xcbf29ce484222325;

        /// <summary>
        /// FNV 素数
        /// </summary>
        /// <remarks>
        /// <para>FNV-1a 64 位算法的素数常量：<c>0x100000001b3</c>。</para>
        /// </remarks>
        public const ulong PRIME = 0x100000001b3;

        /// <summary>
        /// 获取初始哈希值
        /// </summary>
        /// <returns>初始哈希值</returns>
        /// <remarks>
        /// <para>返回 FNV-1a 64 位算法的标准偏移初始值 <see cref="OFFSET"/>。</para>
        /// <para>所有哈希计算都应从此方法返回的值开始累积。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong Start() => OFFSET;
    }
}