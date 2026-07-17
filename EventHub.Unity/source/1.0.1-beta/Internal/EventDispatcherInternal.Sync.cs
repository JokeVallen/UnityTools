#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventHub.Unity
{
    internal partial class EventDispatcherInternal : ISyncSubscriberGetter, ISyncEventDispatcher, ISyncEventDispatcherExtension
    {
        private readonly ConcurrentDictionary<Type, ISyncSubscriberCollection> syncSubscribers = new ConcurrentDictionary<Type, ISyncSubscriberCollection>();
        private readonly ConcurrentDictionary<Type, ReaderWriterLockSlim> syncLocks = new ConcurrentDictionary<Type, ReaderWriterLockSlim>();

        public void Publish<TEvent>(TEvent @event)
        {
            ThrowErrorIfDisposed();
            if (!EventDispatcherUtility.IsValidEvent(@event)) return;
            PublishInternal(@event);
        }

        public ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)
        {
            ThrowErrorIfDisposed();
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            return SubscribeInternal(handler, priority);
        }

        public int Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            ThrowErrorIfDisposed();
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return 0;
            return UnsubscribeInternal(handler);
        }

        public IEnumerable<SyncSubscriber<TEvent>> GetSyncSubscribers<TEvent>()
        {
            ThrowErrorIfDisposed();
            return GetSyncSubscribersInternal<TEvent>();
        }

        int ISyncEventDispatcherExtension.UnsubscribeSyncEvents<TEvent>()
        {
            ThrowErrorIfDisposed();
            return UnsubscribeSyncEventsInternal<TEvent>();
        }

        int ISyncEventDispatcherExtension.UnsubscribeAll()
        {
            ThrowErrorIfDisposed();
            return UnsubscribeAllSyncEventsInternal();
        }

        private void PublishInternal<TEvent>(TEvent @event)
        {
            var eventType = typeof(TEvent);
            if (!syncSubscribers.TryGetValue(eventType, out var rawCollection)
                || !(rawCollection is ISyncSubscriberCollection<TEvent> collection)) return;

            IEnumerable<SyncSubscriber<TEvent>> snapshot = null;
            SyncSubscriber<TEvent> oneSubscriber = default;
            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterReadLock();
            try
            {
                int count = collection.Count;
                if (count == 0) return;
                if (count == 1)
                {
                    if (collection is IIndexable<SyncSubscriber<TEvent>> indexable)
                        oneSubscriber = indexable[0];
                    else
                        oneSubscriber = collection.FirstOrDefault();
                }

                if (count > 1)
                    snapshot = collection is ISnapshotable<SyncSubscriber<TEvent>> typed ? typed.GetSnapshot() : collection.ToArray();
            }
            finally
            {
                key.ExitReadLock();
            }

            if (oneSubscriber.IsValid)
            {
                oneSubscriber.SafeInvoke(@event);
                return;
            }
            else 
            {
                switch (snapshot)
                {
                    case IIndexable<SyncSubscriber<TEvent>> indexableSnapshot:
                        for (int i = 0; i < indexableSnapshot.Count; i++)
                        {
                            var subscriber = indexableSnapshot[i];
                            subscriber.SafeInvoke(@event);
                        }
                        return;
                    default:
                        foreach (var subscriber in snapshot)
                        {
                            subscriber.SafeInvoke(@event);
                        }
                        return;
                }
            }
        }

        private ISubscription SubscribeInternal<TEvent>(Action<TEvent> originalHandler, int priority)
        {
            var eventType = typeof(TEvent);
            var subscriber = new SyncSubscriber<TEvent>(originalHandler, priority);

            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterWriteLock();
            try
            {
                var collection = (ISyncSubscriberCollection<TEvent>)syncSubscribers.GetOrAdd(eventType, CreateSyncSubscriberCollection<TEvent>);
                EventDispatcherUtility.InsertSorted(collection, subscriber);
            }
            finally
            {
                key.ExitWriteLock();
            }

            return new Subscription(eventType, priority, () => UnsubscribeInternal(originalHandler));
        }

        private int UnsubscribeInternal<TEvent>(Action<TEvent> originalHandler)
        {
            var eventType = typeof(TEvent);
            if (!syncSubscribers.TryGetValue(eventType, out var rawCollection)
                || !(rawCollection is ISyncSubscriberCollection<TEvent> collection)) return 0;

            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterWriteLock();
            try
            {
                int count = collection.Count;
                if (count == 0) return 0;

                var removed = collection.RemoveAll(s => s.Handler.Equals(originalHandler));
                return removed;
            }
            finally
            {
                key.ExitWriteLock();
            }
        }

        private IEnumerable<SyncSubscriber<TEvent>> GetSyncSubscribersInternal<TEvent>()
        {
            var eventType = typeof(TEvent);
            if (!syncSubscribers.TryGetValue(eventType, out var rawCollection)
                || !(rawCollection is ISyncSubscriberCollection<TEvent> collection)) return Array.Empty<SyncSubscriber<TEvent>>();

            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterReadLock();
            try
            {
                int count = collection.Count;
                if (count == 0) return Array.Empty<SyncSubscriber<TEvent>>();

                return collection is ISnapshotable<SyncSubscriber<TEvent>> typed ? typed.GetSnapshot() : collection.ToArray();
            }
            finally
            {
                key.ExitReadLock();
            }
        }

        private int UnsubscribeSyncEventsInternal<TEvent>()
        {
            var eventType = typeof(TEvent);
            if (!syncSubscribers.TryGetValue(eventType, out var rawCollection)
                || !(rawCollection is ISyncSubscriberCollection<TEvent> collection)) return 0;

            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterWriteLock();
            try 
            {
                int count = collection.Count;
                if (count == 0) return 0;

                var removed = collection.Clear();
                return removed;
            }
            finally
            {
                key.ExitWriteLock();
            }
        }

        private int UnsubscribeAllSyncEventsInternal()
        {
            foreach (var item in syncLocks.Values)
                item.EnterWriteLock();

            try
            {
                int count = 0;
                foreach (var collection in syncSubscribers.Values)
                    count += collection.Count;
                syncSubscribers.Clear();
                return count;
            }
            finally 
            {
                foreach (var item in syncLocks.Values)
                    item.ExitWriteLock();
            }
        }
    }
}

#endif