#if !EVENTHUB_EXTENSION_ENABLE

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
	[Preserve]
    internal interface ICleanableAsync
    {
        UniTask<int> TryCleanupUnusedLocksAsync(CancellationToken cancellationToken = default);
        UniTask<int> TryCleanupUnusedCollectionsAsync(CancellationToken cancellationToken = default);
        UniTask<int> TryCleanupUnusedLocksAndCollections(CancellationToken cancellationToken = default);
    }
}

#endif