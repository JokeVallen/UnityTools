#if !EVENTHUB_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal readonly struct AsyncSubscriber : ISubscriber
    {
        public readonly Func<object, CancellationToken, UniTask> Handler;
        public readonly Delegate OriginalHandler;
        public int Priority { get; }
        public bool IsValid => Handler != null && OriginalHandler != null;

        public AsyncSubscriber(Func<object, CancellationToken, UniTask> handler, Delegate originalHandler, int priority)
        {
            Handler = handler;
            OriginalHandler = originalHandler;
            Priority = priority;
        }

        public UniTask Invoke<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            return Handler(@event, cancellationToken);
        }

        public async UniTask SafeInvoke<TEvent>(TEvent @event, CancellationToken cancellationToken = default, Action<Exception> onError = null)
        {
            try
            {
                await Handler(@event, cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                EventDispatcherUtility.CatchError(typeof(TEvent), OriginalHandler, ex);
            }
        }
    }
}

#endif