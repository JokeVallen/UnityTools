namespace EasyMapper.Runtime
{
    /// <summary> 线程安全装饰器 </summary>
    /// <typeparam name="TSource"> 数据源类型 </typeparam>
    /// <typeparam name="TToken"> 令牌类型 </typeparam>
    /// <remarks>
    /// <para> 通过 <c>lock</c> 语句包装内部流水线的所有操作，确保多线程环境下的安全访问。 </para>
    /// <para> 可包裹任何 <see cref="IPipeline{TSource, TToken}"/> 实例。 </para>
    /// </remarks>
    public sealed class ThreadSafePipeline<TSource, TToken> : IPipeline<TSource, TToken>, IMaintainable
    where TToken : struct, System.IEquatable<TToken>
    {
        public int Count
        {
            get
            {
                lock (@lock)
                    return inner is IMaintainable m ? m.Count : throw new System.NotSupportedException();
            }
        }

        private readonly IPipeline<TSource, TToken> inner;
        private readonly object @lock = new object();

        /// <param name="inner"> 被包装的流水线 </param>
        public ThreadSafePipeline(IPipeline<TSource, TToken> inner)
        {
            this.inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
        }

        public TToken Import(TSource source)
        {
            lock (@lock) return inner.Import(source);
        }

        public TSource Export(TToken token)
        {
            lock (@lock) return inner.Export(token);
        }

        public void Cleanup()
        {
            if (!(inner is IMaintainable maintainable))
                throw new System.NotSupportedException();
            lock (@lock) maintainable.Cleanup();
        }
    }
}