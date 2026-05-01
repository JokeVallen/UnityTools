namespace FNV1A.x64
{
    public static partial class FNV1AUtility
    {
        /// <summary>
        /// 集合类型哈希缓存
        /// </summary>
        /// <typeparam name="TCollection">集合类型</typeparam>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <remarks>
        /// <para>为特定集合类型（如数组、<see cref="System.Collections.Generic.List{T}"/> 等）生成默认的哈希追加委托，并允许外部覆盖。</para>
        /// <para>静态构造函数中通过反射获取对应的 Append 方法并创建委托，后续调用无反射开销。</para>
        /// </remarks>
        public static class CollectionFNVHasherCache<TCollection, TElement>
        {
            /// <summary>
            /// 哈希追加委托
            /// </summary>
            /// <remarks>
            /// <para>获取或设置用于计算集合哈希的委托。委托接收当前哈希、集合实例和元素哈希器。</para>
            /// </remarks>
            public static System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong> Hasher
            {
                get => hasher ?? defaultHasher;
                set => hasher = value;
            }
            private static System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong> hasher;
            private static readonly System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong> defaultHasher;

            static CollectionFNVHasherCache()
            {
                System.Type type = typeof(TCollection);

                if (typeof(IFNVHashable).IsAssignableFrom(type))
                    defaultHasher = (h, v, eh) => v == null ? AppendByte(h, 0) : ((IFNVHashable)v).AppendHash(h);
                else if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    var method = typeof(FNV1AUtility).GetMethod(nameof(AppendArray)).MakeGenericMethod(elementType);
                    defaultHasher = (System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong>)System.Delegate.CreateDelegate(typeof(System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong>), method);
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                {
                    var elementType = type.GetGenericArguments()[0];
                    var method = typeof(FNV1AUtility).GetMethod(nameof(AppendList)).MakeGenericMethod(elementType);
                    defaultHasher = (System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong>)System.Delegate.CreateDelegate(typeof(System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong>), method);
                }
                else if (type.IsGenericType && typeof(System.Collections.Generic.IList<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                {
                    var elementType = type.GetGenericArguments()[0];
                    var method = typeof(FNV1AUtility).GetMethod(nameof(AppendIListGeneric)).MakeGenericMethod(elementType);
                    defaultHasher = (System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong>)System.Delegate.CreateDelegate(typeof(System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong>), method);
                }
                else
                    defaultHasher = (h, v, eh) => v == null ? AppendByte(h, 0) : AppendInt32(h, v.GetHashCode());
            }
        }

        /// <summary>
        /// 注册集合类型哈希器
        /// </summary>
        /// <typeparam name="TCollection">集合类型</typeparam>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="hasher">自定义哈希委托</param>
        /// <remarks>
        /// <para>允许为特定集合类型注册自定义的哈希计算逻辑。</para>
        /// </remarks>
        public static void RegisterHasherForCollection<TCollection, TElement>(System.Func<ulong, TCollection, System.Func<ulong, TElement, ulong>, ulong> hasher)
        {
            CollectionFNVHasherCache<TCollection, TElement>.Hasher = hasher;
        }

        /// <summary>
        /// 追加数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标数组</param>
        /// <param name="elementHasher">元素哈希器</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>若数组为 <c>null</c>，则追加 0 字节。</para>
        /// <para>否则先追加数组长度（32 位整数），再依次对每个元素调用 <paramref name="elementHasher"/> 追加。</para>
        /// </remarks>
        public static ulong AppendArray<T>(ulong hash, T[] value, System.Func<ulong, T, ulong> elementHasher)
        {
            if (value == null) return AppendByte(hash, 0);
            int len = value.Length;
            hash = AppendInt32(hash, len);
            for (int i = 0; i < len; i++)
                hash = elementHasher(hash, value[i]);
            return hash;
        }

        /// <summary>
        /// 追加 List
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标列表</param>
        /// <param name="elementHasher">元素哈希器</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>处理方式与 <see cref="AppendArray{T}"/> 类似，先追加列表元素个数，再依次追加每个元素。</para>
        /// </remarks>
        public static ulong AppendList<T>(ulong hash, System.Collections.Generic.List<T> value, System.Func<ulong, T, ulong> elementHasher)
        {
            if (value == null) return AppendByte(hash, 0);
            int count = value.Count;
            hash = AppendInt32(hash, count);
            for (int i = 0; i < count; i++)
                hash = elementHasher(hash, value[i]);
            return hash;
        }

        /// <summary>
        /// 追加泛型 IList
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标集合</param>
        /// <param name="elementHasher">元素哈希器</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>若 <paramref name="value"/> 实际为数组或 <see cref="System.Collections.Generic.List{T}"/>，则转发至对应的专用方法以获得更好性能；否则按 <see cref="IList{T}.Count"/> 遍历追加。</para>
        /// </remarks>
        public static ulong AppendIListGeneric<T>(ulong hash, System.Collections.Generic.IList<T> value, System.Func<ulong, T, ulong> elementHasher)
        {
            if (value == null) return AppendByte(hash, 0);

            if (value is T[] array) return AppendArray(hash, array, elementHasher);
            if (value is System.Collections.Generic.List<T> list) return AppendList(hash, list, elementHasher);

            int count = value.Count;
            hash = AppendInt32(hash, count);
            for (int i = 0; i < count; i++)
                hash = elementHasher(hash, value[i]);
            return hash;
        }

        /// <summary>
        /// 追加非泛型 IList
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标集合</param>
        /// <param name="elementHasher">元素哈希器</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>适用于实现 <see cref="System.Collections.IList"/> 接口的非泛型集合。元素以 <see cref="object"/> 形式传递给哈希器。</para>
        /// </remarks>
        public static ulong AppendIList(ulong hash, System.Collections.IList value, System.Func<ulong, object, ulong> elementHasher)
        {
            if (value == null) return AppendByte(hash, 0);

            int count = value.Count;
            hash = AppendInt32(hash, count);
            for (int i = 0; i < count; i++)
                hash = elementHasher(hash, value[i]);
            return hash;
        }

        /// <summary>
        /// 泛型追加集合
        /// </summary>
        /// <typeparam name="TCollection">集合类型</typeparam>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标集合</param>
        /// <param name="elementHasher">元素哈希器</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>通过 <see cref="CollectionFNVHasherCache{TCollection, TElement}"/> 获取对应的哈希委托并执行。</para>
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong AppendForCollection<TCollection, TElement>(ulong hash, TCollection value, System.Func<ulong, TElement, ulong> elementHasher)
        {
            return CollectionFNVHasherCache<TCollection, TElement>.Hasher(hash, value, elementHasher);
        }

        /// <summary>
        /// 非泛型追加集合
        /// </summary>
        /// <param name="hash">当前哈希值</param>
        /// <param name="value">目标对象</param>
        /// <param name="elementHasher">元素哈希器</param>
        /// <returns>更新后的哈希值</returns>
        /// <remarks>
        /// <para>目前仅特殊处理 <see cref="System.Collections.IList"/> 类型，其他类型回退到 <see cref="object.GetHashCode"/>。</para>
        /// </remarks>
        public static ulong AppendForCollection(ulong hash, object value, System.Func<ulong, object, ulong> elementHasher)
        {
            if (value == null) return AppendByte(hash, 0);

            switch (value)
            {
                case System.Collections.IList list: return AppendIList(hash, list, elementHasher);
                default: return AppendInt32(hash, value.GetHashCode());
            }
        }
    }
}