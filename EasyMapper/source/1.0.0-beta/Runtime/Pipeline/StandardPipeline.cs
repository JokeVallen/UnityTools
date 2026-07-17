namespace EasyMapper.Runtime
{
    /// <summary> 标准强引用流水线 </summary>
    /// <typeparam name="TSource"> 数据源类型 </typeparam>
    /// <typeparam name="TToken"> 令牌类型 </typeparam>
    /// <remarks>
    /// <para> 使用两个 <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> 维护双向映射，强引用会阻止数据源被 GC。 </para>
    /// <para> 适合值类型数据源，或需要长期持有的场景。 </para>
    /// <para> 实现 <see cref="IMaintainable"/>，可调用 <see cref="IMaintainable.Cleanup"/> 清空所有映射。 </para>
    /// </remarks>
    public sealed class StandardPipeline<TSource, TToken> : IPipeline<TSource, TToken>, IMaintainable
    where TToken : struct, System.IEquatable<TToken>
    {
        public int Count => registry.Count;
        private readonly IBlueprint<TSource, TToken> blueprint;
        private readonly IFeature feature;
        private readonly System.Collections.Generic.Dictionary<TToken, TSource> registry;
        private readonly System.Collections.Generic.Dictionary<TSource, TToken> lookup;

        /// <param name="blueprint"> 数据源到令牌的蓝图 </param>
        /// <param name="feature"> 蓝图特性（用于判断是否可溯源） </param>
        public StandardPipeline(IBlueprint<TSource, TToken> blueprint, IFeature feature)
        {
            this.blueprint = blueprint ?? throw new System.ArgumentNullException(nameof(blueprint));
            this.feature = feature ?? throw new System.ArgumentNullException(nameof(feature));
            registry = new System.Collections.Generic.Dictionary<TToken, TSource>();
            lookup = new System.Collections.Generic.Dictionary<TSource, TToken>();
        }

        public TToken Import(TSource source)
        {
            TToken token = blueprint.Refine(source);

            if (!lookup.ContainsKey(source))
            {
                lookup[source] = token;
                registry[token] = source;
            }

            return token;
        }

        public TSource Export(TToken token)
        {
            if (feature.IsTraceable) return blueprint.Restore(token);
            return registry.TryGetValue(token, out var source) ? source : default;
        }

        public void Cleanup()
        {
            registry.Clear();
            lookup.Clear();
        }
    }
}