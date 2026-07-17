#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface ISubscriber
    {
        int Priority { get; }
    }
}

#endif