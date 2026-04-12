#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface ISubscriberCollection<T> : IEnumerable<T>
    {
        void Add(T subscriber);
        void Remove(T subscriber);
        int RemoveAll(Predicate<T> predicate);
        void Insert(int index, T item);
    }
}

#endif