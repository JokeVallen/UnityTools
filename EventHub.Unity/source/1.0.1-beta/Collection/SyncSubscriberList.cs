#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal sealed class SyncSubscriberList<TEvent> : ISyncSubscriberCollection<TEvent>, IEnumerable<SyncSubscriber<TEvent>>, IReadOnlyCollection<SyncSubscriber<TEvent>>, ISnapshotable<SyncSubscriber<TEvent>>, IIndexable<SyncSubscriber<TEvent>>
    {
        int IReadOnlyCollection<SyncSubscriber<TEvent>>.Count => subscribers.Count;
        int ISubscriberCollection.Count => subscribers.Count;
        int IIndexable<SyncSubscriber<TEvent>>.Count => subscribers.Count;

        SyncSubscriber<TEvent> IIndexable<SyncSubscriber<TEvent>>.this[int index] => subscribers[index];

        private readonly List<SyncSubscriber<TEvent>> subscribers;
        private readonly Lazy<ISnapshotCamera<SyncSubscriber<TEvent>>> snapshotCamera;

        public SyncSubscriberList()
        {
            subscribers = new List<SyncSubscriber<TEvent>>();
            snapshotCamera = new Lazy<ISnapshotCamera<SyncSubscriber<TEvent>>>(CreateSnapshotCamera);
        }

        public SyncSubscriberList(IEnumerable<SyncSubscriber<TEvent>> collection)
        {
            subscribers = new List<SyncSubscriber<TEvent>>(collection);
            snapshotCamera = new Lazy<ISnapshotCamera<SyncSubscriber<TEvent>>>(CreateSnapshotCamera);
        }

        public SyncSubscriberList(int capacity)
        {
            subscribers = new List<SyncSubscriber<TEvent>>(capacity);
            snapshotCamera = new Lazy<ISnapshotCamera<SyncSubscriber<TEvent>>>(CreateSnapshotCamera);
        }

        void ISubscriberCollection<SyncSubscriber<TEvent>>.Add(SyncSubscriber<TEvent> subscriber)
        {
            subscribers.Add(subscriber);
            snapshotCamera.Value.NotifyModified(1);
        }

        IEnumerator<SyncSubscriber<TEvent>> IEnumerable<SyncSubscriber<TEvent>>.GetEnumerator()
        {
            return subscribers.GetEnumerator();
        }

        void ISubscriberCollection<SyncSubscriber<TEvent>>.Insert(int index, SyncSubscriber<TEvent> item)
        {
            subscribers.Insert(index, item);
            snapshotCamera.Value.NotifyModified(1);
        }

        void ISubscriberCollection<SyncSubscriber<TEvent>>.Remove(SyncSubscriber<TEvent> subscriber)
        {
            if (subscribers.Remove(subscriber)) snapshotCamera.Value.NotifyModified(1);
        }

        int ISubscriberCollection<SyncSubscriber<TEvent>>.RemoveAll(Predicate<SyncSubscriber<TEvent>> predicate)
        {
            int count = subscribers.RemoveAll(predicate);
            if (count > 0) snapshotCamera.Value.NotifyModified(count);
            return count;
        }

        int ISubscriberCollection<SyncSubscriber<TEvent>>.Clear()
        {
            int count = subscribers.Count;
            subscribers.Clear();
            return count;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<SyncSubscriber<TEvent>>)this).GetEnumerator();
        }

        IEnumerable<SyncSubscriber<TEvent>> ISnapshotable<SyncSubscriber<TEvent>>.GetSnapshot()
        {
            return snapshotCamera.Value.TakeSnapshot();
        }

        private ISnapshotCamera<SyncSubscriber<TEvent>> CreateSnapshotCamera()
        {
            return CommonFactory.CreateSnapShotCamera<ReadOnlySubscriberCollection<SyncSubscriberList<TEvent>, SyncSubscriber<TEvent>>, SyncSubscriber<TEvent>>(CreateSnapshot);
        }

        private ReadOnlySubscriberCollection<SyncSubscriberList<TEvent>, SyncSubscriber<TEvent>> CreateSnapshot()
        {
            return CommonFactory.CreateReadOnlySubscriberCollection<SyncSubscriberList<TEvent>, SyncSubscriber<TEvent>>(new SyncSubscriberList<TEvent>(subscribers));
        }
    }
}

#endif