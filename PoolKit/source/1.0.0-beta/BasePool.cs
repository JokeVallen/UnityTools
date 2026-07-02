using System;
using System.Collections.Generic;

namespace PoolKit
{
    /// <summary>
    /// 对象池基类
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public abstract class BasePool<T>
    {
        /// <summary>
        /// 对象池所生产对象的总数量
        /// </summary>
        public int TotalCount { get; protected set; }

        /// <summary>
        /// 对象池当前空闲对象的数量
        /// </summary>
        public int FreeCount => Pool.Count;

        /// <summary>
        /// 是否为固定容量的对象池
        /// <para>默认值: false</para>
        /// </summary>
        public bool IsFixed => isFixed;
        private readonly bool isFixed;

        /// <summary>
        /// 对象池容量
        /// <para>默认值: 100</para>
        /// </summary>
        public int Capacity => capacity;
        private readonly int capacity;

        /// <summary>
        /// 对象创建逻辑
        /// <para>提示：用来自定义对象的创建逻辑</para>
        /// </summary>
        public Func<T> OverrideCreate;

        /// <summary>
        /// 对象重置逻辑
        /// <para>提示：用来自定义对象的重置逻辑</para>
        /// </summary>
        public Action<T> OverrideReset;

        /// <summary>
        /// 对象销毁逻辑
        /// <para>提示：用来自定义对象的销毁逻辑</para>
        /// </summary>
        public Action<T> OverrideDestroy;

        /// <summary>
        /// 池对象访问器
        /// </summary>
        protected Stack<T> Pool => pool;
        private readonly Stack<T> pool;

        /// <summary>
        /// 对象类型名称
        /// </summary>
        protected static string ItemTypeName => itemTypeName;
        private static readonly string itemTypeName = typeof(T).Name;

        /// <summary></summary>
        protected BasePool() : this(100, false) { }

        /// <summary></summary>
        /// <param name="capacity">容量</param>
        protected BasePool(int capacity) : this(capacity, false) { }

        /// <summary></summary>
        /// <param name="capacity">容量</param>
        /// <param name="isFixed">是否固定容量</param>
        protected BasePool(int capacity, bool isFixed)
        {
            if (capacity <= 0) throw new ArgumentException("[PoolKit] The pool capacity must be greater than zero.");
            pool = new Stack<T>(capacity);
            this.capacity = capacity;
            this.isFixed = isFixed;
        }

        /// <summary>
        /// 重置对象
        /// </summary>
        protected abstract void Reset(T item);

        /// <summary>
        /// 创建对象
        /// </summary>
        protected abstract T Create();

        /// <summary>
        /// 获取对象
        /// </summary>
        public abstract T Get();

        /// <summary>
        /// 释放对象
        /// </summary>
        public abstract void Release(T item);

        /// <summary>
        /// 清空对象池
        /// </summary>
        public abstract void Clear();
    }
}