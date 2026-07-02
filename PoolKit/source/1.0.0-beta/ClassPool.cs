using System;

namespace PoolKit
{
    /// <summary>
    /// Class 类型对象池
    /// </summary>
    /// <typeparam name="T">具体的 Class 类型</typeparam>
    public class ClassPool<T> : BasePool<T> where T : class, new()
    {
        /// <inheritdoc/>
        public ClassPool() : this(100, false) { }
        /// <inheritdoc/>
        public ClassPool(int capacity) : this(capacity, false) { }
        /// <inheritdoc/>
        public ClassPool(int capacity, bool isFixed) : base(capacity, isFixed) { }

        /// <inheritdoc/>
        public override void Clear()
        {
            TotalCount -= Pool.Count;
            while (Pool.Count > 0)
            {
                var pop = Pool.Pop();
                if (OverrideDestroy != null) OverrideDestroy(pop);
            }
        }

        /// <inheritdoc/>
        public override T Get()
        {
            T item;
            if (FreeCount > 0) item = Pool.Pop();
            else
            {
                item = Create();
                TotalCount++;
            }
            return item;
        }

        /// <inheritdoc/>
        public override void Release(T item)
        {
            if (item == null) return;
            Reset(item);
            Pool.Push(item);
        }

        /// <inheritdoc/>
        protected override void Reset(T item)
        {
            if (OverrideReset != null) 
                OverrideReset(item);
        }

        /// <inheritdoc/>
        protected override T Create()
        {
            if (IsFixed && TotalCount == Capacity)
                throw new InvalidOperationException("[PoolKit] The pool has reached the limit of capacity.");

            T item;
            if (OverrideCreate != null) item = OverrideCreate();
            else item = new T();

            if (item == null) throw new InvalidOperationException("[PoolKit] The created item is null.");
            return item;
        }
    }
}