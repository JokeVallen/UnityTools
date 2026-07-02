using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PoolKit.Collections
{
    /// <summary>
    /// 哈希集合对象池
    /// </summary>
    public static class HashSetPool
    {
        /// <summary>
        /// 哈希集合对象池的作用域
        /// </summary>
        /// <typeparam name="T">集合元素的类型</typeparam>
        public readonly struct PoolScope<T> : IDisposable
        {
            /// <summary>
            /// 哈希集合
            /// </summary>
            public HashSet<T> HashSet => collection;
            private readonly HashSet<T> collection;

            internal PoolScope(HashSet<T> collection)
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

        private interface IHashSetPool 
        {
            void Clear();
        }

        private static readonly ConcurrentDictionary<Type, IHashSetPool> pools = new ConcurrentDictionary<Type, IHashSetPool>();
        private const int DEFAULT_CAPACITY = 16;
        private static bool disposed;

        /// <summary>
        /// 租借集合附带作用域实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <returns>作用域实例</returns>
        public static PoolScope<T> RentWithScope<T>()
        {
            ThrowErrorIfDisposed();
            if (!pools.TryGetValue(typeof(T), out _))
                pools[typeof(T)] = HashSetPoolHandler<T>.Shared;
            return new PoolScope<T>(HashSetPoolHandler<T>.Shared.Rent());
        }

        /// <summary>
        /// 租借集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <returns>集合实例</returns>
        public static HashSet<T> Rent<T>()
        {
            ThrowErrorIfDisposed();
            if (!pools.TryGetValue(typeof(T), out _))
                pools[typeof(T)] = HashSetPoolHandler<T>.Shared;
            return HashSetPoolHandler<T>.Shared.Rent();
        }

        /// <summary>
        /// 归还集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="collection">集合实例</param>
        public static void Return<T>(HashSet<T> collection)
        {
            ThrowErrorIfDisposed();
            HashSetPoolHandler<T>.Shared.Return(collection);
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
                throw new System.ObjectDisposedException(nameof(HashSetPool));
        }

        private class HashSetPoolHandler<T> : IDisposable, IHashSetPool
        {
            public static readonly HashSetPoolHandler<T> Shared = new HashSetPoolHandler<T>();

            public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
            private readonly ConcurrentStack<HashSet<T>> pool = new ConcurrentStack<HashSet<T>>();
            private readonly Func<HashSet<T>> factory;
            private bool disposed;
            private int capacity;

            public HashSetPoolHandler() : this(DEFAULT_CAPACITY, null) { }
            public HashSetPoolHandler(int capacity) : this(capacity, null) { }
            public HashSetPoolHandler(int capacity, Func<HashSet<T>> factory)
            {
                this.capacity = Math.Max(0, capacity);
                this.factory = factory != null ? factory : (() => new HashSet<T>());
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
                    if (!pool.TryPop(out var HashSet)) continue;
                    HashSet.Clear();
                }
            }

            public HashSet<T> Rent()
            {
                ThrowErrorIfDisposed();
                if (pool.TryPop(out HashSet<T> HashSet))
                    return HashSet;
                return factory();
            }

            public void Return(HashSet<T> collection)
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
                    throw new System.ObjectDisposedException(nameof(HashSetPool));
            }
        }
    }
}
