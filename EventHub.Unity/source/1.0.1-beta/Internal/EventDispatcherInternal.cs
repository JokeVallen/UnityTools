#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal partial class EventDispatcherInternal : IEventDispatcher
    {
        private static ReaderWriterLockSlim CreateReaderWriterLockSlim(Type type) 
        {
            return new ReaderWriterLockSlim();
        }

        private static ISyncSubscriberCollection<TEvent> CreateSyncSubscriberCollection<TEvent>(Type type)
        {
            return CommonFactory.CreateSyncSubscriberCollection<TEvent>();
        }

        private static IAsyncSubscriberCollection<TEvent> CreateAsyncSubscriberCollection<TEvent>(Type type)
        {
            return CommonFactory.CreateAsyncSubscriberCollection<TEvent>();
        }
    }
}

#endif