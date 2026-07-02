using System;
using UnityEngine;

namespace PoolKit.Unity
{
    /// <summary>
    /// <see cref="GameObject"/> 对象池
    /// </summary>
    public class GameObjectPool : UnityObjectPool<GameObject>
    {
        /// <inheritdoc/>
        public GameObjectPool() : this(100, false) { }
        /// <inheritdoc/>
        public GameObjectPool(int capacity) : this(capacity, false) { }
        /// <inheritdoc/>
        public GameObjectPool(int capacity, bool isFixed) : base(capacity, isFixed) { }
        /// <inheritdoc/>
        public GameObjectPool(UnityObjectPoolSettings<GameObject> settings) : base(settings) { }

        /// <inheritdoc/>
        public override GameObject Get()
        {
            if (!activeWhenGet) return base.Get();
            else
            {
                GameObject item = base.Get();
                item.SetActive(true);
                return item;
            }
        }

        /// <inheritdoc/>
        protected override void Reset(GameObject item)
        {
            if (item == null) throw new InvalidOperationException("[PoolKit] The item to be reset is null.");
            if (OverrideReset != null) OverrideReset(item);
            else item.SetActive(false);
        }

        /// <inheritdoc/>
        protected override GameObject Create()
        {
            if (IsFixed && TotalCount == Capacity)
                throw new InvalidOperationException("[PoolKit] The pool has reached the limit of capacity.");

            GameObject item;
            if (OverrideCreate != null) item = OverrideCreate();
            else
            {
                if (original == null) item = new GameObject();
                else item = UnityEngine.Object.Instantiate(original);

                if (item != null)
                {
                    if (!string.IsNullOrEmpty(defaultName)) 
                        item.name = defaultName;
                    item.transform.SetParent(container.transform);
                    item.SetActive(false);
                }
            }

            if (item == null) throw new InvalidOperationException("[PoolKit] The created item is null.");
            return item;
        }
    }
}