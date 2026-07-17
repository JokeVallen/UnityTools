#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface ISyncSubscriberCollection<TEvent> : ISyncSubscriberCollection, ISubscriberCollection<SyncSubscriber<TEvent>>
    {

    }
}

#endif