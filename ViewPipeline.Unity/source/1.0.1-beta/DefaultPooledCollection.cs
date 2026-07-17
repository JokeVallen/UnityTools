using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    internal sealed class DefaultPooledCollection<TElement> : ICollection<TElement>, IAsyncDisposable
    {
        private readonly Stack<TElement> pool;
        private readonly Func<TElement> factory;
        private bool disposed;

        public DefaultPooledCollection(Func<TElement> factory) : this(factory, 32) { }

        public DefaultPooledCollection(Func<TElement> factory, int capacity) 
        {
            if(factory == null) throw new ArgumentNullException(nameof(factory));
            this.factory = factory;
            pool = new Stack<TElement>(capacity);
        }

        public TElement Acquire()
        {
            ThrowErrorIfDisposed();
            TElement context;
            if (pool.Count > 0) context = pool.Pop();
            else context = factory();
            return context;
        }

        public void Return(TElement context)
        {
            ThrowErrorIfDisposed();
            if (context == null) return;
            if (context is IResettable)
                ((IResettable)context).Reset();
            pool.Push(context);
        }

        public async UniTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;
            while (pool.Count > 0)
            {
                var context = pool.Pop();
                if(context is IAsyncDisposable)
                    await ((IAsyncDisposable)context).DisposeAsync();
                if(context is IDisposable)
                    ((IDisposable)context).Dispose();
            }
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(DefaultPooledCollection<TElement>), "[ViewPipeline] The pooled collection has been disposed.");
        }
    }
}
