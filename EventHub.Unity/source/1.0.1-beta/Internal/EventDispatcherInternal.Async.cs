#if !EVENTHUB_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventHub.Unity
{
    internal partial class EventDispatcherInternal : IAsyncSubscriberGetter, IAsyncEventDispatcher, IAsyncEventDispatcherExtension
    {
        private readonly ConcurrentDictionary<Type, IAsyncSubscriberCollection> asyncSubscribers = new ConcurrentDictionary<Type, IAsyncSubscriberCollection>();
        private readonly ConcurrentDictionary<Type, ReaderWriterLockSlim> asyncLocks = new ConcurrentDictionary<Type, ReaderWriterLockSlim>();

        public async UniTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            ThrowErrorIfDisposed();
            if (!EventDispatcherUtility.IsValidEvent(@event)) return;
            await PublishAsyncInternal(@event, cancellationToken);
        }

        public ISubscription Subscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            ThrowErrorIfDisposed();
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            return SubscribeInternal(handler, priority);
        }

        public int Unsubscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler)
        {
            ThrowErrorIfDisposed();
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return 0;
            return UnsubscribeInternal(handler);
        }

        public IEnumerable<AsyncSubscriber<TEvent>> GetAsyncSubscribers<TEvent>()
        {
            ThrowErrorIfDisposed();
            return GetAsyncSubscribersInternal<TEvent>();
        }

        int IAsyncEventDispatcherExtension.UnsubscribeAsyncEvents<TEvent>()
        {
            ThrowErrorIfDisposed();
            return UnsubscribeAsyncEventsInternal<TEvent>();
        }

        int IAsyncEventDispatcherExtension.UnsubscribeAll()
        {
            ThrowErrorIfDisposed();
            return UnsubscribeAllAsyncEventsInternal();
        }

        private async UniTask PublishAsyncInternal<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventType = typeof(TEvent);
            if (!asyncSubscribers.TryGetValue(eventType, out var rawCollection) 
                || !(rawCollection is IAsyncSubscriberCollection<TEvent> collection)) return;

            IEnumerable<AsyncSubscriber<TEvent>> snapshot = null;
            AsyncSubscriber<TEvent> oneSubscriber = default;
            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterReadLock();
            try
            {
                int count = collection.Count;
                if (count == 0) return;
                if (count == 1)
                {
                    if (collection is IIndexable<AsyncSubscriber<TEvent>> indexable)
                        oneSubscriber = indexable[0];
                    else
                        oneSubscriber = collection.FirstOrDefault();
                }

                if (count > 1)
                    snapshot = collection is ISnapshotable<AsyncSubscriber<TEvent>> typed ? typed.GetSnapshot() : collection.ToArray();
            }
            finally
            {
                key.ExitReadLock();
            }

            if (oneSubscriber.IsValid)
            {
                await oneSubscriber.SafeInvoke(@event, cancellationToken);
                return;
            }

            switch (snapshot)
            {
                case IIndexable<AsyncSubscriber<TEvent>> indexableSnapshot:
                    for (int i = 0; i < indexableSnapshot.Count; i++)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        var subscriber = indexableSnapshot[i];
                        await subscriber.SafeInvoke(@event, cancellationToken);
                    }
                    return;
                default:
                    foreach (var subscriber in snapshot)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        await subscriber.SafeInvoke(@event, cancellationToken);
                    }
                    return;
            }
        }

        private ISubscription SubscribeInternal<TEvent>(Func<TEvent, CancellationToken, UniTask> originalHandler, int priority)
        {
            var eventType = typeof(TEvent);
            var subscriber = new AsyncSubscriber<TEvent>(originalHandler, priority);

            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterWriteLock();
            try
            {
                var collection = (IAsyncSubscriberCollection<TEvent>)asyncSubscribers.GetOrAdd(eventType, CreateAsyncSubscriberCollection<TEvent>);
                EventDispatcherUtility.InsertSorted(collection, subscriber);
            }
            finally
            {
                key.ExitWriteLock();
            }

            return new Subscription(eventType, priority, () => UnsubscribeInternal(originalHandler));
        }

        private int UnsubscribeInternal<TEvent>(Func<TEvent, CancellationToken, UniTask> originalHandler)
        {
            var eventType = typeof(TEvent);
            if (!asyncSubscribers.TryGetValue(eventType, out var rawCollection)
                || !(rawCollection is IAsyncSubscriberCollection<TEvent> collection)) return 0;

            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
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

        private IEnumerable<AsyncSubscriber<TEvent>> GetAsyncSubscribersInternal<TEvent>()
        {
            var eventType = typeof(TEvent);
            if (!asyncSubscribers.TryGetValue(eventType, out var rawCollection)
                || !(rawCollection is IAsyncSubscriberCollection<TEvent> collection)) return Array.Empty<AsyncSubscriber<TEvent>>();

            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterReadLock();
            try
            {
                int count = collection.Count;
                if (count == 0) return Array.Empty<AsyncSubscriber<TEvent>>();

                return collection is ISnapshotable<AsyncSubscriber<TEvent>> typed ? typed.GetSnapshot() : collection.ToArray();
            }
            finally
            {
                key.ExitReadLock();
            }
        }

        private int UnsubscribeAsyncEventsInternal<TEvent>()
        {
            var eventType = typeof(TEvent);
            if (!asyncSubscribers.TryGetValue(eventType, out var rawCollection) 
                || !(rawCollection is IAsyncSubscriberCollection<TEvent> collection)) return 0;

            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
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

        private int UnsubscribeAllAsyncEventsInternal()
        {
            foreach (var item in asyncLocks.Values)
                item.EnterWriteLock();

            try
            {
                int count = 0;
                foreach (var collection in asyncSubscribers.Values)
                    count += collection.Count;
                asyncSubscribers.Clear();
                return count;
            }
            finally
            {
                foreach (var item in asyncLocks.Values)
                    item.ExitWriteLock();
            }
        }
    }
}

#endif