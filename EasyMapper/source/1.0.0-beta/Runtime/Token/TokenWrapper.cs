namespace EasyMapper.Runtime
{
    internal sealed class TokenWrapper<TToken> where TToken : struct
    {
        public TToken Value => value;
        private readonly TToken value;
        public TokenWrapper(TToken value) => this.value = value;
    }
}