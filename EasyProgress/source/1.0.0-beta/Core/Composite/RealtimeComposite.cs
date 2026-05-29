using System;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 实时组合节点（无权重）
    /// </summary>
    /// <remarks>
    /// <para>子节点进度变化时立即重新计算总进度并触发事件。</para>
    /// <para>所有子节点权重视为 1（等权）。</para>
    /// <para>线程安全：使用锁保护内部列表，事件在锁外触发。</para>
    /// <para>示例：</para>
    /// <code>
    /// var rule = WeightedAverageRule.Create();
    /// var composite = new RealtimeComposite(rule);
    /// composite.AddChild(leaf1);
    /// composite.AddChild(leaf2);
    /// composite.OnProgressChanged += (n, p) => Console.WriteLine(p);
    /// </code>
    /// </remarks>
    public sealed class RealtimeComposite : IProgressComposite<double>, IResettable
    {
        /// <inheritdoc/>
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
        private event Action<IProgressNode<double>, double> onProgressChanged;

        /// <param name="rule">复合规则</param>
        public RealtimeComposite(ICompositionRule<double> rule)
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
            RecalcProgress();
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
            if (removed) RecalcProgress();
            return removed;
        }

        /// <inheritdoc/>
        public void SetRule(ICompositionRule<double> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            lock (@lock) this.rule = rule;
            RecalcProgress();
        }

        private void OnChildProgressChanged(IProgressNode<double> node, double progress)
        {
            RecalcProgress();
        }

        private void RecalcProgress()
        {
            ICompositionRule<double> rule;
            List<IProgressNode<double>> childrenCopy = null;
            try
            {
                lock (@lock)
                {
                    rule = this.rule;
                    int count = children.Count;
                    childrenCopy = ListPool.Rent<IProgressNode<double>>();
                    childrenCopy.AddRange(children);
                }

                double newProgress = rule.Compute(childrenCopy, _ => 1f);
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

        public void Reset()
        {
            lock (@lock)
            {
                foreach (var child in children)
                    child.OnProgressChanged -= OnChildProgressChanged;
                children.Clear();
                rule = WeightedAverageRule.Create();
                cachedProgress = 0;
                onProgressChanged = null;
                readOnlyChildren = null;
            }
        }
    }
}
