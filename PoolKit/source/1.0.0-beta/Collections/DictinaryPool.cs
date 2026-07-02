using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PoolKit.Collections
{
    /// <summary>
    /// 字典对象池
    /// </summary>
    public static class DictionaryPool 
    {
        /// <summary>
        /// 字典对象池的作用域
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        public readonly struct PoolScope<TKey, TValue> : IDisposable
        {
            /// <summary>
            /// 字典
            /// </summary>
            public Dictionary<TKey, TValue> Dictionary => collection;
            private readonly Dictionary<TKey, TValue> collection;

            internal PoolScope(Dictionary<TKey, TValue> collection)
            {
                if (collection == null) throw new ArgumentNullException(nameof(collection));
                this.collection = collection;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                Return(collection);
            }
        }

        private interface IDictionaryPool 
        {
            void Clear();
        }

        private static readonly ConcurrentDictionary<(Type keyType,Type valueType), IDictionaryPool> pools = new ConcurrentDictionary<(Type keyType, Type valueType), IDictionaryPool>();
        private const int DEFAULT_CAPACITY = 16;
        private static bool disposed;

        /// <summary>
        /// 租借集合附带作用域实例
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <returns>作用域实例</returns>
        public static PoolScope<TKey, TValue> RentWithScope<TKey, TValue>()
        {
            ThrowErrorIfDisposed();
            var combinedKey = (typeof(TKey), typeof(TValue));
            if (!pools.TryGetValue(combinedKey, out _))
                pools[combinedKey] = DictionaryPoolHandler<TKey, TValue>.Shared;
            return new PoolScope<TKey, TValue>(DictionaryPoolHandler<TKey, TValue>.Shared.Rent());
        }

        /// <summary>
        /// 租借集合实例
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <returns>集合实例</returns>
        public static Dictionary<TKey, TValue> Rent<TKey, TValue>()
        {
            ThrowErrorIfDisposed();
            var combinedKey = (typeof(TKey), typeof(TValue));
            if (!pools.TryGetValue(combinedKey, out _))
                pools[combinedKey] = DictionaryPoolHandler<TKey, TValue>.Shared;
            return DictionaryPoolHandler<TKey, TValue>.Shared.Rent();
        }

        /// <summary>
        /// 归还集合实例
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="collection">集合实例</param>
        public static void Return<TKey, TValue>(Dictionary<TKey, TValue> collection)
        {
            ThrowErrorIfDisposed();
            DictionaryPoolHandler<TKey, TValue>.Shared.Return(collection);
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public static void Clear()
        {
            if (disposed) return;
            foreach (var pool in pools.Values)
            {
                if (pool == null) continue;
                pool.Clear();
            }
        }

        /// <summary>
        /// 释放对象池
        /// </summary>
        public static void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var pool in pools.Values)
            {
                if (!(pool is IDisposable)) continue;
                ((IDisposable)pool).Dispose();
            }
            pools.Clear();
        }

        private static void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new System.ObjectDisposedException(nameof(DictionaryPool));
        }

        private class DictionaryPoolHandler<TKey, TValue> : IDisposable, IDictionaryPool
        {
            public static readonly DictionaryPoolHandler<TKey, TValue> Shared = new DictionaryPoolHandler<TKey, TValue>();

            public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
            private readonly ConcurrentStack<Dictionary<TKey, TValue>> pool = new ConcurrentStack<Dictionary<TKey, TValue>>();
            private readonly Func<Dictionary<TKey, TValue>> factory;
            private bool disposed;
            private int capacity;

            public DictionaryPoolHandler() : this(DEFAULT_CAPACITY, null) { }
            public DictionaryPoolHandler(int capacity) : this(capacity, null) { }
            public DictionaryPoolHandler(int capacity, Func<Dictionary<TKey, TValue>> factory)
            {
                this.capacity = Math.Max(0, capacity);
                this.factory = factory != null ? factory : (() => new Dictionary<TKey, TValue>());
            }

            public void Clear() 
            {
                if (disposed) return;
                pool.Clear();
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                int count = pool.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!pool.TryPop(out var dict)) continue;
                    dict.Clear();
                }
            }

            public Dictionary<TKey, TValue> Rent()
            {
                ThrowErrorIfDisposed();
                if (pool.TryPop(out var dict))
                    return dict;
                return factory();
            }

            public void Return(Dictionary<TKey, TValue> collection)
            {
                ThrowErrorIfDisposed();
                if (collection == null) throw new ArgumentNullException(nameof(collection));
                collection.Clear();
                if (pool.Count < capacity)
                    pool.Push(collection);
            }

            private void ThrowErrorIfDisposed()
            {
                if (disposed)
                    throw new System.ObjectDisposedException(nameof(DictionaryPool));
            }
        }
    }
}