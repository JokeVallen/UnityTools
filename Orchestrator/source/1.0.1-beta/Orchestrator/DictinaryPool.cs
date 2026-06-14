using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Orchestrator
{
    /// <summary>
    /// 字典对象池
    /// </summary>
    public static class DictionaryPool 
    {
        private interface IDictionaryPool { }

        private static readonly ConcurrentDictionary<(Type keyType,Type valueType), IDictionaryPool> globalPools = new ConcurrentDictionary<(Type keyType, Type valueType), IDictionaryPool>();
        private const int DEFAULT_CAPACITY = 16;
        private static bool disposed;

        /// <summary>
        /// 租借字典实例
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <returns>字典实例</returns>
        public static Dictionary<TKey, TValue> Rent<TKey, TValue>()
        {
            ThrowErrorIfDisposed();
            var combinedKey = (typeof(TKey), typeof(TValue));
            if (!globalPools.TryGetValue(combinedKey, out _))
                globalPools[combinedKey] = DictionaryPoolHandler<TKey, TValue>.Shared;
            return DictionaryPoolHandler<TKey, TValue>.Shared.Rent();
        }

        /// <summary>
        /// 归还字典实例
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="dict">字典实例</param>
        public static void Return<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            ThrowErrorIfDisposed();
            DictionaryPoolHandler<TKey, TValue>.Shared.Return(dict);
        }

        /// <summary>
        /// 释放对象池
        /// </summary>
        public static void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var pool in globalPools.Values)
            {
                if (!(pool is IDisposable disposable)) continue;
                disposable.Dispose();
            }
        }

        private static void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new System.ObjectDisposedException(nameof(Orchestrator));
        }

        private class DictionaryPoolHandler<TKey, TValue> : IDisposable, IDictionaryPool
        {
            public static readonly DictionaryPoolHandler<TKey, TValue> Shared = new DictionaryPoolHandler<TKey, TValue>();

            public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
            private readonly ConcurrentStack<Dictionary<TKey, TValue>> bag = new ConcurrentStack<Dictionary<TKey, TValue>>();
            private readonly Func<Dictionary<TKey, TValue>> factory;
            private bool disposed;
            private int capacity;

            public DictionaryPoolHandler() : this(DEFAULT_CAPACITY, null) { }
            public DictionaryPoolHandler(int capacity) : this(capacity, null) { }
            public DictionaryPoolHandler(int capacity, Func<Dictionary<TKey, TValue>> factory)
            {
                this.capacity = Math.Max(0, capacity);
                this.factory = factory ?? (() => new Dictionary<TKey, TValue>());
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                int count = bag.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!bag.TryPeek(out var dict)) continue;
                    dict.Clear();
                }
            }

            public Dictionary<TKey, TValue> Rent()
            {
                ThrowErrorIfDisposed();
                if (bag.TryPeek(out var dict))
                    return dict;
                return factory();
            }

            public void Return(Dictionary<TKey, TValue> dictionary)
            {
                ThrowErrorIfDisposed();
                if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
                dictionary.Clear();
                if (bag.Count < capacity)
                    bag.Push(dictionary);
            }

            public bool TryTrim()
            {
                int count = bag.Count;
                while (count > capacity)
                {
                    bag.TryPeek(out _);
                    count--;
                }

                return bag.Count <= capacity;
            }

            private void ThrowErrorIfDisposed()
            {
                if (disposed)
                    throw new System.ObjectDisposedException(nameof(Orchestrator));
            }
        }
    }
}