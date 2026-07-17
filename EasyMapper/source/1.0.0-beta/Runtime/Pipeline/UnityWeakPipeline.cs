namespace EasyMapper.Runtime
{
    /// <summary> Unity 对象弱引用流水线 </summary>
    /// <typeparam name="TSource"> 数据源类型（必须为引用类型） </typeparam>
    /// <typeparam name="TToken"> 令牌类型 </typeparam>
    /// <remarks>
    /// <para> 正向映射使用 <see cref="System.Runtime.CompilerServices.ConditionalWeakTable{TKey, TValue}"/>，反向使用 <see cref="System.WeakReference{T}"/>，不阻止对象被 GC 回收。 </para>
    /// <para> 特别适用于 <see cref="UnityEngine.Object"/> 子类，可在对象销毁后自动感知并返回 null。 </para>
    /// <para> <see cref="IMaintainable.Cleanup"/> 会遍历移除已失效的引用。 </para>
    /// </remarks>
    public sealed class UnityWeakPipeline<TSource, TToken> : IPipeline<TSource, TToken>, IMaintainable
    where TSource : class
    where TToken : struct, System.IEquatable<TToken>
    {
        public int Count => throw new System.NotSupportedException($"{nameof(System.Runtime.CompilerServices.ConditionalWeakTable<TSource, TokenWrapper<TToken>>)} is not countable.");
        private readonly IBlueprint<TSource, TToken> blueprint;
        private readonly IFeature feature;
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<TSource, TokenWrapper<TToken>> sourceToToken;
        private readonly System.Collections.Generic.Dictionary<TToken, System.WeakReference<TSource>> tokenToSource;

        /// <param name="blueprint"> 数据源到令牌的蓝图 </param>
        /// <param name="feature"> 蓝图特性 </param>
        public UnityWeakPipeline(IBlueprint<TSource, TToken> blueprint, IFeature feature)
        {
            this.blueprint = blueprint ?? throw new System.ArgumentNullException(nameof(blueprint));
            this.feature = feature ?? throw new System.ArgumentNullException(nameof(feature));
            sourceToToken = new System.Runtime.CompilerServices.ConditionalWeakTable<TSource, TokenWrapper<TToken>>();
            tokenToSource = new System.Collections.Generic.Dictionary<TToken, System.WeakReference<TSource>>();
        }

        public TToken Import(TSource source)
        {
            if (source is UnityEngine.Object obj && obj == null) return default;

            TToken token = blueprint.Refine(source);
            if (!sourceToToken.TryGetValue(source, out _))
            {
                sourceToToken.Add(source, new TokenWrapper<TToken>(token));

                if (!feature.IsTraceable)
                    tokenToSource[token] = new System.WeakReference<TSource>(source);
            }

            return token;
        }

        public TSource Export(TToken token)
        {
            if (feature.IsTraceable) return blueprint.Restore(token);

            if (tokenToSource.TryGetValue(token, out var weakRef))
            {
                if (weakRef.TryGetTarget(out TSource target))
                {
                    if (target is UnityEngine.Object obj && obj != null)
                        return target;
                    if (!(target is UnityEngine.Object) && target != null)
                        return target;
                }

                tokenToSource.Remove(token);
            }

            return null;
        }

        public void Cleanup()
        {
            var keysToRemove = new System.Collections.Generic.List<TToken>();
            foreach (var kvp in tokenToSource)
            {
                if (!kvp.Value.TryGetTarget(out TSource target))
                {
                    keysToRemove.Add(kvp.Key);
                    continue;
                }

                if (target is UnityEngine.Object obj && obj == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
                tokenToSource.Remove(key);
        }
    }
}