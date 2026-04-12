#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface IIndexable<T> : ICountable
    {
        T this[int index] { get; }
    }
}

#endif