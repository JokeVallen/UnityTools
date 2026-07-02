using System;
using UnityEngine;

namespace PoolKit.Unity
{
    /// <summary>
    /// Component 类型对象池
    /// </summary>
    /// <typeparam name="T">具体的 Component 类型</typeparam>
    public class ComponentPool<T> : UnityObjectPool<T> where T : Component
    {
        private static readonly bool IsBehaviour = typeof(Behaviour).IsAssignableFrom(typeof(T));

        /// <inheritdoc/>
        public ComponentPool() : this(100, false) { }
        /// <inheritdoc/>
        public ComponentPool(int capacity) : this(capacity, false) { }
        /// <inheritdoc/>
        public ComponentPool(int capacity, bool isFixed) : base(capacity, isFixed) { }
        /// <inheritdoc/>
        public ComponentPool(UnityObjectPoolSettings<T> settings) : base(settings) { }

        /// <inheritdoc/>
        public override T Get()
        {
            if (!activeWhenGet) return base.Get();
            else
            {
                T item = base.Get();
                if (IsBehaviour) ((Behaviour)(UnityEngine.Object)item).enabled = true;
                return item;
            }
        }

        /// <inheritdoc/>
        protected override void Reset(T item)
        {
            if (item == null) throw new InvalidOperationException("[PoolKit] The item to be reset is null.");
            if (OverrideReset != null) OverrideReset(item);
            else if(IsBehaviour)((Behaviour)(UnityEngine.Object)item).enabled = false;
        }

        /// <inheritdoc/>
        protected override T Create()
        {
            if (IsFixed && TotalCount == Capacity)
                throw new InvalidOperationException("[PoolKit] The pool has reached the limit of capacity.");

            T item;
            if (OverrideCreate != null) item = OverrideCreate();
            else
            {
                if (original == null) item = container.AddComponent<T>();
                else item = UnityEngine.Object.Instantiate(original);

                if (item != null)
                {
                    if (!string.IsNullOrEmpty(defaultName)) 
                        item.name = defaultName;
                    if (IsBehaviour) ((Behaviour)(UnityEngine.Object)item).enabled = false;
                }
            }

            if (item == null) throw new InvalidOperationException("[PoolKit] The created item is null.");
            return item;
        }
    }
}