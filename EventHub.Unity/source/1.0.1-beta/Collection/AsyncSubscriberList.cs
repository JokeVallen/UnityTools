#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal sealed class AsyncSubscriberList<TEvent> : IAsyncSubscriberCollection<TEvent>, IEnumerable<AsyncSubscriber<TEvent>>, ISnapshotable<AsyncSubscriber<TEvent>>, IReadOnlyCollection<AsyncSubscriber<TEvent>>, IIndexable<AsyncSubscriber<TEvent>>
    {
        int IReadOnlyCollection<AsyncSubscriber<TEvent>>.Count => subscribers.Count;
        int ISubscriberCollection.Count => subscribers.Count;
        int IIndexable<AsyncSubscriber<TEvent>>.Count => subscribers.Count;

        AsyncSubscriber<TEvent> IIndexable<AsyncSubscriber<TEvent>>.this[int index] => subscribers[index];

        private readonly List<AsyncSubscriber<TEvent>> subscribers;
        private readonly Lazy<ISnapshotCamera<AsyncSubscriber<TEvent>>> snapshotCamera;

        public AsyncSubscriberList()
        {
            subscribers = new List<AsyncSubscriber<TEvent>>();
            snapshotCamera = new Lazy<ISnapshotCamera<AsyncSubscriber<TEvent>>>(CreateSnapshotCamera);
        }

        public AsyncSubscriberList(IEnumerable<AsyncSubscriber<TEvent>> collection)
        {
            subscribers = new List<AsyncSubscriber<TEvent>>(collection);
            snapshotCamera = new Lazy<ISnapshotCamera<AsyncSubscriber<TEvent>>>(CreateSnapshotCamera);
        }

        public AsyncSubscriberList(int capacity)
        {
            subscribers = new List<AsyncSubscriber<TEvent>>(capacity);
            snapshotCamera = new Lazy<ISnapshotCamera<AsyncSubscriber<TEvent>>>(CreateSnapshotCamera);
        }

        void ISubscriberCollection<AsyncSubscriber<TEvent>>.Add(AsyncSubscriber<TEvent> subscriber)
        {
            subscribers.Add(subscriber);
            snapshotCamera.Value.NotifyModified(1);
        }

        IEnumerator<AsyncSubscriber<TEvent>> IEnumerable<AsyncSubscriber<TEvent>>.GetEnumerator()
        {
            return subscribers.GetEnumerator();
        }

        void ISubscriberCollection<AsyncSubscriber<TEvent>>.Insert(int index, AsyncSubscriber<TEvent> item)
        {
            subscribers.Insert(index, item);
            snapshotCamera.Value.NotifyModified(1);
        }

        void ISubscriberCollection<AsyncSubscriber<TEvent>>.Remove(AsyncSubscriber<TEvent> subscriber)
        {
            if (subscribers.Remove(subscriber)) snapshotCamera.Value.NotifyModified(1);
        }

        int ISubscriberCollection<AsyncSubscriber<TEvent>>.RemoveAll(Predicate<AsyncSubscriber<TEvent>> predicate)
        {
            int count = subscribers.RemoveAll(predicate);
            if (count > 0) snapshotCamera.Value.NotifyModified(count);
            return count;
        }

        int ISubscriberCollection<AsyncSubscriber<TEvent>>.Clear()
        {
            int count = subscribers.Count;
            subscribers.Clear();
            return count;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<AsyncSubscriber<TEvent>>)this).GetEnumerator();
        }

        IEnumerable<AsyncSubscriber<TEvent>> ISnapshotable<AsyncSubscriber<TEvent>>.GetSnapshot()
        {
            return snapshotCamera.Value.TakeSnapshot();
        }

        private ISnapshotCamera<AsyncSubscriber<TEvent>> CreateSnapshotCamera()
        {
            return CommonFactory.CreateSnapShotCamera<ReadOnlySubscriberCollection<AsyncSubscriberList<TEvent>, AsyncSubscriber<TEvent>>, AsyncSubscriber<TEvent>>(CreateSnapshot);
        }

        private ReadOnlySubscriberCollection<AsyncSubscriberList<TEvent>, AsyncSubscriber<TEvent>> CreateSnapshot()
        {
            return CommonFactory.CreateReadOnlySubscriberCollection<AsyncSubscriberList<TEvent>, AsyncSubscriber<TEvent>>(new AsyncSubscriberList<TEvent>(subscribers));
        }
    }
}

#endif