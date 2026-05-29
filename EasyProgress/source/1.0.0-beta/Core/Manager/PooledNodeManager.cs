using System;
using System.Collections.Concurrent;

namespace EasyProgress.Core
{
    /// <summary>
    /// 池化节点管理器
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <typeparam name="TNode">节点类型</typeparam>
    /// <remarks>
    /// <para>通用的对象池实现，管理实现了 <see cref="IResettable"/> 的节点复用。</para>
    /// <para>内部使用 <see cref="ConcurrentStack{T}"/> 保证线程安全。</para>
    /// <para>示例：</para>
    /// <code>
    /// var pool = new PooledNodeManager&lt;double, DefaultLeaf&gt;( _ => new DefaultLeaf());
    /// var leaf = pool.Acquire();
    /// // 使用 leaf
    /// pool.Release(leaf);
    /// </code>
    /// </remarks>
    internal sealed class PooledNodeManager<T, TNode> where TNode : class, IProgressNode<T>, IResettable
    {
        public int Capacity { get => capacity; set => capacity = Math.Max(0, value); }
        private readonly ConcurrentStack<TNode> pool = new ConcurrentStack<TNode>();
        private readonly Func<object, TNode> factory;
        private int capacity;

        public PooledNodeManager(Func<object, TNode> factory) : this(1024, factory) { }

        /// <param name="capacity">容量</param>
        /// <param name="factory">工厂委托</param>
        /// <remarks>factory 接受一个 object 参数作为用户数据，可用于创建时需要额外信息的场景。</remarks>
        public PooledNodeManager(int capacity, Func<object, TNode> factory) 
        {
            if (factory == null) throw new System.ArgumentNullException(nameof(factory));
            this.capacity = Math.Max(0, capacity);
            this.factory = factory;
        }

        /// <summary>获取节点</summary>
        public TNode Acquire()
        {
            return Acquire(null);
        }

        /// <summary>获取节点</summary>
        /// <param name="userData">用户数据</param>
        /// <remarks>如果池中有空闲节点则直接返回，否则调用工厂创建新节点。</remarks>
        public TNode Acquire(object userData) 
        {
            if (pool.TryPop(out var node)) return node;
            return factory(userData);
        }

        /// <summary>归还节点</summary>
        /// <param name="node">节点</param>
        /// <remarks>归还前会调用节点的 <see cref="IResettable.Reset"/> 方法重置状态。</remarks>
        public void Release(TNode node)
        {
            if(node == null) throw new System.ArgumentNullException(nameof(node));
            node.Reset();
            pool.Push(node);
        }

        /// <summary>
        /// 收缩池容量
        /// </summary>
        public bool TryTrim() 
        {
            int count = pool.Count;
            while (count > capacity)
            {
                pool.TryPop(out _);
                count--;
            }

            return pool.Count <= capacity;
        }
    }
}
