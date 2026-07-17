#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface IIndexable<T>
    {
        int Count { get; }
        T this[int index] { get; }
    }
}

#endif