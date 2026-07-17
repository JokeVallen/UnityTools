#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE && !EVENTHUB_DISABLE_UNITY_INNER_EXTENSION

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace EventHub.Unity
{
    internal static partial class Extension
    {
        public static ISubscription SubscribeOnMainThread<TEvent>(this IAsyncEventDispatcher dispatcher, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            return dispatcher.Subscribe<TEvent>(WrappedHandler, priority);

            async UniTask WrappedHandler(TEvent e, CancellationToken ct)
            {
                try
                {
                    await UniTask.SwitchToMainThread(ct);
                    await EventDispatcherUtility.SafeInvoke(handler, e, ct);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    EventDispatcherUtility.CatchError($"Failed to switch to main thread: {ex.Message}");
                }
            }
        }

        public static ISubscription SubscribeOnceOnMainThread<TEvent>(this IAsyncEventDispatcher dispatcher, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            int executed = 0;
            return dispatcher.Subscribe<TEvent>(OnceHandler, priority);

            async UniTask OnceHandler(TEvent e, CancellationToken ct)
            {
                if (Interlocked.CompareExchange(ref executed, 1, 0) != 0) return;
                try
                {
                    await UniTask.SwitchToMainThread(ct);
                    await EventDispatcherUtility.SafeInvoke(handler, e, cancellationToken: ct);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    EventDispatcherUtility.CatchError($"Failed to switch to main thread: {ex.Message}");
                }
                finally
                {
                    dispatcher.Unsubscribe<TEvent>(OnceHandler);
                }
            }
        }

        public static ISubscription SubscribeIfOnMainThread<TEvent>(this IAsyncEventDispatcher dispatcher, Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
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
                    try 
                    {
                        await UniTask.SwitchToMainThread(ct);
                        await EventDispatcherUtility.SafeInvoke(handler, e, cancellationToken: ct);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        EventDispatcherUtility.CatchError($"Failed to switch to main thread: {ex.Message}");
                    }
                }
            }
        }
    }
}

#endif