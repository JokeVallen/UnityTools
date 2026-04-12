#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
	[Preserve]
    internal interface ICleanable
    {
        int TryCleanupUnusedLocks();
        int TryCleanupUnusedCollections();
        int TryCleanupUnusedLocksAndCollections();
    }
}

#endif