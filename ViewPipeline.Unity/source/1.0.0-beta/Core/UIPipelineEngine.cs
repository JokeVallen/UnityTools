using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 执行管道引擎
    /// </summary>
    internal sealed class UIPipelineEngine : IPipelineEngineInternal, IAsyncDisposable
    {
        private readonly Guid key;
        private readonly IViewMiddleware[] staticMiddlewares;
        private readonly List<IDynamicMiddlewareProvider> dynamicProviders;
        private readonly Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory;
        private readonly IReadOnlyList<IViewMiddleware> readOnlyStaticMiddlewares;
        private bool disposed;

        private List<IViewMiddleware> combinedCache;
        private IDynamicMiddlewareCollection dynamicMiddlewareCollection;
        private static readonly List<IViewMiddleware[]> arrayPool = new List<IViewMiddleware[]>();

        private const int DEFAULT_ARRAY_CAPACITY = 32;
        private const int MAX_POOL_CAPACITY = 16;

        /// <param name="staticMiddlewares">静态中间件集合</param>
        /// <param name="dynamicMiddlewareCollectionFactory">动态中间件集合工厂方法</param>
        public UIPipelineEngine(Guid key, IEnumerable<IViewMiddleware> staticMiddlewares, Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory)
        : this(key, staticMiddlewares, null, dynamicMiddlewareCollectionFactory) { }

        /// <param name="staticMiddlewares">静态中间件集合</param>
        /// <param name="dynamicProviders">动态中间件流式供应器集合</param>
        /// <param name="dynamicMiddlewareCollectionFactory">动态中间件集合工厂方法</param>
        public UIPipelineEngine(Guid key, IEnumerable<IViewMiddleware> staticMiddlewares, IEnumerable<IDynamicMiddlewareProvider> dynamicProviders, Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory)
        {
            this.key = key;
            this.staticMiddlewares = ViewPipelineUtility.FilterAndToArray(staticMiddlewares);
            this.dynamicProviders = new List<IDynamicMiddlewareProvider>(dynamicProviders);
            this.dynamicMiddlewareCollectionFactory = dynamicMiddlewareCollectionFactory;
            readOnlyStaticMiddlewares = Array.AsReadOnly(this.staticMiddlewares);
        }

        /// <inheritdoc/>
        void IPipelineEngine.RegisterDynamicProvider(IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfDisposed();
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dynamicProviders.Contains(provider)) throw new ArgumentException($"[ViewPipeline] The provider '{provider}' is already registered.");
            dynamicProviders.Add(provider);
        }

        /// <inheritdoc/>
        void IPipelineEngine.UnregisterDynamicProvider(IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfDisposed();
            if (provider == null) return;
            dynamicProviders.Remove(provider);
        }

        /// <inheritdoc/>
        UniTask IPipelineEngineInternal.ExecuteAsync(IView view, IPipelineContext context, IPipelineSession session, CancellationToken token)
        {
            ThrowErrorIfDisposed();
            return ExecuteInternalAsync(view, context, (PipelineSession)session, token);
        }

        /// <inheritdoc/>
        public async UniTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;

            for (int i = 0; i < staticMiddlewares.Length; i++)
            {
                if (staticMiddlewares[i] is IAsyncDisposable asyncStaticMiddleware) 
                    await asyncStaticMiddleware.DisposeAsync();
                (staticMiddlewares[i] as IDisposable)?.Dispose();
            }

            for (int i = 0; i < dynamicProviders.Count; i++)
            {
                if (dynamicProviders[i] is IAsyncDisposable asyncDynamicProvider)
                    await asyncDynamicProvider.DisposeAsync();
                (dynamicProviders[i] as IDisposable)?.Dispose();
            }

            if (dynamicMiddlewareCollection is IAsyncDisposable asyncDynamicMiddlewareCollection)
                await asyncDynamicMiddlewareCollection.DisposeAsync();
            (dynamicMiddlewareCollection as IDisposable)?.Dispose();
        }

        private async UniTask ExecuteInternalAsync(IView view, IPipelineContext context, PipelineSession session, CancellationToken token)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            token.ThrowIfCancellationRequested();

            if (combinedCache == null) combinedCache = new List<IViewMiddleware>(DEFAULT_ARRAY_CAPACITY);
            else combinedCache.Clear();

            if (dynamicMiddlewareCollection is IResstable resstable) resstable.Reset();
            else dynamicMiddlewareCollection = dynamicMiddlewareCollectionFactory();

            for (int i = 0; i < staticMiddlewares.Length; i++)
                combinedCache.Add(staticMiddlewares[i]);

            for (int i = 0; i < dynamicProviders.Count; i++)
                dynamicProviders[i].PopulateMiddlewares(view, readOnlyStaticMiddlewares, dynamicMiddlewareCollection);

            combinedCache.AddRange(dynamicMiddlewareCollection);

            int totalCount = combinedCache.Count;
            if (totalCount == 0)
            {
                session.Complete();
                if (session.Direction == PipelineDirection.Close)
                    await view.HideAsync(token);
                else
                    await view.ShowAsync(token);
                return;
            }

            IViewMiddleware[] activeArray = RentArray(totalCount);
            for (int i = 0; i < totalCount; i++)
                activeArray[i] = combinedCache[i];

            try
            {
                var executor = new UIPipelineExecutor(key, activeArray, totalCount, 0, context, session);
                await executor.NextAsync(view, token);
            }
            finally
            {
                Array.Clear(activeArray, 0, totalCount);
                ReturnArray(activeArray);
            }
        }

        private static IViewMiddleware[] RentArray(int minLength)
        {
            for (int i = arrayPool.Count - 1; i >= 0; i--)
            {
                var arr = arrayPool[i];
                if (arr != null && arr.Length >= minLength)
                {
                    arrayPool.RemoveAt(i);
                    return arr;
                }
            }

            int size = DEFAULT_ARRAY_CAPACITY;
            while (size < minLength) size *= 2;
            return new IViewMiddleware[size];
        }

        private static void ReturnArray(IViewMiddleware[] array)
        {
            if (arrayPool.Count < MAX_POOL_CAPACITY)
                arrayPool.Add(array);
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new InvalidOperationException("[ViewPipeline] The pipeline engine has been disposed.");
        }
    }
}