#if ENABLE_UNSAFE

namespace FNV1A.x64
{
    public static partial class FNV1AUtility
    {
        /// <summary>
        /// Unsafe 类型哈希缓存
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <remarks>
        /// <para>仅对部分类型提供特化加速（如 <see cref="System.Guid"/>），其他类型将回退到 <see cref="object.GetHashCode"/> 或默认逻辑。</para>
        /// <para>若需为自定义类型提供 unsafe 优化，请使用 <see cref="RegisterHasherForUnsafe{T}"/> 注册。</para>
        /// </remarks>
        public static class UnsafeFNVHasherCache<T>
        {
            public static System.Func<ulong, T, ulong> Hasher
            {
                get => hasher ?? defaultHasher;
                set => hasher = value;
            }
            private static System.Func<ulong, T, ulong> hasher;
            private static readonly System.Func<ulong, T, ulong> defaultHasher;

            static UnsafeFNVHasherCache()
            {
                System.Type type = typeof(T);

                if (typeof(IFNVHashable).IsAssignableFrom(type))
                    defaultHasher = (h, v) => v == null ? AppendByte(h, 0) : ((IFNVHashable)v).AppendHash(h);
                else if (type == typeof(System.Guid))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, System.Guid, ulong>(AppendGuidFastUnsafe);
                else
                    defaultHasher = (h, v) => v == null ? AppendByte(h, 0) : AppendInt32(h, v.GetHashCode());
            }
        }

        /// <summary>
        /// 注册非安全版本哈希器
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="hasher">自定义哈希委托</param>
        /// <remarks>
        /// <para>允许为指定类型 <typeparamref name="T"/> 注册自定义的哈希计算逻辑，覆盖默认行为。</para>
        /// <para>传入 <c>null</c> 可恢复默认哈希器。</para>
        /// </remarks>
        public static void RegisterHasherForUnsafe<T>(System.Func<ulong, T, ulong> hasher)
        {
            UnsafeFNVHasherCache<T>.Hasher = hasher;
        }

        /// <summary>
        /// 追加 Guid（Unsafe 逐字节版本）
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标 Guid</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>使用 unsafe 指针直接读取 Guid 内存，逐字节追加（16 次迭代），零分配且性能较高（约 20 ns）。</para>
        /// <para>与安全版本 <see cref="AppendGuid"/> 的哈希结果一致，但性能更优且无 GC 分配。</para>
        /// <para><b>注意</b>：此方法仅在项目启用 unsafe 编译选项且定义了 <c>ENABLE_UNSAFE</c> 宏时可用。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static unsafe ulong AppendGuidUnsafe(ulong hash, System.Guid value)
        {
            byte* pGuid = (byte*)&value;

            for (int i = 0; i < 16; i++)
            {
                hash = (hash ^ pGuid[i]) * PRIME;
            }

            return hash;
        }

        /// <summary>
        /// 追加 Guid（Unsafe 极速版本）
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标 Guid</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>将 Guid 视为两个 <see cref="ulong"/>，仅需两次异或与乘法迭代，性能极高（约 2.5 ns），零分配。</para>
        /// <para>这是库中处理 Guid 的最快方法，推荐在性能敏感场景通过 <see cref="AppendForUnsafe{T}"/> 调用。</para>
        /// <para><b>注意</b>：此方法仅在项目启用 unsafe 编译选项且定义了 <c>ENABLE_UNSAFE</c> 宏时可用。</para>
        /// <para><b>注意</b>：此方法产生的哈希值与 <see cref="AppendGuidUnsafe"/> 和 <see cref="AppendGuid"/> 不同。
        /// 若需与已有安全版本哈希兼容，请使用 <see cref="AppendGuidUnsafe"/> 或 <see cref="AppendGuid"/>。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static unsafe ulong AppendGuidFastUnsafe(ulong hash, System.Guid value)
        {
            ulong* pData = (ulong*)&value;

            hash ^= pData[0];
            hash *= PRIME;

            hash ^= pData[1];
            hash *= PRIME;

            return hash;
        }

        /// <summary>
        /// 泛型追加类型（Unsafe 高性能路径）
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>通过 <see cref="UnsafeFNVHasherCache{T}"/> 获取对应的 unsafe 优化委托并执行。</para>
        /// <para>
        /// <b>适用场景</b>：<br/>
        /// - 需要显式使用 unsafe 加速处理特定类型（如 <see cref="System.Guid"/>）。<br/>
        /// - 开发者已通过 <see cref="RegisterHasherForUnsafe{T}"/> 注册了自定义的 unsafe 高性能哈希器。
        /// </para>
        /// <para>
        /// <b>注意事项</b>：<br/>
        /// - 此方法仅在项目启用了 unsafe 编译选项且定义了 <c>ENABLE_UNSAFE</c> 宏时可用。<br/>
        /// - 对于未在 <see cref="UnsafeFNVHasherCache{T}"/> 中特化处理的类型，将回退到调用 <see cref="object.GetHashCode"/> 并追加其结果，
        ///   这与安全版本的 <see cref="AppendForNET{T}"/> 行为可能不同。如需安全的通用处理，请使用 <see cref="AppendForNET{T}"/>。
        /// </para>
        /// <para>
        /// <b>性能参考</b>：<br/>
        /// - 处理 <see cref="System.Guid"/> 时，内部调用 <see cref="AppendGuidFastUnsafe"/>，单次约 2.5 ns，零分配。
        /// </para>
        /// <seealso cref="AppendForNET{T}"/>
        /// <seealso cref="AppendForUnsafe(ulong, object)"/>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendForUnsafe<T>(ulong hash, T value)
        {
            return UnsafeFNVHasherCache<T>.Hasher(hash, value);
        }

        /// <summary>
        /// 非泛型追加类型（Unsafe 高性能路径）
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标对象</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>通过 <c>switch</c> 对已知可加速的类型进行分发，目前特殊处理 <see cref="System.Guid"/> 和 <see cref="IFNVHashable"/>。</para>
        /// <para>
        /// <b>支持的类型</b>：<br/>
        /// - <see cref="IFNVHashable"/>：调用其 <see cref="IFNVHashable.AppendHash"/> 方法。<br/>
        /// - <see cref="System.Guid"/>：调用 <see cref="AppendGuidFastUnsafe"/>，获得极速零分配处理。<br/>
        /// - 其他类型：回退到 <see cref="AppendInt32"/> 追加 <see cref="object.GetHashCode"/> 的结果（注意：不具备跨平台确定性，且值类型会装箱）。
        /// </para>
        /// <para>
        /// <b>注意事项</b>：<br/>
        /// - 此方法仅在项目启用了 unsafe 编译选项且定义了 <c>ENABLE_UNSAFE</c> 宏时可用。<br/>
        /// - 若需要处理更广泛的类型，请优先使用泛型版本 <see cref="AppendForUnsafe{T}"/> 或安全版本的 <see cref="AppendForNET(ulong, object)"/>。
        /// </para>
        /// <seealso cref="AppendForNET(ulong, object)"/>
        /// <seealso cref="AppendForUnsafe{T}"/>
        /// </remarks>
        public static ulong AppendForUnsafe(ulong hash, object value)
        {
            if (value == null) return AppendByte(hash, 0);

            switch (value)
            {
                case IFNVHashable hashable: return hashable.AppendHash(hash);
                case System.Guid guid: return AppendGuidFastUnsafe(hash, guid);
                default: return AppendInt32(hash, value.GetHashCode());
            }
        }
    }
}

#endif