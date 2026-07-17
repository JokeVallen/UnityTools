namespace EasyMapper.Runtime
{
    /// <summary> LRU 容量限制流水线 </summary>
    /// <typeparam name="TSource"> 数据源类型 </typeparam>
    /// <typeparam name="TToken"> 令牌类型 </typeparam>
    /// <remarks>
    /// <para> 独立管理映射存储，不依赖内部流水线存储。当条目数达到 <c>maxEntries</c> 时，按最近最少使用（LRU）策略淘汰旧条目。 </para>
    /// <para> 若蓝图可溯源，导出时未命中本地缓存会尝试通过蓝图还原；否则返回默认值。 </para>
    /// </remarks>
    public sealed class CappedPipeline<TSource, TToken> : IPipeline<TSource, TToken>, IMaintainable
    where TToken : struct, System.IEquatable<TToken>
    {
        public int Count => accessOrder.Count;
        private readonly IBlueprint<TSource, TToken> blueprint;
        private readonly IFeature feature;
        private readonly int maxEntries;
        private readonly System.Collections.Generic.Dictionary<TToken, System.Collections.Generic.LinkedListNode<(TSource source, TToken token)>> tokenToNode;
        private readonly System.Collections.Generic.Dictionary<TSource, System.Collections.Generic.LinkedListNode<(TSource source, TToken token)>> sourceToNode;
        private readonly System.Collections.Generic.LinkedList<(TSource source, TToken token)> accessOrder;

        /// <param name="blueprint"> 数据源到令牌的蓝图 </param>
        /// <param name="feature"> 蓝图特性 </param>
        /// <param name="maxEntries"> 最大容量 </param>
        public CappedPipeline(IBlueprint<TSource, TToken> blueprint, IFeature feature, int maxEntries)
        {
            this.blueprint = blueprint ?? throw new System.ArgumentNullException(nameof(blueprint));
            this.feature = feature ?? throw new System.ArgumentNullException(nameof(feature));
            if (maxEntries < 1) throw new System.ArgumentOutOfRangeException(nameof(maxEntries));
            this.maxEntries = maxEntries;

            tokenToNode = new System.Collections.Generic.Dictionary<TToken, System.Collections.Generic.LinkedListNode<(TSource source, TToken token)>>();
            sourceToNode = new System.Collections.Generic.Dictionary<TSource, System.Collections.Generic.LinkedListNode<(TSource source, TToken token)>>();
            accessOrder = new System.Collections.Generic.LinkedList<(TSource source, TToken token)>();
        }

        public TToken Import(TSource source)
        {
            if (sourceToNode.TryGetValue(source, out var node))
            {
                accessOrder.Remove(node);
                accessOrder.AddLast(node);
                return node.Value.token;
            }

            TToken token = blueprint.Refine(source);

            // 淘汰最旧的条目
            while (accessOrder.Count >= maxEntries)
            {
                var oldest = accessOrder.First;
                accessOrder.RemoveFirst();
                tokenToNode.Remove(oldest.Value.token);
                sourceToNode.Remove(oldest.Value.source);
            }

            var newNode = new System.Collections.Generic.LinkedListNode<(TSource, TToken)>((source, token));
            accessOrder.AddLast(newNode);
            tokenToNode[token] = newNode;
            sourceToNode[source] = newNode;

            return token;
        }

        public TSource Export(TToken token)
        {
            if (tokenToNode.TryGetValue(token, out var node))
            {
                accessOrder.Remove(node);
                accessOrder.AddLast(node);
                return node.Value.source;
            }

            if (feature.IsTraceable)
                return blueprint.Restore(token);

            return default;
        }

        public void Cleanup()
        {
            accessOrder.Clear();
            tokenToNode.Clear();
            sourceToNode.Clear();
        }
    }
}