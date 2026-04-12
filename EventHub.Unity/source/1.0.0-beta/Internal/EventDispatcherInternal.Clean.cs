#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace EventHub.Unity
{
    internal partial class EventDispatcherInternal : ICleanable
    {
        public int TryCleanupUnusedLocks()
        {
            return TryCleanupUnusedLocksInternal(asyncLocks, asyncSubscribers)
                + TryCleanupUnusedLocksInternal(syncLocks, syncSubscribers);
        }

        public int TryCleanupUnusedCollections()
        {
            return TryCleanupUnusedCollectionsInternal(asyncLocks, asyncSubscribers)
                + TryCleanupUnusedCollectionsInternal(syncLocks, syncSubscribers);
        }

        public int TryCleanupUnusedLocksAndCollections()
        {
            return TryCleanupUnusedLocksAndCollectionsInternal(asyncLocks, asyncSubscribers)
                + TryCleanupUnusedLocksAndCollectionsInternal(syncLocks, syncSubscribers);
        }

        private static int TryCleanupUnusedLocksInternal<TCollection>(ConcurrentDictionary<Type, ReaderWriterLockSlim> keyDict, ConcurrentDictionary<Type, TCollection> subscriberDict)
        where TCollection : class
        {
            var eventTypes = keyDict.Keys.ToArray();
            int deleted = 0;
            foreach (var eventType in eventTypes)
            {
                if (subscriberDict.TryGetValue(eventType, out var collection))
                {
                    if (collection is ICountable countable && countable.Count > 0)
                        continue;
                }

                if (!keyDict.TryGetValue(eventType, out var key))
                    continue;

                bool lockAcquired = false;
                try
                {
                    lockAcquired = key.TryEnterWriteLock(10);
                    if (!lockAcquired) continue;

                    if (subscriberDict.TryGetValue(eventType, out collection))
                    {
                        if (collection is ICountable countable2 && countable2.Count > 0)
                            continue;
                    }

                    if (keyDict.TryRemove(eventType, out var removedLock))
                    {
                        deleted++;
                        key.ExitWriteLock();
                        lockAcquired = false;
                        removedLock.Dispose();
                    }
                }
                finally
                {
                    if (lockAcquired)
                        key.ExitWriteLock();
                }
            }

            return deleted;
        }

        private static int TryCleanupUnusedCollectionsInternal<TCollection>(ConcurrentDictionary<Type, ReaderWriterLockSlim> keyDict, ConcurrentDictionary<Type, TCollection> subscriberDict)
        where TCollection : class
        {
            var eventTypes = keyDict.Keys.ToArray();
            int deleted = 0;
            foreach (var eventType in eventTypes)
            {
                if (subscriberDict.TryGetValue(eventType, out var collection))
                {
                    if (collection is ICountable countable && countable.Count > 0)
                        continue;
                }

                if (!keyDict.TryGetValue(eventType, out var key))
                    continue;

                bool lockAcquired = false;
                try
                {
                    lockAcquired = key.TryEnterWriteLock(10);
                    if (!lockAcquired) continue;

                    if (subscriberDict.TryGetValue(eventType, out collection))
                    {
                        if (collection is ICountable countable2 && countable2.Count > 0)
                            continue;
                    }

                    if (subscriberDict.TryRemove(eventType, out _))
                    {
                        deleted++;
                    }
                }
                finally
                {
                    if (lockAcquired)
                        key.ExitWriteLock();
                }
            }

            return deleted;
        }

        private static int TryCleanupUnusedLocksAndCollectionsInternal<TCollection>(ConcurrentDictionary<Type, ReaderWriterLockSlim> keyDict, ConcurrentDictionary<Type, TCollection> subscriberDict)
        {
            var eventTypes = keyDict.Keys.ToArray();
            int deleted = 0;
            foreach (var eventType in eventTypes)
            {
                if (subscriberDict.TryGetValue(eventType, out var collection))
                {
                    if (collection is ICountable countable && countable.Count > 0)
                        continue;
                }

                if (!keyDict.TryGetValue(eventType, out var key))
                    continue;

                bool lockAcquired = false;
                try
                {
                    lockAcquired = key.TryEnterWriteLock(10);
                    if (!lockAcquired) continue;

                    if (subscriberDict.TryGetValue(eventType, out collection))
                    {
                        if (collection is ICountable countable2 && countable2.Count > 0)
                            continue;
                    }

                    if (subscriberDict.TryRemove(eventType, out _))
                    {
                        deleted++;
                    }

                    if (keyDict.TryRemove(eventType, out var removedLock))
                    {
                        deleted++;
                        key.ExitWriteLock();
                        lockAcquired = false;
                        removedLock.Dispose();
                    }
                }
                finally
                {
                    if (lockAcquired)
                        key.ExitWriteLock();
                }
            }

            return deleted;
        }
    }
}

#endif