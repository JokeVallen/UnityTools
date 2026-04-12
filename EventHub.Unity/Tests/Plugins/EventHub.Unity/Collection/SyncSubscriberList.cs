#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal sealed class SyncSubscriberList : ISyncSubscriberCollection, IEnumerable<SyncSubscriber>, IReadOnlyCollection<SyncSubscriber>, ISnapshotable<SyncSubscriber>, IIndexable<SyncSubscriber>, ICountable
    {
        int IReadOnlyCollection<SyncSubscriber>.Count => subscribers.Count;
        int ICountable.Count => subscribers.Count;

        SyncSubscriber IIndexable<SyncSubscriber>.this[int index] => subscribers[index]; 

        private readonly List<SyncSubscriber> subscribers;
        private readonly Lazy<ISnapshotCamera<SyncSubscriber>> snapshotCamera;

        public SyncSubscriberList()
        {
            subscribers = new List<SyncSubscriber>();
            snapshotCamera = new Lazy<ISnapshotCamera<SyncSubscriber>>(CreateSnapshotCamera);
        }

        public SyncSubscriberList(IEnumerable<SyncSubscriber> collection)
        {
            subscribers = new List<SyncSubscriber>(collection);
            snapshotCamera = new Lazy<ISnapshotCamera<SyncSubscriber>>(CreateSnapshotCamera);
        }

        public SyncSubscriberList(int capacity)
        {
            subscribers = new List<SyncSubscriber>(capacity);
            snapshotCamera = new Lazy<ISnapshotCamera<SyncSubscriber>>(CreateSnapshotCamera);
        }

        void ISubscriberCollection<SyncSubscriber>.Add(SyncSubscriber subscriber)
        {
            subscribers.Add(subscriber);
            snapshotCamera.Value.NotifyModified(1);
        }

        IEnumerator<SyncSubscriber> IEnumerable<SyncSubscriber>.GetEnumerator()
        {
            return subscribers.GetEnumerator();
        }

        void ISubscriberCollection<SyncSubscriber>.Insert(int index, SyncSubscriber item)
        {
            subscribers.Insert(index, item);
            snapshotCamera.Value.NotifyModified(1);
        }

        void ISubscriberCollection<SyncSubscriber>.Remove(SyncSubscriber subscriber)
        {
            if(subscribers.Remove(subscriber)) snapshotCamera.Value.NotifyModified(1);
        }

        int ISubscriberCollection<SyncSubscriber>.RemoveAll(Predicate<SyncSubscriber> predicate)
        {
            int count = subscribers.RemoveAll(predicate);
            if(count > 0) snapshotCamera.Value.NotifyModified(count);
            return count;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<SyncSubscriber>)this).GetEnumerator();
        }

        IEnumerable<SyncSubscriber> ISnapshotable<SyncSubscriber>.GetSnapshot()
        {
            return snapshotCamera.Value.TakeSnapshot();
        }

        private ISnapshotCamera<SyncSubscriber> CreateSnapshotCamera()
        {
            return CommonFactory.CreateSnapShotCamera<ReadOnlySubscriberCollection<SyncSubscriberList, SyncSubscriber>, SyncSubscriber>(CreateSnapshot);
        }

        private ReadOnlySubscriberCollection<SyncSubscriberList, SyncSubscriber> CreateSnapshot()
        {
            return CommonFactory.CreateReadOnlySubscriberCollection<SyncSubscriberList, SyncSubscriber>(new SyncSubscriberList(subscribers));
        }
    }
}

#endif