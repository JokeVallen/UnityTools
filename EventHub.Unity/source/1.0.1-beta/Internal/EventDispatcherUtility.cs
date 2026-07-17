#if !EVENTHUB_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal static class EventDispatcherUtility
    {
        public static void InsertSorted<TEvent>(ISyncSubscriberCollection<TEvent> collection, SyncSubscriber<TEvent> item)
        {
            var index = FindInsertIndex(collection, item.Priority);
            collection.Insert(index, item);
        }

        public static void InsertSorted<TEvent>(IAsyncSubscriberCollection<TEvent> collection, AsyncSubscriber<TEvent> item)
        {
            var index = FindInsertIndex(collection, item.Priority);
            collection.Insert(index, item);
        }

        public static bool IsValidEvent<TEvent>(TEvent @event)
        {
            if (@event == null)
            {
                EventDispatcherLog.LogWarning($"The event typed '{typeof(TEvent).Name}' cannot be null.");
                return false;
            }
            return true;
        }

        public static bool IsValidHandler<TEvent>(Delegate handler)
        {
            if (handler == null)
            {
                EventDispatcherLog.LogWarning($"The handler for event type '{typeof(TEvent).Name}' cannot be null.");
                return false;
            }
            return true;
        }

        public static void Invoke<TEvent>(Action<TEvent> handler, TEvent @event)
        {
            handler(@event);
        }

        public static UniTask Invoke<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, TEvent @event, CancellationToken cancellationToken = default)
        {
            return handler(@event, cancellationToken);
        }

        public static void SafeInvoke<TEvent>(Action<TEvent> handler, TEvent @event)
        {
            try
            {
                handler(@event);
            }
            catch (Exception ex)
            {
                var eventType = typeof(TEvent);
                EventDispatcherLog.LogError(eventType, handler, ex);
                ExceptionCatcher.CatchError(eventType, handler, ex);
            }
        }

        public static void SafeInvoke<TEvent>(Action<TEvent> handler, TEvent @event, out Exception exception)
        {
            exception = null;
            try
            {
                handler(@event);
            }
            catch (Exception ex)
            {
                exception = ex;
                var eventType = typeof(TEvent);
                EventDispatcherLog.LogError(eventType, handler, ex);
                ExceptionCatcher.CatchError(eventType, handler, ex);
            }
        }

        public static void SafeInvoke<TEvent>(Action<TEvent> handler, TEvent @event, Action<Exception> onError)
        {
            try
            {
                handler(@event);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                var eventType = typeof(TEvent);
                EventDispatcherLog.LogError(eventType, handler, ex);
                ExceptionCatcher.CatchError(eventType, handler, ex);
            }
        }

        public static async UniTask SafeInvoke<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, TEvent @event, CancellationToken cancellationToken = default, Action<Exception> onError = null)
        {
            try
            {
                await handler(@event, cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                var eventType = typeof(TEvent);
                EventDispatcherLog.LogError(eventType, handler, ex);
                ExceptionCatcher.CatchError(eventType, handler, ex);
            }
        }

        public static void CatchError(Type eventType, Delegate handler, Exception exception) 
        {
            EventDispatcherLog.LogError(eventType, handler, exception);
            ExceptionCatcher.CatchError(eventType, handler, exception);
        }

        public static void CatchError(Exception exception)
        {
            EventDispatcherLog.LogError(exception);
        }

        public static void CatchError(string message) 
        {
            EventDispatcherLog.LogError(message);
        }

        private static int FindInsertIndex<T>(IEnumerable<T> collection,int priority) where T : ISubscriber
        {
            if (collection is IIndexable<T> indexable)
            {
                int count = indexable.Count;
                if (count < 16)
                {
                    return FindInsertIndexLinear(indexable, priority);
                }
                else
                {
                    return FindInsertIndexBinary(indexable, priority);
                }
            }
            else
            {
                return FindInsertIndexLinear(collection, priority);
            }
        }

        // 线性查找：基于索引器 + Count，适用于小规模集合，时间复杂度O(n)
        private static int FindInsertIndexLinear<T>(IIndexable<T> collection,int priority) where T : ISubscriber
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Priority < priority) return i;
            }

            return collection.Count;
        }

        // 线性查走：基于枚举器，适用于无法通过索引访问的集合，时间复杂度O(n)
        private static int FindInsertIndexLinear<T>(IEnumerable<T> collection, int priority) where T : ISubscriber
        {
            int index = 0;
            foreach (var item in collection)
            {
                if (item.Priority < priority) return index;
                index++;
            }
            return index;
        }

        // 二分查找：基于索引器 + Count，时间复杂度O(log n)
        private static int FindInsertIndexBinary<T>(IIndexable<T> collection, int priority) where T : ISubscriber
        {
            int low = 0, high = collection.Count;
            while (low < high)
            {
                int mid = low + (high - low) / 2;
                if (collection[mid].Priority >= priority)
                    low = mid + 1;
                else
                    high = mid;
            }
            return low;
        }
    }
}

#endif