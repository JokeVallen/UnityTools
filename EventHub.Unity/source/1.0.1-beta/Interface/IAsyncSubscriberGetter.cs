#if !EVENTHUB_EXTENSION_ENABLE

using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface IAsyncSubscriberGetter
    {
        IEnumerable<AsyncSubscriber<TEvent>> GetAsyncSubscribers<TEvent>();
    }
}

#endif