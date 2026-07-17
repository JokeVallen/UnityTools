using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    internal sealed class ViewSession : IExtendedViewSession, IFullSnapshotable<ViewSessionSnapshot>
    {
        public Guid Key
        {
            get 
            {
                ThrowErrorIfDisposed();
                return key;
            }
        }

        private readonly Guid key;
        private readonly IPipelineEngineInternal openPipelineEngine;
        private readonly IPipelineEngineInternal closePipelineEngine;
        private readonly ICollection<IPipelineContext> contextCollection;
        private readonly List<IExtension> extensions;
        private bool disposed;

        public ViewSession(Guid key, IPipelineEngineInternal openPipelineEngine, IPipelineEngineInternal closePipelineEngine, ICollection<IPipelineContext> contextCollection, List<IExtension> extensions)
        {
            if (openPipelineEngine == null) throw new ArgumentNullException(nameof(openPipelineEngine));
            if (closePipelineEngine == null) throw new ArgumentNullException(nameof(closePipelineEngine));
            if (contextCollection == null) throw new ArgumentNullException(nameof(contextCollection));
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
            this.key = key;
            this.openPipelineEngine = openPipelineEngine;
            this.closePipelineEngine = closePipelineEngine;
            this.contextCollection = contextCollection;
            this.extensions = extensions;
            SnapshotCache.OnRefresh += OnSnapshotRefresh;
        }

        public async UniTask OpenViewAsync(IView view, CancellationToken cancellationToken)
        {
            ThrowErrorIfDisposed();
            if (view == null) throw new ArgumentNullException(nameof(view));
            cancellationToken.ThrowIfCancellationRequested();
            if (ViewPipelineUtility.ShouldTerminate(key, view)) return;

            var context = contextCollection.Acquire();
            var session = PipelineSession.Acquire(key);
            session.Direction = PipelineDirection.Open;
            try
            {
                await openPipelineEngine.ExecuteAsync(view, context, session, cancellationToken);
                if (!session.IsTerminalReached)
                    Log.Logger.Info($"[ViewPipeline] Detected that the activation pipeline for view [{view.GetType().Name}] was intercepted and short-circuited.");
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Logger.Error($"[ViewPipeline] A runtime crash occurred in the opening pipeline for view [{view.GetType().Name}], aborting the normal stage.\n{ex}");
                throw;
            }
            finally
            {
                contextCollection.Return(context);
                session.Release();
            }
        }

        public async UniTask CloseViewAsync(IView view, CancellationToken cancellationToken)
        {
            ThrowErrorIfDisposed();
            if (view == null) throw new ArgumentNullException(nameof(view));
            cancellationToken.ThrowIfCancellationRequested();
            if (ViewPipelineUtility.ShouldTerminate(key, view)) return;

            var context = contextCollection.Acquire();
            var session = PipelineSession.Acquire(key);
            session.Direction = PipelineDirection.Close;
            try
            {
                await closePipelineEngine.ExecuteAsync(view, context, session, cancellationToken);
                if (!session.IsTerminalReached)
                    Log.Logger.Info($"[ViewPipeline] Detected that the closing pipeline for view [{view.GetType().Name}] was intercepted and short-circuited, aborting the exit process.");
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Logger.Error($"[ViewPipeline] A runtime crash occurred in the closing pipeline for view [{view.GetType().Name}], aborting the exit process.\n{ex}");
                throw;
            }
            finally
            {
                contextCollection.Return(context);
                session.Release();
            }
        }

        public void RegisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfDisposed();
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            IPipelineEngine pipelineEngine;
            if (direction == PipelineDirection.Open) pipelineEngine = openPipelineEngine;
            else pipelineEngine = closePipelineEngine;
            pipelineEngine.RegisterDynamicProvider(provider);
        }

        public void UnregisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfDisposed();
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            IPipelineEngine pipelineEngine;
            if (direction == PipelineDirection.Open) pipelineEngine = openPipelineEngine;
            else pipelineEngine = closePipelineEngine;
            pipelineEngine.UnregisterDynamicProvider(provider);
        }

        public async UniTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;

            ViewSessionRegistry.Unregister(key);
            SnapshotCache.OnRefresh -= OnSnapshotRefresh;
            SnapshotCacheInternal.Remove<ViewSessionSnapshot>(key);

            while ((openPipelineEngine.ActiveOperations + closePipelineEngine.ActiveOperations) > 0)
            {
                await UniTask.Delay(10);
            }

            if(openPipelineEngine is IAsyncDisposable)
                await ((IAsyncDisposable)openPipelineEngine).DisposeAsync();
            if(closePipelineEngine is IAsyncDisposable)
                await ((IAsyncDisposable)closePipelineEngine).DisposeAsync();
            if(contextCollection is IAsyncDisposable)
                await ((IAsyncDisposable)contextCollection).DisposeAsync();
            await ExecutionPolicy.DisposeAsync(key);

            if(openPipelineEngine is IDisposable)
                ((IDisposable)openPipelineEngine).Dispose();
            if(closePipelineEngine is IDisposable)
                ((IDisposable)closePipelineEngine).Dispose();
            if(contextCollection is IDisposable)
                ((IDisposable)contextCollection).Dispose();

            extensions.Clear();
        }

        public ViewSessionSnapshot GetFullSnapshot()
        {
            ThrowErrorIfDisposed();
            return new ViewSessionSnapshot(
                key,
                extensions.Where(e => e is IFullSnapshotable<ExtensionSnapshot>).Select(e => ((IFullSnapshotable<ExtensionSnapshot>)e).GetFullSnapshot()).ToArray(),
                openPipelineEngine is IFullSnapshotable<ViewPipelineEngineSnapshot> typed1 ? typed1.GetFullSnapshot() : ViewPipelineEngineSnapshot.Empty,
                closePipelineEngine is IFullSnapshotable<ViewPipelineEngineSnapshot> typed2 ? typed2.GetFullSnapshot() : ViewPipelineEngineSnapshot.Empty,
                openPipelineEngine.ActiveOperations,
                closePipelineEngine.ActiveOperations
            );
        }

        private void OnSnapshotRefresh(Guid key, Type type)
        {
            if (this.key != key) return;
            if (type != null && type != typeof(ViewSessionSnapshot)) return;
            var snapshot = GetFullSnapshot();
            SnapshotCacheInternal.Store(key, snapshot);
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ViewSession), "[ViewPipeline] The view session has been disposed.");
        }
    }
}