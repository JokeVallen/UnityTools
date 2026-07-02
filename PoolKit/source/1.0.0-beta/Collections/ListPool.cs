using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PoolKit.Collections
{
    /// <summary>
    /// 列表对象池
    /// </summary>
    public static class ListPool
    {
        /// <summary>
        /// 列表对象池的作用域
        /// </summary>
        /// <typeparam name="T">集合元素的类型</typeparam>
        public readonly struct PoolScope<T> : IDisposable
        {
            /// <summary>
            /// 列表
            /// </summary>
            public List<T> List => collection;
            private readonly List<T> collection;

            internal PoolScope(List<T> collection)
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

        private interface IListPool 
        {
            void Clear();
        }

        private static readonly ConcurrentDictionary<Type, IListPool> pools = new ConcurrentDictionary<Type, IListPool>();
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
                pools[typeof(T)] = ListPoolHandler<T>.Shared;
            return new PoolScope<T>(ListPoolHandler<T>.Shared.Rent());
        }

        /// <summary>
        /// 租借集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <returns>集合实例</returns>
        public static List<T> Rent<T>() 
        {
            ThrowErrorIfDisposed();
            if (!pools.TryGetValue(typeof(T), out _))
                pools[typeof(T)] = ListPoolHandler<T>.Shared;
            return ListPoolHandler<T>.Shared.Rent();
        }

        /// <summary>
        /// 归还集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="collection">集合实例</param>
        public static void Return<T>(List<T> collection) 
        {
            ThrowErrorIfDisposed();
            ListPoolHandler<T>.Shared.Return(collection);
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
                throw new System.ObjectDisposedException(nameof(ListPool));
        }

        private class ListPoolHandler<T> : IDisposable, IListPool
        {
            public static readonly ListPoolHandler<T> Shared = new ListPoolHandler<T>();

            public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
            private readonly ConcurrentStack<List<T>> pool = new ConcurrentStack<List<T>>();
            private readonly Func<List<T>> factory;
            private bool disposed;
            private int capacity;

            public ListPoolHandler() : this(DEFAULT_CAPACITY, null) { }
            public ListPoolHandler(int capacity) : this(capacity, null) { }
            public ListPoolHandler(int capacity, Func<List<T>> factory)
            {
                this.capacity = Math.Max(0, capacity);
                this.factory = factory != null ? factory : (() => new List<T>());
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
                    if (!pool.TryPop(out var list)) continue;
                    list.Clear();
                }
            }

            public List<T> Rent()
            {
                ThrowErrorIfDisposed();
                if (pool.TryPop(out List<T> list))
                    return list;
                return factory();
            }

            public void Return(List<T> collection)
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
                    throw new System.ObjectDisposedException(nameof(ListPool));
            }
        }
    }
}
