using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PoolKit.Collections
{
    /// <summary>
    /// 队列对象池
    /// </summary>
    public static class QueuePool
    {
        /// <summary>
        /// 队列对象池的作用域
        /// </summary>
        /// <typeparam name="T">集合元素的类型</typeparam>
        public readonly struct PoolScope<T> : IDisposable
        {
            /// <summary>
            /// 队列
            /// </summary>
            public Queue<T> Queue => collection;
            private readonly Queue<T> collection;

            internal PoolScope(Queue<T> collection)
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

        private interface IQueuePool 
        {
            void Clear();
        }

        private static readonly ConcurrentDictionary<Type, IQueuePool> pools = new ConcurrentDictionary<Type, IQueuePool>();
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
                pools[typeof(T)] = QueuePoolHandler<T>.Shared;
            return new PoolScope<T>(QueuePoolHandler<T>.Shared.Rent());
        }

        /// <summary>
        /// 租借集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <returns>集合实例</returns>
        public static Queue<T> Rent<T>()
        {
            ThrowErrorIfDisposed();
            if (!pools.TryGetValue(typeof(T), out _))
                pools[typeof(T)] = QueuePoolHandler<T>.Shared;
            return QueuePoolHandler<T>.Shared.Rent();
        }

        /// <summary>
        /// 归还集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="collection">集合实例</param>
        public static void Return<T>(Queue<T> collection)
        {
            ThrowErrorIfDisposed();
            QueuePoolHandler<T>.Shared.Return(collection);
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
                throw new System.ObjectDisposedException(nameof(QueuePool));
        }

        private class QueuePoolHandler<T> : IDisposable, IQueuePool
        {
            public static readonly QueuePoolHandler<T> Shared = new QueuePoolHandler<T>();

            public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
            private readonly ConcurrentStack<Queue<T>> pool = new ConcurrentStack<Queue<T>>();
            private readonly Func<Queue<T>> factory;
            private bool disposed;
            private int capacity;

            public QueuePoolHandler() : this(DEFAULT_CAPACITY, null) { }
            public QueuePoolHandler(int capacity) : this(capacity, null) { }
            public QueuePoolHandler(int capacity, Func<Queue<T>> factory)
            {
                this.capacity = Math.Max(0, capacity);
                this.factory = factory != null ? factory : (() => new Queue<T>());
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
                    if (!pool.TryPop(out var queue)) continue;
                    queue.Clear();
                }
            }

            public Queue<T> Rent()
            {
                ThrowErrorIfDisposed();
                if (pool.TryPop(out Queue<T> queue))
                    return queue;
                return factory();
            }

            public void Return(Queue<T> collection)
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
                    throw new System.ObjectDisposedException(nameof(QueuePool));
            }
        }
    }
}
