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

        private static ISyncSubscriberCollection CreateSyncSubscriberCollection(Type type)
        {
            return CommonFactory.CreateSyncSubscriberCollection();
        }

        private static IAsyncSubscriberCollection CreateAsyncSubscriberCollection(Type type)
        {
            return CommonFactory.CreateAsyncSubscriberCollection();
        }
    }
}

#endif