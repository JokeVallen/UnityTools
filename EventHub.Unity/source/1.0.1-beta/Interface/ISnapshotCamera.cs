#if !EVENTHUB_EXTENSION_ENABLE

using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
	[Preserve]
    internal interface ISnapshotCamera<TElement>
    {
        IReadOnlyCollection<TElement> TakeSnapshot();
        void NotifyModified(int count);
    }
}

#endif