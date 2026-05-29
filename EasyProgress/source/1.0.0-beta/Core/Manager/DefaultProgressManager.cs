using System;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 默认进度管理器工厂
    /// </summary>
    /// <remarks>
    /// <para>提供创建 <see cref="DefaultProgressManager{T}"/> 实例的便捷方法。</para>
    /// </remarks>
    public static class DefaultProgressManager 
    {
        /// <summary>创建默认的进度管理器实例（进度值类型为 double）</summary>
        /// <remarks>
        /// <para>叶子节点使用 <see cref="DefaultLeaf"/>，组合节点使用 <see cref="WeightedRealtimeComposite"/>。</para>
        /// <para>组合节点的默认规则为 <see cref="WeightedAverageRule"/>，若传入规则为 null 则自动使用该默认规则。</para>
        /// </remarks>
        public static DefaultProgressManager<double> CreateDefault()
        {
            return new DefaultProgressManager<double>(
                leafFactory: () => new DefaultLeaf(),
                compositeFactory: rule => new WeightedRealtimeComposite(rule ?? WeightedAverageRule.Create())
            );
        }
    }

    /// <summary>
    /// 默认进度管理器
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>实现 <see cref="IProgressManager{T}"/>，使用 <see cref="PooledNodeManager{T, TNode}"/> 对叶子节点和组合节点进行池化复用。</para>
    /// <para>节点通过包装器适配，内部调用 <see cref="IResettable.Reset"/> 重置状态。</para>
    /// <para>线程安全。</para>
    /// </remarks>
    public sealed class DefaultProgressManager<T> : IProgressManager<T>
    {
        private class LeafWrapper : IProgressLeaf<T>, IResettable
        {
            public IProgressLeaf<T> Leaf => leaf;
            private readonly IProgressLeaf<T> leaf;
            public LeafWrapper(IProgressLeaf<T> leaf) { this.leaf = leaf; }

            public T Progress => leaf.Progress;

            public event Action<IProgressNode<T>, T> OnProgressChanged
            {
                add => leaf.OnProgressChanged += value;
                remove => leaf.OnProgressChanged -= value;
            }

            public void Complete()
            {
                leaf.Complete();
            }

            public void Report(T value)
            {
                leaf.Report(value);
            }

            public void Reset()
            {
                if (!(leaf is IResettable resettable)) 
                    throw new InvalidOperationException($"The leaf '{leaf}' doesn't implement the interface '{typeof(IResettable)}', therefore it cannot be pooled.");
                resettable.Reset();
            }
        }

        private class CompositeWrapper : IProgressComposite<T>, IResettable 
        {
            public IProgressComposite<T> Composite => composite;
            private readonly IProgressComposite<T> composite;
            public CompositeWrapper(IProgressComposite<T> composite) { this.composite = composite; }

            public ICompositionRule<T> Rule => composite.Rule;

            public IReadOnlyCollection<IProgressNode<T>> Children => composite.Children;

            public T Progress => composite.Progress;

            public event Action<IProgressNode<T>, T> OnProgressChanged
            {
                add => composite.OnProgressChanged += value;
                remove => composite.OnProgressChanged -= value;
            }

            public void AddChild(IProgressNode<T> node)
            {
                composite.AddChild(node);
            }

            public bool RemoveChild(IProgressNode<T> node)
            {
                return composite.RemoveChild(node);
            }

            public void Reset()
            {
                if (!(composite is IResettable resettable))
                    throw new InvalidOperationException($"The composite '{composite}' doesn't implement the interface '{typeof(IResettable)}', therefore it cannot be pooled.");
                resettable.Reset();
            }

            public void SetRule(ICompositionRule<T> rule)
            {
                composite.SetRule(rule);
            }
        }

        private readonly PooledNodeManager<T, LeafWrapper> leafPool;
        private readonly PooledNodeManager<T, CompositeWrapper> compositePool;
        private readonly Func<IProgressLeaf<T>> leafFactory;
        private readonly Func<ICompositionRule<T>, IProgressComposite<T>> compositeFactory;

        /// <param name="leafFactory">叶子节点工厂方法</param>
        /// <param name="compositeFactory">复合节点工厂方法</param>
        public DefaultProgressManager(Func<IProgressLeaf<T>> leafFactory, Func<ICompositionRule<T>, IProgressComposite<T>> compositeFactory) : this(leafFactory, compositeFactory, 1024, 1024) { }

        /// <param name="leafFactory">叶子节点工厂方法</param>
        /// <param name="compositeFactory">复合节点工厂方法</param>
        /// <param name="leafPoolCapacity">叶子节点池容量</param>
        /// <param name="compositePoolCapacity">复合节点池容量</param>
        public DefaultProgressManager(Func<IProgressLeaf<T>> leafFactory, Func<ICompositionRule<T>, IProgressComposite<T>> compositeFactory,int leafPoolCapacity, int compositePoolCapacity) 
        {
            if (leafFactory == null) throw new System.ArgumentNullException(nameof(leafFactory));
            if (compositeFactory == null) throw new System.ArgumentNullException(nameof(compositeFactory));
            this.leafFactory = leafFactory;
            this.compositeFactory = compositeFactory;
            leafPool = new PooledNodeManager<T, LeafWrapper>(leafPoolCapacity, CreateLeafWrapper);
            compositePool = new PooledNodeManager<T, CompositeWrapper>(compositePoolCapacity, CreateCompositeWrapper);
        }

        /// <inheritdoc/>
        public IProgressComposite<T> AcquireComposite(ICompositionRule<T> rule)
        {
            var composite = compositePool.Acquire(rule);
            composite.SetRule(rule);
            return composite.Composite;
        }

        /// <inheritdoc/>
        public IProgressLeaf<T> AcquireLeaf()
        {
            return leafPool.Acquire(null).Leaf;
        }

        /// <inheritdoc/>
        public void ReleaseComposite(IProgressComposite<T> composite)
        {
            if (composite == null) return;
            compositePool.Release(new CompositeWrapper(composite));
        }

        /// <inheritdoc/>
        public void ReleaseLeaf(IProgressLeaf<T> leaf)
        {
            if (leaf == null) return;
            leafPool.Release(new LeafWrapper(leaf));
        }

        private CompositeWrapper CreateCompositeWrapper(object userData)
        {
            return new CompositeWrapper(compositeFactory(userData as ICompositionRule<T>));
        }

        private LeafWrapper CreateLeafWrapper(object userData)
        {
            return new LeafWrapper(leafFactory());
        }
    }
}
