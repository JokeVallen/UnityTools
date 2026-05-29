using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    internal interface IListPool { }

    internal static class ListPool 
    { 
        private static readonly ConcurrentDictionary<Type, IListPool> globalPools = new ConcurrentDictionary<Type, IListPool>();
        private const int DEFAULT_CAPACITY = 1024;
        private static bool disposed;

        public static List<T> Rent<T>() 
        {
            ThrowErrorIfDisposed();
            if (!globalPools.TryGetValue(typeof(T), out _))
                globalPools[typeof(T)] = ListPoolHandler<T>.Shared;
            return ListPoolHandler<T>.Shared.Rent();
        }

        public static void Return<T>(List<T> list) 
        {
            ThrowErrorIfDisposed();
            ListPoolHandler<T>.Shared.Return(list);
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

        private class ListPoolHandler<T> : IDisposable, IListPool
        {
            public static readonly ListPoolHandler<T> Shared = new ListPoolHandler<T>();

            public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
            private readonly ConcurrentBag<List<T>> bag = new ConcurrentBag<List<T>>();
            private readonly Func<List<T>> factory;
            private bool disposed;
            private int capacity;

            public ListPoolHandler() : this(DEFAULT_CAPACITY, null) { }
            public ListPoolHandler(int capacity) : this(capacity, null) { }
            public ListPoolHandler(int capacity, Func<List<T>> factory)
            {
                this.capacity = Math.Max(0, capacity);
                this.factory = factory ?? (() => new List<T>());
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                int count = bag.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!bag.TryTake(out var list)) continue;
                    list.Clear();
                }
            }

            public List<T> Rent()
            {
                ThrowErrorIfDisposed();
                if (bag.TryTake(out List<T> list))
                    return list;
                return factory();
            }

            public void Return(List<T> list)
            {
                ThrowErrorIfDisposed();
                if (list == null) throw new ArgumentNullException(nameof(list));
                list.Clear();
                if (bag.Count < capacity)
                    bag.Add(list);
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
