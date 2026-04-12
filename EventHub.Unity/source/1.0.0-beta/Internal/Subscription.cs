#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal class Subscription : ISubscription
    {
        public bool IsDisposed => disposed == 1;
        public Type EventType { get; }
        public int Priority { get; }

        private readonly Action unsubscribeAction;
        private int disposed; // 0 表示未释放，1 表示已释放

        public Subscription(Type eventType, int priority, Action unsubscribeAction)
        {
            EventType = eventType;
            Priority = priority;
            this.unsubscribeAction = unsubscribeAction;
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Unsubscribe()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
            {
                unsubscribeAction?.Invoke();
            }
        }
    }
}

#endif