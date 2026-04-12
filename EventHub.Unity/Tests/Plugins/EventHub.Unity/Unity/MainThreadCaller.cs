#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace EventHub.Unity
{
    internal sealed class MainThreadCaller : IMainThreadCaller
    {
        async UniTask IMainThreadCaller.PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
            await UniTask.SwitchToMainThread();
            await EventDispatcher.GetAsyncEventDispatcher().PublishAsync(@event, cancellationToken);
        }

        async UniTask IMainThreadCaller.PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
            await UniTask.SwitchToMainThread();
            await EventDispatcher.GetParallelizable().PublishParallelAsync(@event, cancellationToken);
        }

        ISubscription IMainThreadCaller.SubscribeIf<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority)
        {
            return EventDispatcher.GetAsyncEventDispatcher().SubscribeIfOnMainThread(filter, handler, priority);
        }

        ISubscription IMainThreadCaller.SubscribeOnce<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority)
        {
            return EventDispatcher.GetAsyncEventDispatcher().SubscribeOnceOnMainThread(handler, priority);
        }
    }
}

#endif