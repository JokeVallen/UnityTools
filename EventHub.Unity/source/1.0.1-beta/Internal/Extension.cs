#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_DISABLE_INNER_EXTENSION

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal static partial class Extension
    {
        public static ISubscription SubscribeOnce<TEvent>(this IAsyncEventDispatcher dispatcher, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            int executed = 0;
            return dispatcher.Subscribe<TEvent>(OnceHandler, priority);

            async UniTask OnceHandler(TEvent e, CancellationToken ct)
            {
                if (Interlocked.CompareExchange(ref executed, 1, 0) != 0) return;
                try
                {
                    await EventDispatcherUtility.SafeInvoke(handler, e, cancellationToken: ct);
                }
                finally
                {
                    dispatcher.Unsubscribe<TEvent>(OnceHandler);
                }
            }
        }

        public static ISubscription SubscribeOnce<TEvent>(this ISyncEventDispatcher dispatcher, Action<TEvent> handler, int priority = 0)
        {
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            int executed = 0;
            return dispatcher.Subscribe<TEvent>(OnceHandler, priority);

            void OnceHandler(TEvent e)
            {
                if (Interlocked.CompareExchange(ref executed, 1, 0) != 0) return;
                try
                {
                    EventDispatcherUtility.SafeInvoke(handler, e);
                }
                finally
                {
                    dispatcher.Unsubscribe<TEvent>(OnceHandler);
                }
            }
        }

        public static ISubscription SubscribeIf<TEvent>(this IAsyncEventDispatcher dispatcher, Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            if (!IsValidFilter(filter)) return null;
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            return dispatcher.Subscribe<TEvent>(IfHandler, priority);

            async UniTask IfHandler(TEvent e, CancellationToken ct)
            {
                bool shouldInvoke = false;
                try
                {
                    shouldInvoke = filter(e);
                }
                catch (Exception ex)
                {
                    EventDispatcherUtility.CatchError(typeof(TEvent), filter, ex);
                    return;
                }

                if (shouldInvoke)
                {
                    await EventDispatcherUtility.SafeInvoke(handler, e, cancellationToken: ct);
                }
            }
        }

        public static ISubscription SubscribeIf<TEvent>(this ISyncEventDispatcher dispatcher, Predicate<TEvent> filter, Action<TEvent> handler, int priority = 0)
        {
            if (!IsValidFilter(filter)) return null;
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            return dispatcher.Subscribe<TEvent>(IfHandler, priority);

            void IfHandler(TEvent e)
            {
                bool shouldInvoke = false;
                try
                {
                    shouldInvoke = filter(e);
                }
                catch (Exception ex)
                {
                    EventDispatcherUtility.CatchError(typeof(TEvent), filter, ex);
                    return;
                }

                if (shouldInvoke)
                {
                    EventDispatcherUtility.SafeInvoke(handler, e);
                }
            }
        }

        public static void PublishInterruptableEvents<TEvent>(this ISyncEventDispatcher dispatcher, TEvent @event) where TEvent : IInterruptableEvent
        {
            if (!(dispatcher is ISyncSubscriberGetter getter))
            {
                EventDispatcherLog.LogWarning($"The event dispatcher for event type '{nameof(TEvent)}' does not implement the interface '{nameof(ISyncSubscriberGetter)}'.");
                return;
            }

            // 获取订阅者快照
            var subscribers = getter.GetSyncSubscribers<TEvent>();

            switch (subscribers)
            {
                case IIndexable<SyncSubscriber<TEvent>> indexable:
                    for (int i = 0; i < indexable.Count; i++)
                    {
                        if (@event.Interrupted) break;
                        var subscriber = indexable[i];
                        subscriber.SafeInvoke(@event);
                    }
                    return;
                default:
                    foreach (var subscriber in subscribers)
                    {
                        if (@event.Interrupted) break;
                        subscriber.SafeInvoke(@event);
                    }
                    return;
            }
        }

        public static void PublishCancelableEvents<TEvent>(this ISyncEventDispatcher dispatcher, TEvent @event) where TEvent : ICancelableEvent 
        {
            if (!(dispatcher is ISyncSubscriberGetter getter))
            {
                EventDispatcherLog.LogWarning($"The event dispatcher for event type '{nameof(TEvent)}' does not implement the interface '{nameof(ISyncSubscriberGetter)}'.");
                return;
            }

            var subscribers = getter.GetSyncSubscribers<TEvent>();

            switch (subscribers)
            {
                case IIndexable<SyncSubscriber<TEvent>> indexable:
                    for (int i = 0; i < indexable.Count; i++)
                    {
                        if (@event.Cancelled) continue;
                        var subscriber = indexable[i];
                        subscriber.SafeInvoke(@event);
                    }
                    return;
                default:
                    foreach (var subscriber in subscribers)
                    {
                        if (@event.Cancelled) continue;
                        subscriber.SafeInvoke(@event);
                    }
                    return;
            }
        }

        private static bool IsValidFilter<TEvent>(Predicate<TEvent> filter)
        {
            if (filter == null)
            {
                EventDispatcherLog.LogWarning($"The filter for event type '{typeof(TEvent).Name}' cannot be null.");
                return false;
            }
            return true;
        }
    }
}

#endif