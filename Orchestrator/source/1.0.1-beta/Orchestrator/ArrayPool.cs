using System;
using System.Collections.Concurrent;

namespace Orchestrator
{
    /// <summary>
    /// 数组对象池
    /// </summary>
    public static class ArrayPool
    {
        private interface IArrayPool { }

        private static readonly ConcurrentDictionary<Type, IArrayPool> globalPools = new ConcurrentDictionary<Type, IArrayPool>();
        private const int DefaultMaxArraysPerBucket = 32;
        private const int MaxBucketIndex = 20;
        private static bool disposed;

        /// <summary>
        /// 租借数组实例
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="minimumLength">数组最小长度</param>
        /// <returns>数组实例</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minimumLength"/> 数值非法。</exception>
        public static T[] Rent<T>(int minimumLength)
        {
            ThrowIfDisposed();
            if (minimumLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength));

            if (minimumLength == 0)
                return Array.Empty<T>();

            var pool = GetPool<T>();
            return pool.Rent(minimumLength);
        }

        /// <summary>
        /// 归还数组实例
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="array">数组实例</param>
        /// <param name="clearArray">是否清空数组</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> 不能为 null。</exception>
        public static void Return<T>(T[] array, bool clearArray = false)
        {
            ThrowIfDisposed();
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Length == 0)
                return;

            var pool = GetPool<T>();
            pool.Return(array, clearArray);
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
                if (pool is IDisposable disposable)
                    disposable.Dispose();
            }
            globalPools.Clear();
        }

        private static ArrayPoolImpl<T> GetPool<T>()
        {
            var type = typeof(T);
            if (!globalPools.TryGetValue(type, out var pool))
            {
                pool = new ArrayPoolImpl<T>();
                globalPools[type] = pool;
            }
            return (ArrayPoolImpl<T>)pool;
        }

        private static void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Orchestrator));
        }

        private sealed class ArrayPoolImpl<T> : IDisposable, IArrayPool
        {
            private readonly int maxArraysPerBucket;
            private readonly Bucket[] buckets;
            private bool isDisposed;

            public ArrayPoolImpl() : this(DefaultMaxArraysPerBucket) { }

            public ArrayPoolImpl(int maxArraysPerBucket)
            {
                this.maxArraysPerBucket = maxArraysPerBucket;
                buckets = new Bucket[MaxBucketIndex + 1];
            }

            public T[] Rent(int minimumLength)
            {
                int bucketIndex = GetBucketIndex(minimumLength);

                if (bucketIndex > MaxBucketIndex)
                    return new T[minimumLength];

                var bucket = buckets[bucketIndex];
                if (bucket == null)
                {
                    lock (buckets)
                    {
                        bucket = buckets[bucketIndex];
                        if (bucket == null)
                        {
                            int bucketSize = GetBucketSize(bucketIndex);
                            bucket = new Bucket(bucketSize, maxArraysPerBucket);
                            buckets[bucketIndex] = bucket;
                        }
                    }
                }

                return bucket.Rent();
            }

            public void Return(T[] array, bool clearArray)
            {
                int bucketIndex = GetBucketIndex(array.Length);
                if (bucketIndex > MaxBucketIndex)
                    return;

                var bucket = buckets[bucketIndex];
                if (bucket == null)
                    return;

                if (clearArray)
                    Array.Clear(array, 0, array.Length);

                bucket.Return(array);
            }

            public void Dispose()
            {
                if (isDisposed) return;
                isDisposed = true;

                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i]?.Dispose();
                }
            }

            private static int GetBucketIndex(int length)
            {
                uint size = (uint)(length - 1);
                size |= 0xF;
                int log2 = Log2(size);
                return log2 - 3;
            }

            private static int GetBucketSize(int bucketIndex)
            {
                return 16 << bucketIndex;
            }

            private static int Log2(uint value)
            {
                int log = 0;
                while (value > 1)
                {
                    value >>= 1;
                    log++;
                }
                return log;
            }

            private sealed class Bucket : IDisposable
            {
                public int BufferSize { get; }

                private readonly int maxCount;
                private readonly ConcurrentStack<T[]> stack;
                private bool isDisposed;

                public Bucket(int bufferSize, int maxCount)
                {
                    this.BufferSize = bufferSize;
                    this.maxCount = maxCount;
                    this.stack = new ConcurrentStack<T[]>();
                }

                public T[] Rent()
                {
                    return stack.TryPop(out var array) ? array : new T[BufferSize];
                }

                public void Return(T[] array)
                {
                    if (array.Length != BufferSize)
                        return;

                    if (stack.Count >= maxCount)
                        return;

                    stack.Push(array);
                }

                public void Dispose()
                {
                    if (isDisposed) return;
                    isDisposed = true;

                    while (stack.TryPop(out _)) { }
                }
            }
        }
    }
}