namespace FNV1A.x64
{
    public static partial class FNV1AUtility
    {
        /// <summary>
        /// Unity 类型哈希缓存
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <remarks>
        /// <para>为 Unity 特有类型（如 <see cref="UnityEngine.Vector3"/>、<see cref="UnityEngine.Color"/> 等）提供默认的哈希追加委托。</para>
        /// <para>若类型未特化处理，则回退到 <see cref="object.GetHashCode"/>。</para>
        /// </remarks>
        public static class UnityFNVHasherCache<T>
        {
            /// <summary>
            /// 哈希追加委托
            /// </summary>
            /// <remarks>
            /// <para>获取或设置用于计算 <typeparamref name="T"/> 类型哈希的委托。</para>
            /// </remarks>
            public static System.Func<ulong, T, ulong> Hasher
            {
                get => hasher ?? defaultHasher;
                set => hasher = value;
            }
            private static System.Func<ulong, T, ulong> hasher;
            private static readonly System.Func<ulong, T, ulong> defaultHasher;

            static UnityFNVHasherCache()
            {
                System.Type type = typeof(T);

                if (typeof(IFNVHashable).IsAssignableFrom(type))
                    defaultHasher = (h, v) => v == null ? AppendByte(h, 0) : ((IFNVHashable)v).AppendHash(h);
                else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, UnityEngine.Object, ulong>(AppendUnityObject);
                else if (type == typeof(UnityEngine.Vector2))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, UnityEngine.Vector2, ulong>(AppendVector2);
                else if (type == typeof(UnityEngine.Vector3))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, UnityEngine.Vector3, ulong>(AppendVector3);
                else if (type == typeof(UnityEngine.Vector4))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, UnityEngine.Vector4, ulong>(AppendVector4);
                else if (type == typeof(UnityEngine.Quaternion))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, UnityEngine.Quaternion, ulong>(AppendQuaternion);
                else if (type == typeof(UnityEngine.Color))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, UnityEngine.Color, ulong>(AppendColor);
                else if (type == typeof(UnityEngine.Rect))
                    defaultHasher = (System.Func<ulong, T, ulong>)(System.Delegate)new System.Func<ulong, UnityEngine.Rect, ulong>(AppendRect);
                else
                    defaultHasher = (h, v) => v == null ? AppendByte(h, 0) : AppendInt32(h, v.GetHashCode());
            }
        }

        /// <summary>
        /// 注册 Unity 类型哈希器
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="hasher">自定义哈希委托</param>
        /// <remarks>
        /// <para>允许为指定 Unity 类型注册自定义的哈希计算逻辑，覆盖默认行为。</para>
        /// </remarks>
        public static void RegisterHasherForUnity<T>(System.Func<ulong, T, ulong> hasher)
        {
            UnityFNVHasherCache<T>.Hasher = hasher;
        }

        /// <summary>
        /// 追加 Unity 对象
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标对象</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>若对象为 <c>null</c>，则追加 0 字节；否则使用 <see cref="UnityEngine.Object.GetInstanceID"/> 返回的实例 ID 作为整数追加。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendUnityObject(ulong hash, UnityEngine.Object value)
        {
            if (value == null) return AppendByte(hash, 0);
            return AppendInt32(hash, value.GetInstanceID());
        }

        /// <summary>
        /// 追加 Vector2
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标向量</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>依次追加 x 和 y 分量。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendVector2(ulong hash, UnityEngine.Vector2 value)
        {
            hash = AppendFloat(hash, value.x);
            return AppendFloat(hash, value.y);
        }

        /// <summary>
        /// 追加 Vector3
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标向量</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>依次追加 x、y、z 分量。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendVector3(ulong hash, UnityEngine.Vector3 value)
        {
            hash = AppendFloat(hash, value.x);
            hash = AppendFloat(hash, value.y);
            hash = AppendFloat(hash, value.z);
            return hash;
        }

        /// <summary>
        /// 追加 Vector4
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标向量</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>依次追加 x、y、z、w 分量。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendVector4(ulong hash, UnityEngine.Vector4 value)
        {
            hash = AppendFloat(hash, value.x);
            hash = AppendFloat(hash, value.y);
            hash = AppendFloat(hash, value.z);
            return AppendFloat(hash, value.w);
        }

        /// <summary>
        /// 追加 Quaternion
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标四元数</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>依次追加 x、y、z、w 分量。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendQuaternion(ulong hash, UnityEngine.Quaternion value)
        {
            hash = AppendFloat(hash, value.x);
            hash = AppendFloat(hash, value.y);
            hash = AppendFloat(hash, value.z);
            return AppendFloat(hash, value.w);
        }

        /// <summary>
        /// 追加 Color
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标颜色</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>依次追加 r、g、b、a 分量。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendColor(ulong hash, UnityEngine.Color value)
        {
            hash = AppendFloat(hash, value.r);
            hash = AppendFloat(hash, value.g);
            hash = AppendFloat(hash, value.b);
            hash = AppendFloat(hash, value.a);
            return hash;
        }

        /// <summary>
        /// 追加 Rect
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标矩形</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>依次追加 x、y、width、height 分量。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendRect(ulong hash, UnityEngine.Rect value)
        {
            hash = AppendFloat(hash, value.x);
            hash = AppendFloat(hash, value.y);
            hash = AppendFloat(hash, value.width);
            return AppendFloat(hash, value.height);
        }

        /// <summary>
        /// 泛型追加 Unity 类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标值</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>通过 <see cref="UnityFNVHasherCache{T}"/> 获取对应的哈希委托并执行。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendForUnity<T>(ulong hash, T value)
        {
            return UnityFNVHasherCache<T>.Hasher(hash, value);
        }

        /// <summary>
        /// 非泛型追加 Unity 类型
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标对象</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>通过 <c>switch</c> 对常见 Unity 类型进行分发，避免值类型装箱。</para>
        /// </remarks>
        public static ulong AppendForUnity(ulong hash, object value)
        {
            if (value == null) return AppendByte(hash, 0);

            switch (value)
            {
                case UnityEngine.Object uobj: return AppendUnityObject(hash, uobj);
                case UnityEngine.Vector2 v2: return AppendVector2(hash, v2);
                case UnityEngine.Vector3 v3: return AppendVector3(hash, v3);
                case UnityEngine.Vector4 v4: return AppendVector4(hash, v4);
                case UnityEngine.Quaternion q: return AppendQuaternion(hash, q);
                case UnityEngine.Color c: return AppendColor(hash, c);
                case UnityEngine.Rect re: return AppendRect(hash, re);
                default: return AppendInt32(hash, value.GetHashCode());
            }
        }
    }
}