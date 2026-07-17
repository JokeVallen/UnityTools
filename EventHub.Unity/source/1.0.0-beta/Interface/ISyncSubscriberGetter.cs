#if !EVENTHUB_EXTENSION_ENABLE

using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface ISyncSubscriberGetter
    {
        IEnumerable<SyncSubscriber> GetSyncSubscribers<TEvent>();
    }
}

#endif