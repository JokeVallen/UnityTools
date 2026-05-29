using System;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 手动刷新组合节点（无权重）
    /// </summary>
    /// <remarks>
    /// <para>子节点进度变化时仅标记脏，需要外部主动调用 <see cref="Refresh"/> 方法才重新计算总进度并触发事件。</para>
    /// <para>适用于需要合并高频更新的场景（例如 Unity 中每帧调用一次 Refresh）。</para>
    /// <para>线程安全：使用锁保护内部状态，事件在锁外触发。</para>
    /// <para>示例：</para>
    /// <code>
    /// var rule = WeightedAverageRule.Create();
    /// var composite = new ManualComposite(rule);
    /// composite.AddChild(leaf);
    /// // 在 Update 中每帧调用
    /// void Update() { composite.Refresh(); }
    /// </code>
    /// </remarks>
    public sealed class ManualComposite : IProgressComposite<double>, IResettable, IManualRefreshNode
    {
        /// <inheritdoc/>
        /// <remarks>缓存值，需调用 <see cref="Refresh"/> 更新。</remarks>
        public double Progress
        {
            get { lock (@lock) return cachedProgress; }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IProgressNode<double>> Children
        {
            get
            {
                if (readOnlyChildren == null)
                {
                    lock (@lock)
                    {
                        if (readOnlyChildren == null)
                            readOnlyChildren = children.AsReadOnly();
                    }
                }
                return readOnlyChildren;
            }
        }

        /// <inheritdoc/>
        public ICompositionRule<double> Rule
        {
            get { lock (@lock) return rule; }
        }

        /// <inheritdoc/>
        public event Action<IProgressNode<double>, double> OnProgressChanged
        {
            add { lock (@lock) onProgressChanged += value; }
            remove { lock (@lock) onProgressChanged -= value; }
        }

        private readonly object @lock = new object();
        private readonly List<IProgressNode<double>> children = new List<IProgressNode<double>>();
        private IReadOnlyList<IProgressNode<double>> readOnlyChildren;
        private ICompositionRule<double> rule;
        private double cachedProgress;
        private bool dirty = true;
        private event Action<IProgressNode<double>, double> onProgressChanged;

        /// <param name="rule">复合规则</param>
        public ManualComposite(ICompositionRule<double> rule)
        {
            if (rule == null) throw new System.ArgumentNullException(nameof(rule));
            this.rule = rule;
        }

        /// <inheritdoc/>
        public void AddChild(IProgressNode<double> node)
        {
            if (node == null) return;
            lock (@lock)
            {
                children.Add(node);
                node.OnProgressChanged -= OnChildProgressChanged;
                node.OnProgressChanged += OnChildProgressChanged;
            }
            MarkDirty();
        }

        /// <inheritdoc/>
        public bool RemoveChild(IProgressNode<double> node)
        {
            if (node == null) return false;
            bool removed;
            lock (@lock)
            {
                removed = children.Remove(node);
                if (removed) node.OnProgressChanged -= OnChildProgressChanged;
            }
            if (removed) MarkDirty();
            return removed;
        }

        /// <inheritdoc/>
        public void SetRule(ICompositionRule<double> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            lock (@lock) this.rule = rule;
            MarkDirty();
        }

        /// <inheritdoc/>
        public void Refresh()
        {
            ICompositionRule<double> rule;
            List<IProgressNode<double>> childrenCopy = null;
            try
            {
                lock (@lock)
                {
                    if (!dirty) return;
                    dirty = false;
                    rule = this.rule;
                    int count = children.Count;
                    childrenCopy = ListPool.Rent<IProgressNode<double>>();
                    childrenCopy.AddRange(children);
                }

                double newProgress = rule.Compute(childrenCopy.AsReadOnly(), _ => 1f);
                double oldProgress;
                Action<IProgressNode<double>, double> handler = null;
                lock (@lock)
                {
                    oldProgress = cachedProgress;
                    if (Math.Abs(oldProgress - newProgress) < 1e-9) return;
                    cachedProgress = newProgress;
                    handler = onProgressChanged;
                }
                handler?.Invoke(this, newProgress);
            }
            finally 
            { 
                if(childrenCopy != null)
                    ListPool.Return(childrenCopy);
            }
        }

        /// <inheritdoc/>
        public void Reset()
        {
            lock (@lock)
            {
                foreach (var child in children)
                    child.OnProgressChanged -= OnChildProgressChanged;
                children.Clear();
                rule = WeightedAverageRule.Create();
                cachedProgress = 0;
                dirty = true;
                onProgressChanged = null;
                readOnlyChildren = null;
            }
        }

        private void OnChildProgressChanged(IProgressNode<double> node, double progress)
        {
            MarkDirty();
        }

        private void MarkDirty()
        {
            lock (@lock) 
                dirty = true;
        }
    }
}
