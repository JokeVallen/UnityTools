namespace FNV1A.x64
{
    public static partial class FNV1AUtility
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct FloatToUint
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public float floatValue;
            [System.Runtime.InteropServices.FieldOffset(0)] public int intValue;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct DoubleLongUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public double doubleValue;
            [System.Runtime.InteropServices.FieldOffset(0)] public long longValue;
        }

        /// <summary>
        /// .NET 类型哈希缓存
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <remarks>
        /// <para>为任意类型 <typeparamref name="T"/> 提供默认的哈希追加委托，并允许外部覆盖。</para>
        /// <para>静态构造函数中会根据类型特征生成高效委托，包括对基础类型、枚举、DateTime、Guid 等的特化处理。</para>
        ///  <para>对于 <see cref="System.Guid"/> 类型，若定义了 <c>ENABLE_UNSAFE</c>，默认使用高性能的 <see cref="AppendGuidFastUnsafe"/>，否则回退至 <see cref="AppendGuid"/>。开发者可通过 <see cref="RegisterHasherForNET{T}"/> 覆盖此行为。</para>
        /// </remarks>
        public static class NETFNVHasherCache<T>
        {
            /// <summary>
            /// 哈希追加委托
            /// </summary>
            /// <remarks>
            /// <para>获取或设置用于计算 <typeparamref name="T"/> 类型哈希的委托。若未显式设置，则返回默认生成的委托。</para>
            /// </remarks>
            public static System.Func<ulong, T, ulong> Hasher
            {
                get => hasher ?? defaultHasher;
                set => hasher = value;
            }
            private static System.Func<ulong, T, ulong> hasher;
            private static readonly System.Func<ulong, T, ulong> defaultHasher;

            static NETFNVHasherCache()
            {
                System.Type type = typeof(T);

                if (typeof(IFNVHashable).IsAssignableFrom(type))
                    defaultHasher = (h, v) => v == null ? AppendByte(h, 0) : ((IFNVHashable)v).AppendHash(h);
                else if (type == typeof(byte))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, byte, ulong>(AppendByte);
                else if (type == typeof(int))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, int, ulong>(AppendInt32);
                else if (type == typeof(long))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, long, ulong>(AppendInt64);
                else if (type == typeof(float))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, float, ulong>(AppendFloat);
                else if (type == typeof(double))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, double, ulong>(AppendDouble);
                else if (type == typeof(string))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, string, ulong>(AppendString);
                else if (type == typeof(bool))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, bool, ulong>(AppendBool);
                else if (type.IsEnum)
                    defaultHasher = (h, v) => AppendInt32(h, System.Convert.ToInt32(v));
                else if (type == typeof(System.DateTime))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, System.DateTime, ulong>(AppendDateTime);
                else if (type == typeof(System.Guid))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, System.Guid, ulong>(AppendGuid);
                else
                    defaultHasher = (h, v) => v == null ? AppendByte(h, 0) : AppendInt32(h, v.GetHashCode());
            }
        }

        /// <summary>
        /// 注册 .NET 类型哈希器
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="hasher">自定义哈希委托</param>
        /// <remarks>
        /// <para>允许为指定类型 <typeparamref name="T"/> 注册自定义的哈希计算逻辑，覆盖默认行为。</para>
        /// <para>传入 <c>null</c> 可恢复默认哈希器。</para>
        /// </remarks>
        public static void RegisterHasherForNET<T>(System.Func<ulong, T, ulong> hasher)
        {
            NETFNVHasherCache<T>.Hasher = hasher;
        }

        /// <summary>
        /// 追加字节
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标字节</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>FNV-1a 算法原子操作：将当前哈希与字节进行异或，然后乘以素数 <see cref="PRIME"/>。</para>
        /// <para>此方法为内联优化，适合作为高频调用的基础操作。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendByte(ulong hash, byte value)
        {
            unchecked
            {
                return (hash ^ value) * PRIME;
            }
        }

        /// <summary>
        /// 追加 32 位整数
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标数值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>将 32 位整数按小端序拆分为 4 个字节，依次调用 <see cref="AppendByte"/> 进行哈希更新。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendInt32(ulong hash, int value)
        {
            hash = AppendByte(hash, (byte)(value & 0xFF));
            hash = AppendByte(hash, (byte)((value >> 8) & 0xFF));
            hash = AppendByte(hash, (byte)((value >> 16) & 0xFF));
            hash = AppendByte(hash, (byte)((value >> 24) & 0xFF));
            return hash;
        }

        /// <summary>
        /// 追加 64 位整数
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标数值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>将 64 位整数按小端序拆分为 8 个字节，依次调用 <see cref="AppendByte"/> 进行哈希更新。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendInt64(ulong hash, long value)
        {
            ulong u = (ulong)value;
            hash = AppendByte(hash, (byte)(u & 0xFF));
            hash = AppendByte(hash, (byte)((u >> 8) & 0xFF));
            hash = AppendByte(hash, (byte)((u >> 16) & 0xFF));
            hash = AppendByte(hash, (byte)((u >> 24) & 0xFF));
            hash = AppendByte(hash, (byte)((u >> 32) & 0xFF));
            hash = AppendByte(hash, (byte)((u >> 40) & 0xFF));
            hash = AppendByte(hash, (byte)((u >> 48) & 0xFF));
            hash = AppendByte(hash, (byte)((u >> 56) & 0xFF));
            return hash;
        }

        /// <summary>
        /// 追加单精度浮点数
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标数值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>将 <see cref="float"/> 的二进制位重新解释为 <see cref="int"/>，然后调用 <see cref="AppendInt32"/> 追加。</para>
        /// <para>注意：由于使用原始二进制位，<c>0.0f</c> 与 <c>-0.0f</c> 会产生不同哈希。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendFloat(ulong hash, float value)
        {
            var union = new FloatToUint { floatValue = value };
            return AppendInt32(hash, union.intValue);
        }

        /// <summary>
        /// 追加双精度浮点数
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标数值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>将 <see cref="double"/> 的二进制位重新解释为 <see cref="long"/>，然后调用 <see cref="AppendInt64"/> 追加。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendDouble(ulong hash, double value)
        {
            var union = new DoubleLongUnion { doubleValue = value };
            return AppendInt64(hash, union.longValue);
        }

        /// <summary>
        /// 追加字符串
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标字符串</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>若字符串为 <c>null</c>，等价于追加一个值为 0 的字节。</para>
        /// <para>否则遍历字符串中的每个 <see cref="char"/>，按小端序追加其两个字节。</para>
        /// </remarks>
        public static ulong AppendString(ulong hash, string value)
        {
            if (value == null) return AppendByte(hash, 0);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                hash = AppendByte(hash, (byte)(c & 0xFF));
                hash = AppendByte(hash, (byte)((c >> 8) & 0xFF));
            }

            return hash;
        }

        /// <summary>
        /// 追加布尔值
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标布尔值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para><c>true</c> 转换为字节 1，<c>false</c> 转换为字节 0，然后调用 <see cref="AppendByte"/>。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendBool(ulong hash, bool value) => AppendByte(hash, (byte)(value ? 1 : 0));

        /// <summary>
        /// 追加枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标枚举值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>将枚举值转换为 32 位整数，然后调用 <see cref="AppendInt32"/> 追加。</para>
        /// </remarks>
        public static ulong AppendEnum<T>(ulong hash, T value) where T : System.Enum => AppendInt32(hash, System.Convert.ToInt32(value));

        /// <summary>
        /// 追加日期时间
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标日期时间</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>使用 <see cref="DateTime.Ticks"/> 属性作为 64 位整数追加。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendDateTime(ulong hash, System.DateTime value) => AppendInt64(hash, value.Ticks);

        /// <summary>
        /// 泛型追加 .NET 类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>通过 <see cref="NETFNVHasherCache{T}"/> 获取对应的哈希委托并执行。</para>
        /// <para>若从未调用过该类型，静态构造函数会自动生成默认委托，后续调用零开销。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendForNET<T>(ulong hash, T value)
        {
            return NETFNVHasherCache<T>.Hasher(hash, value);
        }

        /// <summary>
        /// 非泛型追加 .NET 类型
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标对象</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>通过 <c>switch</c> 对常见类型进行分发，避免值类型装箱（在具体分支中转换为强类型调用）。</para>
        /// <para>若对象类型不在已知列表中，则回退到调用 <see cref="object.GetHashCode"/> 并追加其结果。</para>
        /// </remarks>
        public static ulong AppendForNET(ulong hash, object value)
        {
            if (value == null) return AppendByte(hash, 0);

            switch (value)
            {
                case IFNVHashable hashable: return hashable.AppendHash(hash);
                case byte by: return AppendByte(hash, by);
                case int i: return AppendInt32(hash, i);
                case long l: return AppendInt64(hash, l);
                case float f: return AppendFloat(hash, f);
                case double dou: return AppendDouble(hash, dou);
                case string s: return AppendString(hash, s);
                case bool b: return AppendBool(hash, b);
                case System.Enum e: return AppendInt32(hash, System.Convert.ToInt32(e));
                case System.DateTime dt: return AppendDateTime(hash, dt);
                case System.Guid guid: return AppendGuid(hash, guid);
                default: return AppendInt32(hash, value.GetHashCode());
            }
        }

        /// <summary>
        /// 追加 Guid（标准实现）
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标 Guid</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>调用 <see cref="Guid.ToByteArray"/> 获取字节数组后追加，会产生 16 字节堆分配，性能较低（约 130 ns/次）。</para>
        /// </remarks>
        public static ulong AppendGuid(ulong hash, System.Guid value)
        {
            byte[] bytes = value.ToByteArray();
            for (int i = 0; i < bytes.Length; i++)
            {
                hash = AppendByte(hash, bytes[i]);
            }
            return hash;
        }
    }
}