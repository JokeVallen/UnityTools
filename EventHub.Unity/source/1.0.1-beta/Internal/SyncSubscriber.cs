#if !EVENTHUB_EXTENSION_ENABLE

using System;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal readonly struct SyncSubscriber<TEvent> : ISubscriber
    {
        public readonly Action<TEvent> Handler;
        public int Priority { get; }
        public bool IsValid => Handler != null;

        public SyncSubscriber(Action<TEvent> handler, int priority)
        {
            Handler = handler;
            Priority = priority;
        }

        public void Invoke(TEvent @event)
        {
            Handler(@event);
        }

        public void SafeInvoke(TEvent @event)
        {
            try
            {
                Handler(@event);
            }
            catch (Exception ex)
            {
                EventDispatcherUtility.CatchError(typeof(TEvent), Handler, ex);
            }
        }

        public void SafeInvoke(TEvent @event, out Exception exception)
        {
            exception = null;
            try
            {
                Handler(@event);
            }
            catch (Exception ex)
            {
                exception = ex;
                EventDispatcherUtility.CatchError(typeof(TEvent), Handler, ex);
            }
        }

        public void SafeInvoke(TEvent @event, Action<Exception> onError)
        {
            try
            {
                Handler(@event);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                EventDispatcherUtility.CatchError(typeof(TEvent), Handler, ex);
            }
        }
    }
}

#endif