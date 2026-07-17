#if !EVENTHUB_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal readonly struct AsyncSubscriber<TEvent> : ISubscriber
    {
        public readonly Func<TEvent, CancellationToken, UniTask> Handler;
        public int Priority { get; }
        public bool IsValid => Handler != null;

        public AsyncSubscriber(Func<TEvent, CancellationToken, UniTask> handler, int priority)
        {
            Handler = handler;
            Priority = priority;
        }

        public UniTask Invoke(TEvent @event, CancellationToken cancellationToken = default)
        {
            return Handler(@event, cancellationToken);
        }

        public async UniTask SafeInvoke(TEvent @event, CancellationToken cancellationToken = default, Action<Exception> onError = null)
        {
            try
            {
                await Handler(@event, cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                EventDispatcherUtility.CatchError(typeof(TEvent), Handler, ex);
            }
        }
    }
}

#endif