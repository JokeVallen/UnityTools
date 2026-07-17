using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 默认管线上下文集合
    /// </summary>
    internal sealed class DefaultPipelineContextCollection : IPipelineContextCollection, IAsyncDisposable
    {
        private readonly Stack<IPipelineContext> pool = new Stack<IPipelineContext>(DEFAULT_POOL_SIZE);
        private readonly Func<IPipelineContext> factory;
        private const int DEFAULT_POOL_SIZE = 8;
        private bool disposed;

        public DefaultPipelineContextCollection(Func<IPipelineContext> factory) 
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <inheritdoc/>
        public IPipelineContext Acquire()
        {
            ThrowErrorIfDisposed();
            IPipelineContext context;
            if (pool.Count > 0) context = pool.Pop();
            else context = factory();
            return context;
        }

        /// <inheritdoc/>
        public void Return(IPipelineContext context)
        {
            ThrowErrorIfDisposed();
            if (context == null) return;
            if (context is IResstable resstable) resstable.Reset();
            pool.Push(context);
        }

        /// <inheritdoc/>
        public async UniTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;
            while (pool.Count > 0)
            {
                var context = pool.Pop();
                if (context is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                (context as IDisposable)?.Dispose();
            }
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new InvalidOperationException("[ViewPipeline] The pipeline context collection has been disposed.");
        }
    }
}
