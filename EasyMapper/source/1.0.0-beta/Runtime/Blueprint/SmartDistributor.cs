namespace EasyMapper.Runtime
{
    /// <summary> 智能字符串分发器 </summary>
    /// <remarks>
    /// <para> 组合快速路径与回退蓝图，根据输入字符串特征自动选择最优编码方案。 </para>
    /// <para> 决策规则：长度≤10 且全部字符合法则走快速路径（如 <see cref="Char10PackingBlueprint"/>），否则回退。 </para>
    /// <para> 使用令牌最高位（bit63）标记来源，方便还原时路由到正确的蓝图。 </para>
    /// <para> 本身不可溯源，因为混合了两种不同性质的蓝图。 </para>
    /// </remarks>
    public sealed class SmartDistributor : IBlueprint<string, LongToken>, IFeature
    {
        public bool IsTraceable => false;
        private readonly IBlueprint<string, LongToken> fastPath;
        private readonly IBlueprint<string, LongToken> fallback;

        /// <param name="fastPath"> 快速路径蓝图 </param>
        /// <param name="fallback"> 回退蓝图 </param>
        public SmartDistributor(IBlueprint<string, LongToken> fastPath, IBlueprint<string, LongToken> fallback)
        {
            this.fastPath = fastPath;
            this.fallback = fallback;
        }

        public LongToken Refine(string source)
        {
            if (string.IsNullOrEmpty(source)) return new LongToken(0);

            if (CanUseFastPath(source))
                return fastPath.Refine(source);

            var token = fallback.Refine(source);
            return MarkAsFallback(token);
        }

        public string Restore(LongToken token)
        {
            if (IsFallback(token))
                return fallback.Restore(Unmark(token));
            return fastPath.Restore(token);
        }

        private bool CanUseFastPath(string s)
        {
            if (s.Length > 10) return false;
            foreach (var c in s)
            {
                if (!IsLegalForFastPath(c))
                    return false;
            }
            return true;
        }

        private bool IsLegalForFastPath(char c)
        {
            char lower = (c >= 'A' && c <= 'Z') ? (char)(c + 32) : c;
            return (lower >= 'a' && lower <= 'z')
                || (lower >= '0' && lower <= '9')
                || lower == '_'
                || lower == '-';
        }

        private LongToken MarkAsFallback(LongToken t) => new LongToken(t.Value | (1L << 63));
        private bool IsFallback(LongToken t) => (t.Value & (1L << 63)) != 0;
        private LongToken Unmark(LongToken t) => new LongToken(t.Value & ~(1L << 63));
    }
}