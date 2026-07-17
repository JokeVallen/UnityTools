namespace EasyMapper.Runtime
{
    /// <summary> 幂等装饰器 </summary>
    /// <typeparam name="TSource"> 数据源类型 </typeparam>
    /// <typeparam name="TToken"> 令牌类型 </typeparam>
    /// <remarks>
    /// <para> 保证相同的数据源实例多次导入返回同一个令牌，内部维护双向字典缓存。 </para>
    /// <para> 适用于源对象自定义 <c>Equals</c> 比较但可能产生重复令牌的场景。 </para>
    /// </remarks>
    public sealed class IdempotentPipeline<TSource, TToken> : IPipeline<TSource, TToken>, IMaintainable
    where TToken : struct, System.IEquatable<TToken>
    {
        public int Count => registry.Count;
        private readonly IPipeline<TSource, TToken> inner;
        private readonly System.Collections.Generic.Dictionary<TSource, TToken> lookup;
        private readonly System.Collections.Generic.Dictionary<TToken, TSource> registry;

        /// <param name="inner"> 内部流水线 </param>
        public IdempotentPipeline(IPipeline<TSource, TToken> inner)
        {
            this.inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
            lookup = new System.Collections.Generic.Dictionary<TSource, TToken>();
            registry = new System.Collections.Generic.Dictionary<TToken, TSource>();
        }

        public TToken Import(TSource source)
        {
            if (lookup.TryGetValue(source, out TToken existing))
                return existing;

            TToken token = inner.Import(source);
            lookup[source] = token;
            registry[token] = source;
            return token;
        }

        public TSource Export(TToken token)
        {
            if (registry.TryGetValue(token, out TSource source))
                return source;
            return inner.Export(token);
        }

        public void Cleanup()
        {
            lookup.Clear();
            registry.Clear();
            if (inner is IMaintainable m) m.Cleanup();
        }
    }
}