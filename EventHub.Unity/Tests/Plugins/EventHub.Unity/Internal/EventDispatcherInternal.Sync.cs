#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventHub.Unity
{
    internal partial class EventDispatcherInternal : ISyncSubscriberGetter, ISyncEventDispatcher
    {
        private readonly ConcurrentDictionary<Type, ISyncSubscriberCollection> syncSubscribers = new ConcurrentDictionary<Type, ISyncSubscriberCollection>();
        private readonly ConcurrentDictionary<Type, ReaderWriterLockSlim> syncLocks = new ConcurrentDictionary<Type, ReaderWriterLockSlim>();

        public void Publish<TEvent>(TEvent @event)
        {
            if (!EventDispatcherUtility.IsValidEvent(@event)) return;
            PublishInternal(@event);
        }

        public ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)
        {
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            return SubscribeInternal(handler, priority);
        }

        public int Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return 0;
            return UnsubscribeInternal(handler);
        }

        public IEnumerable<SyncSubscriber> GetSyncSubscribers<TEvent>()
        {
            return GetSyncSubscribersInternal<TEvent>();
        }

        private void PublishInternal<TEvent>(TEvent @event)
        {
            var eventType = typeof(TEvent);
            if (!syncSubscribers.TryGetValue(eventType, out var collection)) return;

            IEnumerable<SyncSubscriber> snapshot = null;
            SyncSubscriber oneSubscriber = default;
            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterReadLock();
            try
            {
                int count = -1;
                if (collection is ICountable countable)
                {
                    count = countable.Count;
                    if (count == 0) return;
                    if (count == 1)
                    {
                        if (collection is IIndexable<SyncSubscriber> indexable)
                            oneSubscriber = indexable[0];
                        else
                            oneSubscriber = collection.FirstOrDefault();
                    }
                }

                if (count > 1)
                    snapshot = collection is ISnapshotable<SyncSubscriber> typed ? typed.GetSnapshot() : collection.ToArray();
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
                    case IIndexable<SyncSubscriber> indexableSnapshot:
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
            var handler = new Action<object>(obj => originalHandler((TEvent)obj));
            var subscriber = new SyncSubscriber(handler, originalHandler, priority);

            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterWriteLock();
            try
            {
                var collection = syncSubscribers.GetOrAdd(eventType, CreateSyncSubscriberCollection);
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
            if (!syncSubscribers.TryGetValue(eventType, out var collection)) return 0;

            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterWriteLock();
            try
            {
                int count = -1;
                if (collection is ICountable countable)
                {
                    count = countable.Count;
                    if (count == 0) return 0;
                }

                var removed = collection.RemoveAll(s => s.OriginalHandler.Equals(originalHandler));
                return removed;
            }
            finally
            {
                key.ExitWriteLock();
            }
        }

        private IEnumerable<SyncSubscriber> GetSyncSubscribersInternal<TEvent>()
        {
            var eventType = typeof(TEvent);
            if (!syncSubscribers.TryGetValue(eventType, out var collection)) return Array.Empty<SyncSubscriber>();

            var key = syncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterReadLock();
            try
            {
                int count = -1;
                if (collection is ICountable countable)
                {
                    count = countable.Count;
                    if (count == 0) return Array.Empty<SyncSubscriber>();
                }

                return collection is ISnapshotable<SyncSubscriber> typed ? typed.GetSnapshot() : collection.ToArray();
            }
            finally
            {
                key.ExitReadLock();
            }
        }
    }
}

#endif