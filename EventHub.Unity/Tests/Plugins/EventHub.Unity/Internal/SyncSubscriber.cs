#if !EVENTHUB_EXTENSION_ENABLE

using System;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal readonly struct SyncSubscriber : ISubscriber
    {
        public readonly Action<object> Handler;
        public readonly Delegate OriginalHandler;
        public int Priority { get; }
        public bool IsValid => Handler != null && OriginalHandler != null;

        public SyncSubscriber(Action<object> handler, Delegate originalHandler, int priority)
        {
            Handler = handler;
            OriginalHandler = originalHandler;
            Priority = priority;
        }

        public void Invoke<TEvent>(TEvent @event)
        {
            Handler(@event);
        }

        public void SafeInvoke<TEvent>(TEvent @event)
        {
            try
            {
                Handler(@event);
            }
            catch (Exception ex)
            {
                EventDispatcherUtility.CatchError(typeof(TEvent), OriginalHandler, ex);
            }
        }

        public void SafeInvoke<TEvent>(TEvent @event, out Exception exception)
        {
            exception = null;
            try
            {
                Handler(@event);
            }
            catch (Exception ex)
            {
                exception = ex;
                EventDispatcherUtility.CatchError(typeof(TEvent), OriginalHandler, ex);
            }
        }

        public void SafeInvoke<TEvent>(TEvent @event, Action<Exception> onError)
        {
            try
            {
                Handler(@event);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                EventDispatcherUtility.CatchError(typeof(TEvent), OriginalHandler, ex);
            }
        }
    }
}

#endif