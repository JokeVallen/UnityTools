using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    internal interface IDictionaryPool { }

    internal static class DictionaryPool 
    {
        private static readonly ConcurrentDictionary<(Type keyType,Type valueType), IDictionaryPool> globalPools = new ConcurrentDictionary<(Type keyType, Type valueType), IDictionaryPool>();
        private const int DEFAULT_CAPACITY = 1024;
        private static bool disposed;

        public static Dictionary<TKey, TValue> Rent<TKey, TValue>()
        {
            ThrowErrorIfDisposed();
            var combinedKey = (typeof(TKey), typeof(TValue));
            if (!globalPools.TryGetValue(combinedKey, out _))
                globalPools[combinedKey] = DictionaryPoolHandler<TKey, TValue>.Shared;
            return DictionaryPoolHandler<TKey, TValue>.Shared.Rent();
        }

        public static void Return<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            ThrowErrorIfDisposed();
            DictionaryPoolHandler<TKey, TValue>.Shared.Return(dict);
        }

        internal static void Dispose()
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
                throw new System.ObjectDisposedException(nameof(Progress));
        }

        private class DictionaryPoolHandler<TKey, TValue> : IDisposable, IDictionaryPool
        {
            public static readonly DictionaryPoolHandler<TKey, TValue> Shared = new DictionaryPoolHandler<TKey, TValue>();

            public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
            private readonly ConcurrentBag<Dictionary<TKey, TValue>> bag = new ConcurrentBag<Dictionary<TKey, TValue>>();
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
                    if (!bag.TryTake(out var dict)) continue;
                    dict.Clear();
                }
            }

            public Dictionary<TKey, TValue> Rent()
            {
                ThrowErrorIfDisposed();
                if (bag.TryTake(out var dict))
                    return dict;
                return factory();
            }

            public void Return(Dictionary<TKey, TValue> dictionary)
            {
                ThrowErrorIfDisposed();
                if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
                dictionary.Clear();
                if (bag.Count < capacity)
                    bag.Add(dictionary);
            }

            public bool TryTrim()
            {
                int count = bag.Count;
                while (count > capacity)
                {
                    bag.TryTake(out _);
                    count--;
                }

                return bag.Count <= capacity;
            }

            private void ThrowErrorIfDisposed()
            {
                if (disposed)
                    throw new System.ObjectDisposedException(nameof(Progress));
            }
        }
    }
}