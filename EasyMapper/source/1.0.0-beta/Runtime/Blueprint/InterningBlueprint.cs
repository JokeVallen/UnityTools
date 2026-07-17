namespace EasyMapper.Runtime
{
    /// <summary> 字符串驻留蓝图 </summary>
    /// <remarks>
    /// <para> 通过内部 <see cref="System.Collections.Generic.Dictionary{String, LongToken}"/> 和原子自增计数器为任意字符串分配唯一令牌。 </para>
    /// <para> 不可溯源（<see cref="IFeature.IsTraceable"/> 为 <c>false</c>），还原时必须依赖流水线存储。 </para>
    /// <para> 适用于长字符串或字符串内容包容性更大的情景。 </para>
    /// </remarks>
    public sealed class InterningBlueprint : IBlueprint<string, LongToken>, IFeature
    {
        public bool IsTraceable => false;
        private long counter = 0;
        private readonly System.Collections.Generic.Dictionary<string, LongToken> cache =
        new System.Collections.Generic.Dictionary<string, LongToken>();

        public LongToken Refine(string source)
        {
            if (cache.TryGetValue(source, out var token)) return token;
            token = new LongToken(System.Threading.Interlocked.Increment(ref counter));
            cache[source] = token;
            return token;
        }

        public string Restore(LongToken token) => throw new System.NotSupportedException();
    }
}