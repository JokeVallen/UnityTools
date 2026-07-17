#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
	[Preserve]
    internal interface IManualSnapshotCamera<TSnapshot, TElement> : ISnapshotCamera<TElement>
    {
        TSnapshot Snapshot { get; }
        int DirtyCount { get; }
        void Flush();
    }
}

#endif