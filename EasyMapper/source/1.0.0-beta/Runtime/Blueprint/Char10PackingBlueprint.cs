namespace EasyMapper.Runtime
{
    /// <summary> 10字符短字符串可逆编码蓝图 </summary>
    /// <remarks>
    /// <para> 用于将长度≤10 且仅含 [a-z0-9_-] 的字符串直接编码到 <see cref="LongToken"/> 的低 60 位，双向可逆。 </para>
    /// <para> 每字符占 6 bit，字符集共 38 个符号。空字符串或 null 编码为 0。 </para>
    /// <para> 该蓝图标记为可溯源（<see cref="IFeature.IsTraceable"/> 返回 <c>true</c>）。 </para>
    /// </remarks>
    public sealed class Char10PackingBlueprint : IBlueprint<string, LongToken>, IFeature
    {
        public bool IsTraceable => true;
        private const int BITS_PER_CHAR = 6;
        private const int MAX_CHARS = 10;
        private const long CHAR_MASK = (1L << BITS_PER_CHAR) - 1;

        public LongToken Refine(string source)
        {
            if (source == null) return new LongToken(0);

            string input = source.ToLower();
            int length = System.Math.Min(input.Length, MAX_CHARS);

            long result = 0;
            for (int i = 0; i < length; i++)
            {
                char c = input[i];
                long val = MapCharToFiveBit(c);
                result |= val << (i * BITS_PER_CHAR);
            }

            return new LongToken(result);
        }

        public string Restore(LongToken token)
        {
            if (token.Value == 0) return string.Empty;

            System.Text.StringBuilder sb = new System.Text.StringBuilder(MAX_CHARS);
            long data = token.Value;

            for (int i = 0; i < MAX_CHARS; i++)
            {
                int val = (int)((data >> (i * BITS_PER_CHAR)) & CHAR_MASK);
                if (val == 0) break;
                sb.Append(MapFiveBitToChar(val));
            }

            return sb.ToString();
        }

        private long MapCharToFiveBit(char c)
        {
            if (c >= 'a' && c <= 'z') return c - 'a' + 1;
            if (c == '_') return 27;
            if (c == '-') return 28;
            if (c >= '0' && c <= '9') return c - '0' + 29;
            return 0;
        }

        private char MapFiveBitToChar(int val)
        {
            if (val >= 1 && val <= 26) return (char)('a' + val - 1);
            if (val == 27) return '_';
            if (val == 28) return '-';
            if (val >= 29 && val <= 38) return (char)('0' + val - 29);
            return '\0';
        }
    }
}