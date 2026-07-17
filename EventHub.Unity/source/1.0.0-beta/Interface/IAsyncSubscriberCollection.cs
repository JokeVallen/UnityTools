#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface IAsyncSubscriberCollection : ISubscriberCollection<AsyncSubscriber>
    {
        
    }
}

#endif