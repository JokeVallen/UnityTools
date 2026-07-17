namespace EasyMapper.Runtime
{
    /// <summary> 诊断装饰器 </summary>
    /// <typeparam name="TSource"> 数据源类型 </typeparam>
    /// <typeparam name="TToken"> 令牌类型 </typeparam>
    /// <remarks>
    /// <para> 记录 <c>Import</c> / <c>Export</c> 的总次数，并提供事件回调，方便调试与性能监控。 </para>
    /// <para> 所有统计操作通过 <see cref="System.Threading.Interlocked"/> 保证线程安全。 </para>
    /// </remarks>
    public sealed class DiagnosticPipeline<TSource, TToken> : IPipeline<TSource, TToken>
    where TToken : struct, System.IEquatable<TToken>
    {
        private readonly IPipeline<TSource, TToken> inner;
        private long importCount;
        private long exportCount;

        /// <summary> 导入总次数 </summary>
        public long ImportCount => System.Threading.Interlocked.Read(ref importCount);

        /// <summary> 导出总次数 </summary>
        public long ExportCount => System.Threading.Interlocked.Read(ref exportCount);

        /// <summary> 导入事件 </summary>
        public event System.Action<TSource, TToken> OnImport;

        /// <summary> 导出事件 </summary>
        public event System.Action<TToken, TSource> OnExport;

        /// <param name="inner"> 内部流水线 </param>
        public DiagnosticPipeline(IPipeline<TSource, TToken> inner)
        {
            this.inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
        }

        public TToken Import(TSource source)
        {
            System.Threading.Interlocked.Increment(ref importCount);
            TToken token = inner.Import(source);
            OnImport?.Invoke(source, token);
            return token;
        }

        public TSource Export(TToken token)
        {
            System.Threading.Interlocked.Increment(ref exportCount);
            TSource source = inner.Export(token);
            OnExport?.Invoke(token, source);
            return source;
        }

        /// <summary> 重置计数器 </summary>
        public void ResetCounters()
        {
            System.Threading.Interlocked.Exchange(ref importCount, 0);
            System.Threading.Interlocked.Exchange(ref exportCount, 0);
        }
    }
}