using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    internal sealed class ViewPipelineEngine : IPipelineEngineInternal, IAsyncDisposable, IFullSnapshotable<ViewPipelineEngineSnapshot>
    {
        public Guid Key
        {
            get 
            {
                ThrowErrorIfDisposed();
                return key;
            }
        }

        public int ActiveOperations
        {
            get 
            {
                ThrowErrorIfDisposed();
                return activeOperations;
            }
        }

        public PipelineDirection Direction
        {
            get
            {
                ThrowErrorIfDisposed();
                return direction;
            }
        }

        private readonly Guid key;
        private readonly IViewMiddleware[] staticMiddlewares;
        private readonly List<IDynamicMiddlewareProvider> dynamicProviders;
        private readonly Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory;
        private readonly PipelineDirection direction;
        
        private List<IViewMiddleware> combinedCache;
        private IDynamicMiddlewareCollection dynamicMiddlewareCollection;
        private int activeOperations;
        private bool disposed;

        private static readonly List<IViewMiddleware[]> arrayPool = new List<IViewMiddleware[]>();
        private const int DEFAULT_ARRAY_CAPACITY = 32;
        private const int MAX_POOL_CAPACITY = 16;

        public ViewPipelineEngine(PipelineDirection direction, Guid key, IEnumerable<IViewMiddleware> staticMiddlewares)
        : this(direction, key, staticMiddlewares, null, null) { }

        public ViewPipelineEngine(PipelineDirection direction, Guid key, IEnumerable<IViewMiddleware> staticMiddlewares, IEnumerable<IDynamicMiddlewareProvider> dynamicProviders, Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory)
        {
            if (staticMiddlewares == null) throw new ArgumentNullException(nameof(staticMiddlewares));
            if (dynamicMiddlewareCollectionFactory == null) throw new ArgumentNullException(nameof(dynamicMiddlewareCollectionFactory));
            this.direction = direction;
            this.key = key;
            this.staticMiddlewares = ViewPipelineUtility.FilterAndToArray(staticMiddlewares);
            this.dynamicProviders = new List<IDynamicMiddlewareProvider>(dynamicProviders);
            this.dynamicMiddlewareCollectionFactory = dynamicMiddlewareCollectionFactory;
            SnapshotCache.OnRefresh += OnSnapshotRefresh;
        }

        void IPipelineEngine.RegisterDynamicProvider(IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfDisposed();
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dynamicProviders.Contains(provider)) throw new ArgumentException($"[ViewPipeline] The provider '{provider}' is already registered.");
            dynamicProviders.Add(provider);
        }

        void IPipelineEngine.UnregisterDynamicProvider(IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfDisposed();
            if (provider == null) return;
            dynamicProviders.Remove(provider);
        }

        UniTask IPipelineEngineInternal.ExecuteAsync(IView view, IPipelineContext context, IPipelineSession session, CancellationToken token)
        {
            ThrowErrorIfDisposed();
            return ExecuteInternalAsync(view, context, (PipelineSession)session, token);
        }

        public async UniTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;

            SnapshotCache.OnRefresh -= OnSnapshotRefresh;
            SnapshotCacheInternal<PipelineDirection>.Remove<ViewPipelineEngineSnapshot>(key, direction);

            for (int i = 0; i < staticMiddlewares.Length; i++)
            {
                if (staticMiddlewares[i] is IAsyncDisposable)
                    await ((IAsyncDisposable)staticMiddlewares[i]).DisposeAsync();
                if (staticMiddlewares[i] is IDisposable)
                    ((IDisposable)staticMiddlewares[i]).Dispose();
            }

            for (int i = 0; i < dynamicProviders.Count; i++)
            {
                if (dynamicProviders[i] is IAsyncDisposable)
                    await ((IAsyncDisposable)dynamicProviders[i]).DisposeAsync();
                if (dynamicProviders[i] is IDisposable)
                    ((IDisposable)dynamicProviders[i]).Dispose();
            }
            dynamicProviders.Clear();

            if(dynamicMiddlewareCollection is IAsyncDisposable)
                await ((IAsyncDisposable)dynamicMiddlewareCollection).DisposeAsync();
            if(dynamicMiddlewareCollection is IDisposable)
                ((IDisposable)dynamicMiddlewareCollection).Dispose();

            if(combinedCache != null) combinedCache.Clear();
        }

        public ViewPipelineEngineSnapshot GetFullSnapshot()
        {
            ThrowErrorIfDisposed();
            return new ViewPipelineEngineSnapshot(
                key,
                direction,
                staticMiddlewares.Where(m => m is IFullSnapshotable<MiddlewareSnapshot>)
                .Select(m => ((IFullSnapshotable<MiddlewareSnapshot>)m).GetFullSnapshot()).ToArray(),
                dynamicProviders.Where(p => p is IFullSnapshotable<DynamicMiddlewareProviderSnapshot>)
                .Select(p => ((IFullSnapshotable<DynamicMiddlewareProviderSnapshot>)p).GetFullSnapshot()).ToArray(),
                combinedCache == null ? Array.Empty<MiddlewareSnapshot>() : combinedCache.Where(m => m is IFullSnapshotable<MiddlewareSnapshot>)
                .Select(m => ((IFullSnapshotable<MiddlewareSnapshot>)m).GetFullSnapshot()).ToArray(),
                activeOperations
            );
        }

        private async UniTask ExecuteInternalAsync(IView view, IPipelineContext context, PipelineSession session, CancellationToken token)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            token.ThrowIfCancellationRequested();

            activeOperations++;
            try
            {
                if (combinedCache == null) combinedCache = new List<IViewMiddleware>(DEFAULT_ARRAY_CAPACITY);
                else combinedCache.Clear();

                if (dynamicMiddlewareCollection is IResettable resettable) resettable.Reset();
                else dynamicMiddlewareCollection = dynamicMiddlewareCollectionFactory();

                for (int i = 0; i < staticMiddlewares.Length; i++)
                    combinedCache.Add(staticMiddlewares[i]);

                for (int i = 0; i < dynamicProviders.Count; i++)
                    dynamicProviders[i].PopulateMiddlewares(view, dynamicMiddlewareCollection);

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
                    var executor = new ViewPipelineExecutor(activeArray, totalCount, 0, context, session);
                    await executor.NextAsync(view, token);
                }
                finally
                {
                    Array.Clear(activeArray, 0, totalCount);
                    ReturnArray(activeArray);
                }
            }
            finally 
            {
                activeOperations--;
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

        private void OnSnapshotRefresh(Guid key, Type type)
        {
            if (this.key != key) return;
            if (type != null && type != typeof(ViewPipelineEngineSnapshot)) return;
            var snapshot = GetFullSnapshot();
            SnapshotCacheInternal<PipelineDirection>.Store(key, snapshot, direction);
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ViewPipelineEngine), "[ViewPipeline] The pipeline engine has been disposed.");
        }
    }
}