#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
	[Preserve]
    internal class DefaultSnapshotCamera<TSnapshot, TElement> : IManualSnapshotCamera<TSnapshot, TElement> , ISnapshotCamera<TElement> 
    where TSnapshot : class, IReadOnlyCollection<TElement>
    {
        public TSnapshot Snapshot => snapshot;
        public int DirtyCount => dirtyCount;

        private readonly Func<TSnapshot> snapshotGetter;
        private volatile TSnapshot snapshot;
        private volatile int dirtyCount;
        private readonly object key = new object();

        public DefaultSnapshotCamera(Func<TSnapshot> snapshotGetter)
        {
            this.snapshotGetter = snapshotGetter ?? throw new ArgumentNullException(nameof(snapshotGetter));
            snapshot = snapshotGetter();
        }

        public IReadOnlyCollection<TElement> TakeSnapshot()
        {
            Flush();
            return snapshot;
        }

        public void NotifyModified(int count)
        {
            Interlocked.Add(ref dirtyCount, count);
        }

        public void Flush() 
        {
            if (dirtyCount > 0)
            {
                lock (key)
                {
                    if (dirtyCount > 0)
                        FlushInternal();
                }
            }
        }

        private void FlushInternal()
        {
            int oldDirty = Interlocked.Exchange(ref dirtyCount, 0);
            if (oldDirty == 0) return;
            snapshot = snapshotGetter();
        }
    }
}

#endif