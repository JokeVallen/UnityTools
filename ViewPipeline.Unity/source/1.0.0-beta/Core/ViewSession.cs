using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图会话
    /// </summary>
    internal sealed class ViewSession : IExtendedViewSession
    {
        /// <inheritdoc/>
        public Guid Key
        {
            get 
            {
                ThrowErrorIfDisposed();
                return key;
            }
        }

        private readonly IViewRegistry registry;
        private readonly IViewStackPolicy stackPolicy;
        private readonly IPipelineEngineInternal openPipelineEngine;
        private readonly IPipelineEngineInternal closePipelineEngine;
        private readonly IPipelineContextCollection contextCollection;
        private readonly Guid key;
        private int activeOperations;
        private bool disposed;

        public ViewSession(Guid key, IViewRegistry registry, IViewStackPolicy stackPolicy, IPipelineEngineInternal openPipelineEngine, IPipelineEngineInternal closePipelineEngine, IPipelineContextCollection contextCollection)
        {
            this.key = key;
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.stackPolicy = stackPolicy ?? throw new ArgumentNullException(nameof(stackPolicy));
            this.openPipelineEngine = openPipelineEngine ?? throw new ArgumentNullException(nameof(openPipelineEngine));
            this.closePipelineEngine = closePipelineEngine;
            this.contextCollection = contextCollection ?? throw new ArgumentNullException(nameof(contextCollection));
        }

        /// <inheritdoc/>
        public async UniTask OpenViewAsync(IView view, CancellationToken cancellationToken)
        {
            ThrowErrorIfDisposed();
            if (view == null) throw new ArgumentNullException(nameof(view));
            cancellationToken.ThrowIfCancellationRequested();

            activeOperations++;
            registry.Register(view);
            stackPolicy.Push(view);

            var context = contextCollection.Acquire();
            var session = PipelineSession.Acquire();
            session.Direction = PipelineDirection.Open;
            try
            {
                await openPipelineEngine.ExecuteAsync(view, context, session, cancellationToken);
                if (!session.IsTerminalReached)
                {
                    Log.Logger.Info($"[ViewPipeline] Detected that the activation pipeline for view [{view.GetType().Name}] was intercepted and short-circuited, triggering a graceful state rollback.");
                    RollbackState(view);
                }
            }
            catch (Exception)
            {
                RollbackState(view);
                throw;
            }
            finally
            {
                contextCollection.Return(context);
                session.Release();
                activeOperations--;
            }
        }

        /// <inheritdoc/>
        public async UniTask CloseViewAsync(IView view, CancellationToken cancellationToken)
        {
            ThrowErrorIfDisposed();
            if (view == null) throw new ArgumentNullException(nameof(view));
            cancellationToken.ThrowIfCancellationRequested();

            var context = contextCollection.Acquire();
            var session = PipelineSession.Acquire();
            session.Direction = PipelineDirection.Close;
            try
            {
                await closePipelineEngine.ExecuteAsync(view, context, session, cancellationToken);
                if (session.IsTerminalReached)
                {
                    stackPolicy.Pop(view);
                    registry.Unregister(view);
                }
                else
                {
                    Log.Logger.Info($"[ViewPipeline] Detected that the closing pipeline for view [{view.GetType().Name}] was intercepted and short-circuited, aborting the exit process and maintaining the original stack state.");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Logger.Error($"[ViewPipeline] A runtime crash occurred in the closing pipeline for view [{view.GetType().Name}], aborting the stack state change.\n{ex}");
                throw;
            }
            finally
            {
                contextCollection.Return(context);
                session.Release();
            }
        }

        /// <inheritdoc/>
        public void RegisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfDisposed();
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            IPipelineEngine pipelineEngine;
            if (direction == PipelineDirection.Open) pipelineEngine = openPipelineEngine;
            else pipelineEngine = closePipelineEngine;
            pipelineEngine.RegisterDynamicProvider(provider);
        }

        /// <inheritdoc/>
        public void UnregisterDynamicProvider(PipelineDirection direction, IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfDisposed();
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            IPipelineEngine pipelineEngine;
            if (direction == PipelineDirection.Open) pipelineEngine = openPipelineEngine;
            else pipelineEngine = closePipelineEngine;
            pipelineEngine.UnregisterDynamicProvider(provider);
        }

        /// <inheritdoc/>
        public async UniTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;

            while (activeOperations > 0)
            {
                await UniTask.Delay(10);
            }

            if (registry is IAsyncDisposable asyncRegistry)
                await asyncRegistry.DisposeAsync();
            if (stackPolicy is IAsyncDisposable asyncStackPolicy)
                await asyncStackPolicy.DisposeAsync();
            if (openPipelineEngine is IAsyncDisposable asyncOpenPipelineEngine)
                await asyncOpenPipelineEngine.DisposeAsync();
            if (closePipelineEngine is IAsyncDisposable asyncClosePipelineEngine)
                await asyncClosePipelineEngine.DisposeAsync();
            if (contextCollection is IAsyncDisposable asyncContextCollection)
                await asyncContextCollection.DisposeAsync();
            await Validation.DisposeAsync(key);
            await ExecutionPolicy.DisposeAsync(key);

            (registry as IDisposable)?.Dispose();
            (stackPolicy as IDisposable)?.Dispose();
            (openPipelineEngine as IDisposable)?.Dispose();
            (closePipelineEngine as IDisposable)?.Dispose();
            (contextCollection as IDisposable)?.Dispose();
        }

        private void RollbackState(IView view)
        {
            stackPolicy.Pop(view);
            registry.Unregister(view);
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new InvalidOperationException("[ViewPipeline] The view session has been disposed.");
        }
    }
}