using System.Collections.Generic;

namespace PoolKit
{
    /// <summary>
    /// 集合对象池
    /// </summary>
    /// <typeparam name="T1">集合元素类型</typeparam>
    /// <typeparam name="T2">集合类型</typeparam>
    public class CollectionPool<T1, T2> : ClassPool<T2> where T2 : class, IEnumerable<T1>, new()
    {
        /// <inheritdoc/>
        public CollectionPool() { }
        /// <inheritdoc/>
        public CollectionPool(int capacity) : base(capacity) { }
        /// <inheritdoc/>
        public CollectionPool(int capacity, bool isFixed) : base(capacity, isFixed) { }
    }
}