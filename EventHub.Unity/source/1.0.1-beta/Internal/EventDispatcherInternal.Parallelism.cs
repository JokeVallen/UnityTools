#if !EVENTHUB_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace EventHub.Unity
{
    internal partial class EventDispatcherInternal : IParallelizable
    {
        public async UniTask PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            ThrowErrorIfDisposed();
            if (!EventDispatcherUtility.IsValidEvent(@event)) return;
            await PublishParallelAsyncInternal(@event, cancellationToken);
        }

        private async UniTask PublishParallelAsyncInternal<TEvent>(TEvent @event, CancellationToken cancellationToken)
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
                await oneSubscriber.SafeInvoke(@event, cancellationToken, onError: e => throw new AggregateException(e));
            }
            else
            {
                ConcurrentBag<Exception> exceptions = null;
                void onError(Exception e)
                {
                    if (exceptions == null)
                        Interlocked.CompareExchange(ref exceptions, new ConcurrentBag<Exception>(), null);
                    exceptions?.Add(e);
                }

                switch (snapshot)
                {
                    case IIndexable<AsyncSubscriber<TEvent>> indexable:
                        var tasks1 = new UniTask[indexable.Count];
                        for (int i = 0; i < indexable.Count; i++)
                        {
                            var subscriber = indexable[i];
                            tasks1[i] = subscriber.SafeInvoke(@event, cancellationToken, onError: onError);
                        }
                        await UniTask.WhenAll(tasks1);
                        break;
                    default:
                        var tasks = snapshot.Select(subscriber => subscriber.SafeInvoke(@event, cancellationToken, onError: onError));
                        await UniTask.WhenAll(tasks);
                        break;
                }

                if (exceptions != null && !exceptions.IsEmpty)
                    throw new AggregateException(exceptions);
            }
        }
    }
}

#endif