using System;
using System.Collections.Concurrent;

namespace PoolKit.Collections
{
    /// <summary>
    /// 数组对象池
    /// </summary>
    public static class ArrayPool
    {
        /// <summary>
        /// 数组对象池的作用域
        /// </summary>
        /// <typeparam name="T">集合元素的类型</typeparam>
        public readonly struct PoolScope<T> : IDisposable
        {
            /// <summary>
            /// 数组
            /// </summary>
            public T[] Array => collection;
            private readonly T[] collection;

            internal PoolScope(T[] collection)
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

        private interface IArrayPool
        {
            void Clear();
        }

        private static readonly ConcurrentDictionary<Type, IArrayPool> pools = new ConcurrentDictionary<Type, IArrayPool>();
        private const int DEFAULT_MAX_ARRAYS_PER_BUCKET = 32;
        private const int MAX_BUCKET_INDEX = 20;
        private static int maxBucketIndex = MAX_BUCKET_INDEX;
        private static int maxArraysPerBucket = DEFAULT_MAX_ARRAYS_PER_BUCKET;
        private static bool configured = false;
        private static bool disposed = false;

        /// <summary>
        /// 配置全局参数（必须在首次使用前调用）
        /// </summary>
        /// <param name="maxArraySizeMB">最大缓存数组大小（MB），默认 4MB</param>
        /// <param name="maxArraysPerBucket">每个 Bucket 最大缓存数量，默认 32</param>
        /// <exception cref="InvalidOperationException">配置已被使用</exception>
        public static void Configure(int maxArraySizeMB = 4, int maxArraysPerBucket = 32)
        {
            if (configured)
                throw new InvalidOperationException("[PoolKit] ArrayPool already configured.");

            if (maxArraySizeMB <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxArraySizeMB), "[PoolKit] maxArraySizeMB must be greater than zero.");

            if (maxArraysPerBucket <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxArraysPerBucket), "[PoolKit] maxArraysPerBucket must be greater than zero.");

            ArrayPool.maxArraysPerBucket = maxArraysPerBucket;
            maxBucketIndex = 0;
            int targetSize = maxArraySizeMB * 1024 * 1024;
            int current = 16;
            while (current < targetSize && maxBucketIndex < 30)
            {
                current <<= 1;
                maxBucketIndex++;
            }
            configured = true;
        }

        /// <summary>
        /// 预热指定大小的数组池
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="minimumLength">数组最小长度</param>
        /// <param name="count">预热数量</param>
        public static void WarmUp<T>(int minimumLength, int count)
        {
            ThrowIfDisposed();
            if (minimumLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength), "[PoolKit] minimumLength cannot be less than zero.");

            if (count <= 0)
                return;

            var pool = GetPool<T>();
            pool.WarmUp(minimumLength, count);
        }

        /// <summary>
        /// 租借集合附带作用域实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="minimumLength">集合最小长度</param>
        /// <returns>作用域实例</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minimumLength"/> 不能小于 0。</exception>
        public static PoolScope<T> RentWithScope<T>(int minimumLength)
        {
            ThrowIfDisposed();
            if (minimumLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength), "[PoolKit] minimumLength cannot be less than zero.");

            if (minimumLength == 0)
                return new PoolScope<T>(Array.Empty<T>());

            var pool = GetPool<T>();
            return new PoolScope<T>(pool.Rent(minimumLength));
        }

        /// <summary>
        /// 租借集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="minimumLength">集合最小长度</param>
        /// <returns>集合实例</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minimumLength"/> 不能小于 0。</exception>
        public static T[] Rent<T>(int minimumLength)
        {
            ThrowIfDisposed();
            if (minimumLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength), "[PoolKit] minimumLength cannot be less than zero.");

            if (minimumLength == 0)
                return Array.Empty<T>();

            var pool = GetPool<T>();
            return pool.Rent(minimumLength);
        }

        /// <summary>
        /// 归还集合实例
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="array">集合实例</param>
        /// <param name="clearArray">是否清空集合</param>
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
        /// 清空对象池
        /// </summary>
        public static void Clear()
        {
            if (disposed) return;
            foreach (var pool in pools.Values)
                pool.Clear();
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
                if (pool is IDisposable disposable)
                    disposable.Dispose();
            }
            pools.Clear();
        }

        private static ArrayPoolHandler<T> GetPool<T>()
        {
            var type = typeof(T);
            if (!pools.TryGetValue(type, out var pool))
            {
                pool = new ArrayPoolHandler<T>(maxArraysPerBucket, maxBucketIndex);
                pools[type] = pool;
            }
            return (ArrayPoolHandler<T>)pool;
        }

        private static void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ArrayPool));
        }

        private sealed class ArrayPoolHandler<T> : IDisposable, IArrayPool
        {
            private readonly Bucket[] buckets;
            private bool disposed;

            public ArrayPoolHandler(int maxArraysPerBucket, int maxBucketIndex)
            {
                buckets = new Bucket[maxBucketIndex + 1];
                for (int i = 0; i < buckets.Length; i++)
                {
                    int size = GetBucketSize(i);
                    buckets[i] = new Bucket(size, maxArraysPerBucket);
                }
            }

            public void WarmUp(int minimumLength, int count)
            {
                int bucketIndex = GetBucketIndex(minimumLength);
                if (bucketIndex < 0 || bucketIndex >= buckets.Length)
                    return;

                var bucket = buckets[bucketIndex];
                for (int i = 0; i < count; i++)
                {
                    var array = new T[bucket.BufferSize];
                    bucket.Return(array);
                }
            }

            public T[] Rent(int minimumLength)
            {
                int bucketIndex = GetBucketIndex(minimumLength);
                if (bucketIndex < 0 || bucketIndex >= buckets.Length)
                    return new T[minimumLength];

                return buckets[bucketIndex].Rent();
            }

            public void Return(T[] array, bool clearArray)
            {
                int bucketIndex = GetBucketIndex(array.Length);
                if (bucketIndex < 0 || bucketIndex >= buckets.Length)
                    return;

                if (clearArray)
                    Array.Clear(array, 0, array.Length);

                buckets[bucketIndex].Return(array);
            }

            public void Clear()
            {
                if (disposed) return;
                foreach (var bucket in buckets)
                {
                    bucket.Clear();
                }
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                foreach (var bucket in buckets)
                {
                    bucket.Dispose();
                }
            }

            private static int GetBucketIndex(int length)
            {
                if (length <= 0)
                    return -1;

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

                private readonly T[][] buffers;
                private int index;
                private bool disposed;

                public Bucket(int bufferSize, int maxCount)
                {
                    BufferSize = bufferSize;
                    buffers = new T[maxCount][];
                    index = 0;
                }

                public T[] Rent()
                {
                    if (index < buffers.Length)
                    {
                        var buffer = buffers[index];
                        buffers[index++] = null;
                        return buffer ?? new T[BufferSize];
                    }
                    return new T[BufferSize];
                }

                public void Return(T[] array)
                {
                    if (array == null || array.Length != BufferSize)
                        return;

                    if (index > 0)
                    {
                        buffers[--index] = array;
                    }
                }

                public void Clear()
                {
                    if (disposed) return;
                    Array.Clear(buffers, 0, buffers.Length);
                    index = 0;
                }

                public void Dispose()
                {
                    if (disposed) return;
                    disposed = true;
                    Array.Clear(buffers, 0, buffers.Length);
                    index = 0;
                }
            }
        }
    }
}