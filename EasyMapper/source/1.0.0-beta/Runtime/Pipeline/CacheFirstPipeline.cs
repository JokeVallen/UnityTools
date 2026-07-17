namespace EasyMapper.Runtime
{
    /// <summary> 缓存优先装饰器 </summary>
    /// <typeparam name="TSource"> 数据源类型（引用类型） </typeparam>
    /// <typeparam name="TToken"> 令牌类型 </typeparam>
    /// <remarks>
    /// <para> 使用 <see cref="System.Runtime.CompilerServices.ConditionalWeakTable{TKey, TValue}"/> 缓存已导入的源对象，后续导入同一对象直接返回已分配的令牌，避免重复调用内部流水线的 <c>Import</c>。 </para>
    /// <para> 仅缓存源对象到令牌的映射，导出仍委托内部流水线。 </para>
    /// </remarks>
    public sealed class CacheFirstPipeline<TSource, TToken> : IPipeline<TSource, TToken>
    where TSource : class
    where TToken : struct, System.IEquatable<TToken>
    {
        private readonly IPipeline<TSource, TToken> inner;
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<TSource, TokenWrapper<TToken>> cache;

        /// <param name="inner"> 内部流水线 </param>
        public CacheFirstPipeline(IPipeline<TSource, TToken> inner)
        {
            this.inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
            cache = new System.Runtime.CompilerServices.ConditionalWeakTable<TSource, TokenWrapper<TToken>>();
        }

        public TToken Import(TSource source)
        {
            if (cache.TryGetValue(source, out var existing))
                return existing.Value;

            TToken token = inner.Import(source);
            cache.Add(source, new TokenWrapper<TToken>(token));
            return token;
        }

        public TSource Export(TToken token)
        {
            return inner.Export(token);
        }
    }
}