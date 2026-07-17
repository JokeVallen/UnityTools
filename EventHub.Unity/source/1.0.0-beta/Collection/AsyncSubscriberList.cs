#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal sealed class AsyncSubscriberList : IAsyncSubscriberCollection, IEnumerable<AsyncSubscriber>, ISnapshotable<AsyncSubscriber>, IReadOnlyCollection<AsyncSubscriber>, IIndexable<AsyncSubscriber>, ICountable
    {
        int IReadOnlyCollection<AsyncSubscriber>.Count => subscribers.Count;
        int ICountable.Count => subscribers.Count;

        AsyncSubscriber IIndexable<AsyncSubscriber>.this[int index] => subscribers[index]; 

        private readonly List<AsyncSubscriber> subscribers;
        private readonly Lazy<ISnapshotCamera<AsyncSubscriber>> snapshotCamera;

        public AsyncSubscriberList()
        {
            subscribers = new List<AsyncSubscriber>();
            snapshotCamera = new Lazy<ISnapshotCamera<AsyncSubscriber>>(CreateSnapshotCamera);
        }

        public AsyncSubscriberList(IEnumerable<AsyncSubscriber> collection)
        {
            subscribers = new List<AsyncSubscriber>(collection);
            snapshotCamera = new Lazy<ISnapshotCamera<AsyncSubscriber>>(CreateSnapshotCamera);
        }

        public AsyncSubscriberList(int capacity)
        {
            subscribers = new List<AsyncSubscriber>(capacity);
            snapshotCamera = new Lazy<ISnapshotCamera<AsyncSubscriber>>(CreateSnapshotCamera);
        }

        void ISubscriberCollection<AsyncSubscriber>.Add(AsyncSubscriber subscriber)
        {
            subscribers.Add(subscriber);
            snapshotCamera.Value.NotifyModified(1);
        }

        IEnumerator<AsyncSubscriber> IEnumerable<AsyncSubscriber>.GetEnumerator()
        {
            return subscribers.GetEnumerator();
        }

        void ISubscriberCollection<AsyncSubscriber>.Insert(int index, AsyncSubscriber item)
        {
            subscribers.Insert(index, item);
            snapshotCamera.Value.NotifyModified(1);
        }

        void ISubscriberCollection<AsyncSubscriber>.Remove(AsyncSubscriber subscriber)
        {
            if (subscribers.Remove(subscriber)) snapshotCamera.Value.NotifyModified(1);
        }

        int ISubscriberCollection<AsyncSubscriber>.RemoveAll(Predicate<AsyncSubscriber> predicate)
        {
            int count = subscribers.RemoveAll(predicate);
            if (count > 0) snapshotCamera.Value.NotifyModified(count);
            return count;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<AsyncSubscriber>)this).GetEnumerator();
        }

        IEnumerable<AsyncSubscriber> ISnapshotable<AsyncSubscriber>.GetSnapshot()
        {
            return snapshotCamera.Value.TakeSnapshot();
        }

        private ISnapshotCamera<AsyncSubscriber> CreateSnapshotCamera() 
        {
            return CommonFactory.CreateSnapShotCamera<ReadOnlySubscriberCollection<AsyncSubscriberList, AsyncSubscriber>, AsyncSubscriber>(CreateSnapshot);
        }

        private ReadOnlySubscriberCollection<AsyncSubscriberList, AsyncSubscriber> CreateSnapshot() 
        {
            return CommonFactory.CreateReadOnlySubscriberCollection<AsyncSubscriberList, AsyncSubscriber>(new AsyncSubscriberList(subscribers));
        }
    }
}

#endif