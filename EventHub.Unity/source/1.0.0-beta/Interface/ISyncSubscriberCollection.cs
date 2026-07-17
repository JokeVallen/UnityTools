#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface ISyncSubscriberCollection : ISubscriberCollection<SyncSubscriber>
    {
        
    }
}

#endif