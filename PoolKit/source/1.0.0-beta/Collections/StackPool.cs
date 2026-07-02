using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PoolKit.Collections
{
    /// <summary>
    /// 栈对象池
    /// </summary>
    public static class StackPool
    {
        /// <summary>
        /// 栈对象池的作用域
        /// </summary>
        /// <typeparam name="T">集合元素的类型</typeparam>
        public readonly struct PoolScope<T> : IDisposable
        {
            /// <summary>
            /// 栈
            /// </summary>
            public Stack<T> Stack => collection;
            private readonly Stack<T> collection;

            internal PoolScope(Stack<T> collection)
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

        private interface IStackPool 
        {
            void Clear();
        }

        private static readonly ConcurrentDictionary<Type, IStackPool> pools = new ConcurrentDictionary<Type, IStackPool>();
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
                pools[typeof(T)] = StackPoolHandler<T>.Shared;
            return new PoolScope<T>(StackPoolHandler<T>.Shared.Rent());
        }

        /// <summary>
        /// 租借集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <returns>集合实例</returns>
        public static Stack<T> Rent<T>()
        {
            ThrowErrorIfDisposed();
            if (!pools.TryGetValue(typeof(T), out _))
                pools[typeof(T)] = StackPoolHandler<T>.Shared;
            return StackPoolHandler<T>.Shared.Rent();
        }

        /// <summary>
        /// 归还集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="collection">集合实例</param>
        public static void Return<T>(Stack<T> collection)
        {
            ThrowErrorIfDisposed();
            StackPoolHandler<T>.Shared.Return(collection);
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
                throw new System.ObjectDisposedException(nameof(StackPool));
        }

        private class StackPoolHandler<T> : IDisposable, IStackPool
        {
            public static readonly StackPoolHandler<T> Shared = new StackPoolHandler<T>();

            public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
            private readonly ConcurrentStack<Stack<T>> pool = new ConcurrentStack<Stack<T>>();
            private readonly Func<Stack<T>> factory;
            private bool disposed;
            private int capacity;

            public StackPoolHandler() : this(DEFAULT_CAPACITY, null) { }
            public StackPoolHandler(int capacity) : this(capacity, null) { }
            public StackPoolHandler(int capacity, Func<Stack<T>> factory)
            {
                this.capacity = Math.Max(0, capacity);
                this.factory = factory != null ? factory : (() => new Stack<T>());
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
                    if (!pool.TryPop(out var Stack)) continue;
                    Stack.Clear();
                }
            }

            public Stack<T> Rent()
            {
                ThrowErrorIfDisposed();
                if (pool.TryPop(out Stack<T> Stack))
                    return Stack;
                return factory();
            }

            public void Return(Stack<T> collection)
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
                    throw new System.ObjectDisposedException(nameof(StackPool));
            }
        }
    }
}
