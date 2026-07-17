namespace EasyMapper.Runtime
{
    /// <summary> 参数校验装饰器 </summary>
    /// <typeparam name="TSource"> 数据源类型 </typeparam>
    /// <typeparam name="TToken"> 令牌类型 </typeparam>
    /// <remarks>
    /// <para> 在导入时对 null 源、导出时对默认令牌进行校验，并抛出 <see cref="System.ArgumentNullException"/> 或 <see cref="System.ArgumentException"/>。 </para>
    /// </remarks>
    public sealed class GuardedPipeline<TSource, TToken> : IPipeline<TSource, TToken>
    where TToken : struct, System.IEquatable<TToken>
    {
        private readonly IPipeline<TSource, TToken> inner;

        /// <param name="inner"> 内部流水线 </param>
        public GuardedPipeline(IPipeline<TSource, TToken> inner)
        {
            this.inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
        }

        public TToken Import(TSource source)
        {
            if (source == null)
                throw new System.ArgumentNullException(nameof(source));

            TToken token = inner.Import(source);
            return token;
        }

        public TSource Export(TToken token)
        {
            if (token.Equals(default))
                throw new System.ArgumentException("Token cannot be default value.", nameof(token));

            return inner.Export(token);
        }
    }
}