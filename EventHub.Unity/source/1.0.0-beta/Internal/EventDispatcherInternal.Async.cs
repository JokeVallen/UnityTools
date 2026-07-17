#if !EVENTHUB_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventHub.Unity
{
    internal partial class EventDispatcherInternal : IAsyncSubscriberGetter, IAsyncEventDispatcher
    {
        private readonly ConcurrentDictionary<Type, IAsyncSubscriberCollection> asyncSubscribers = new ConcurrentDictionary<Type, IAsyncSubscriberCollection>();
        private readonly ConcurrentDictionary<Type, ReaderWriterLockSlim> asyncLocks = new ConcurrentDictionary<Type, ReaderWriterLockSlim>();

        public async UniTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            if (!EventDispatcherUtility.IsValidEvent(@event)) return;
            await PublishAsyncInternal(@event, cancellationToken);
        }

        public ISubscription Subscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return null;
            return SubscribeInternal(handler, priority);
        }

        public int Unsubscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler)
        {
            if (!EventDispatcherUtility.IsValidHandler<TEvent>(handler)) return 0;
            return UnsubscribeInternal(handler);
        }

        public IEnumerable<AsyncSubscriber> GetAsyncSubscribers<TEvent>()
        {
            return GetAsyncSubscribersInternal<TEvent>();
        }

        private async UniTask PublishAsyncInternal<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventType = typeof(TEvent);
            if (!asyncSubscribers.TryGetValue(eventType, out var collection)) return;

            IEnumerable<AsyncSubscriber> snapshot = null;
            AsyncSubscriber oneSubscriber = default;
            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
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
                        if (collection is IIndexable<AsyncSubscriber> indexable)
                            oneSubscriber = indexable[0];
                        else
                            oneSubscriber = collection.FirstOrDefault();
                    }
                }

                if (count > 1)
                    snapshot = collection is ISnapshotable<AsyncSubscriber> typed ? typed.GetSnapshot() : collection.ToArray();
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
                case IIndexable<AsyncSubscriber> indexableSnapshot:
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
            Func<object, CancellationToken, UniTask> handler = (obj, ct) => originalHandler((TEvent)obj, ct);
            var subscriber = new AsyncSubscriber(handler, originalHandler, priority);

            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterWriteLock();
            try
            {
                var collection = asyncSubscribers.GetOrAdd(eventType, CreateAsyncSubscriberCollection);
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
            if (!asyncSubscribers.TryGetValue(eventType, out var collection)) return 0;

            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
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

        private IEnumerable<AsyncSubscriber> GetAsyncSubscribersInternal<TEvent>()
        {
            var eventType = typeof(TEvent);
            if (!asyncSubscribers.TryGetValue(eventType, out var collection)) return Array.Empty<AsyncSubscriber>();

            var key = asyncLocks.GetOrAdd(eventType, CreateReaderWriterLockSlim);
            key.EnterReadLock();
            try
            {
                int count = -1;
                if (collection is ICountable countable)
                {
                    count = countable.Count;
                    if (count == 0) return Array.Empty<AsyncSubscriber>();
                }

                return collection is ISnapshotable<AsyncSubscriber> typed ? typed.GetSnapshot() : collection.ToArray();
            }
            finally
            {
                key.ExitReadLock();
            }
        }
    }
}

#endif