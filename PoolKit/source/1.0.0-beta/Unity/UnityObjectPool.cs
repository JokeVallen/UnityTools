using System;
using UnityEngine;

namespace PoolKit.Unity
{
    /// <summary>
    /// Unity 对象池
    /// </summary>
    /// <typeparam name="T">Unity 对象类型</typeparam>
    public class UnityObjectPool<T> : BasePool<T> where T : UnityEngine.Object
    {
        /// <summary>容器对象</summary>
        protected readonly GameObject container;
        /// <summary>对象原型</summary>
        protected readonly T original;
        /// <summary>对象默认名称</summary>
        protected readonly string defaultName;
        /// <summary>获取时是否激活</summary>
        protected readonly bool activeWhenGet;

        /// <inheritdoc/>
        public UnityObjectPool() : this(100, false) { }
        /// <inheritdoc/>
        public UnityObjectPool(int capacity) : this(capacity, false) { }
        /// <inheritdoc/>
        public UnityObjectPool(int capacity, bool isFixed) : base(capacity, isFixed)
        {
            container = CreateContainer(true);
            activeWhenGet = true;
        }

        /// <summary></summary>
        /// <param name="settings">对象池设置</param>
        public UnityObjectPool(UnityObjectPoolSettings<T> settings) : base(settings == null ? 100 : settings.capacity, settings != null && settings.isFixed)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            container = settings.container == null ? CreateContainer(settings.isPersistant) : settings.container;
            original = settings.original;
            defaultName = settings.defaultName;
            activeWhenGet = settings.activeWhenGet;

            if (settings.isPersistant) UnityEngine.Object.DontDestroyOnLoad(container);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            TotalCount -= Pool.Count;
            while (Pool.Count > 0)
            {
                var item = Pool.Pop();
                if (OverrideDestroy != null) OverrideDestroy(item);
                else UnityEngine.Object.Destroy(item);
            }
        }

        protected override void Reset(T item)
        {
            if (OverrideReset != null)
                OverrideReset(item);
        }

        protected override T Create()
        {
            if (IsFixed && TotalCount == Capacity)
                throw new InvalidOperationException("[PoolKit] The pool has reached the limit of capacity.");

            T item;
            if (OverrideCreate != null) item = OverrideCreate();
            else
            {
                item = original != null ? UnityEngine.Object.Instantiate(original) : null;
                if (item != null) 
                {
                    if (!string.IsNullOrEmpty(defaultName))
                        item.name = defaultName;
                }
            }

            if (item == null) throw new InvalidOperationException("[PoolKit] The created item is null.");
            return item;
        }

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

        public override void Release(T item)
        {
            if (item == null) return;
            Reset(item);
            Pool.Push(item);
        }

        private GameObject CreateContainer(bool persistent)
        {
            var container = new GameObject($"{ItemTypeName}Pool");
            if(persistent) UnityEngine.Object.DontDestroyOnLoad(container);
            return container;
        }
    }
}