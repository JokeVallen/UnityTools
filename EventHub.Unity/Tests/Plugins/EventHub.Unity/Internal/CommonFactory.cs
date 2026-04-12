#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
	[Preserve]
    internal static class CommonFactory
    {
        public static ISyncSubscriberCollection CreateSyncSubscriberCollection()
        {
            return new SyncSubscriberList();
        }

        public static IAsyncSubscriberCollection CreateAsyncSubscriberCollection()
        {
            return new AsyncSubscriberList();
        }

        public static ReadOnlySubscriberCollection<T, TElement> CreateReadOnlySubscriberCollection<T, TElement>(T collection)
        where T : IReadOnlyCollection<TElement>
        {
            return new ReadOnlySubscriberCollection<T, TElement>(collection);
        }

        public static ISnapshotCamera<TElement> CreateSnapShotCamera<TSnapshot, TElement>(Func<TSnapshot> snapshotGetter)
        where TSnapshot : class, IReadOnlyCollection<TElement>
        {
            return new DefaultSnapshotCamera<TSnapshot, TElement>(snapshotGetter);
        }
    }
}

#endif