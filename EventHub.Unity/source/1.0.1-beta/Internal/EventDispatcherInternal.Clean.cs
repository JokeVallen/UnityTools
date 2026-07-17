#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace EventHub.Unity
{
    internal partial class EventDispatcherInternal : ICleanable
    {
        private bool disposed;

        public int TryCleanupUnusedLocks()
        {
            ThrowErrorIfDisposed();
            return TryCleanupUnusedLocksInternal(asyncLocks, asyncSubscribers)
                + TryCleanupUnusedLocksInternal(syncLocks, syncSubscribers);
        }

        public int TryCleanupUnusedCollections()
        {
            ThrowErrorIfDisposed();
            return TryCleanupUnusedCollectionsInternal(asyncLocks, asyncSubscribers)
                + TryCleanupUnusedCollectionsInternal(syncLocks, syncSubscribers);
        }

        public int TryCleanupUnusedLocksAndCollections()
        {
            ThrowErrorIfDisposed();
            return TryCleanupUnusedLocksAndCollectionsInternal(asyncLocks, asyncSubscribers)
                + TryCleanupUnusedLocksAndCollectionsInternal(syncLocks, syncSubscribers);
        }

        public void Dispose()
        {
            DisposeInternal();
        }

        public void SafeDispose() 
        {
            SafeDisposeInternal();
        }

        private static int TryCleanupUnusedLocksInternal<TCollection>(ConcurrentDictionary<Type, ReaderWriterLockSlim> keyDict, ConcurrentDictionary<Type, TCollection> subscriberDict)
        where TCollection : class, ISubscriberCollection
        {
            var eventTypes = keyDict.Keys.ToArray();
            int deleted = 0;
            foreach (var eventType in eventTypes)
            {
                if (subscriberDict.TryGetValue(eventType, out var collection))
                {
                    if (collection.Count > 0)
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
                        if (collection.Count > 0)
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
        where TCollection : class, ISubscriberCollection
        {
            var eventTypes = keyDict.Keys.ToArray();
            int deleted = 0;
            foreach (var eventType in eventTypes)
            {
                if (subscriberDict.TryGetValue(eventType, out var collection))
                {
                    if (collection.Count > 0)
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
                        if (collection.Count > 0)
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
        where TCollection : class, ISubscriberCollection
        {
            var eventTypes = keyDict.Keys.ToArray();
            int deleted = 0;
            foreach (var eventType in eventTypes)
            {
                if (subscriberDict.TryGetValue(eventType, out var collection))
                {
                    if (collection.Count > 0)
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
                        if (collection.Count > 0)
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

        private void DisposeInternal() 
        {
            if (disposed) return;
            disposed = true;

            foreach (var lockSlim in syncLocks.Values)
                lockSlim.Dispose();
            foreach (var lockSlim in asyncLocks.Values)
                lockSlim.Dispose();

            syncLocks.Clear();
            asyncLocks.Clear();
            syncSubscribers.Clear();
            asyncSubscribers.Clear();
            EventDispatcherLog.Dispose();
            ExceptionCatcher.Dispose();
        }

        private void SafeDisposeInternal() 
        {
            if (disposed) return;
            disposed = true;

            foreach (var lockSlim in syncLocks.Values)
                lockSlim.EnterWriteLock();
            foreach (var lockSlim in asyncLocks.Values)
                lockSlim.EnterWriteLock();

            try
            {
                syncSubscribers.Clear();
                asyncSubscribers.Clear();
            }
            finally
            {
                foreach (var lockSlim in syncLocks.Values)
                {
                    lockSlim.ExitWriteLock();
                    lockSlim.Dispose();
                }
                foreach (var lockSlim in asyncLocks.Values)
                {
                    lockSlim.ExitWriteLock();
                    lockSlim.Dispose();
                }
                syncLocks.Clear();
                asyncLocks.Clear();
            }
        }

        private void ThrowErrorIfDisposed() 
        { 
            if(disposed)
                throw new ObjectDisposedException("EventHub.Unity.EventDispatcher");
        }
    }
}

#endif