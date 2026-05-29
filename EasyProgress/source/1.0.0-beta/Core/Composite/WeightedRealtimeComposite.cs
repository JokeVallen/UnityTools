using System;
using System.Collections.Generic;

namespace EasyProgress.Core
{
    /// <summary>
    /// 加权实时组合节点
    /// </summary>
    /// <remarks>
    /// <para>支持为每个子节点设置权重，子节点进度变化时立即重新计算总进度并触发事件。</para>
    /// <para>线程安全：使用锁保护内部状态，事件在锁外触发。</para>
    /// <para>示例：</para>
    /// <code>
    /// var rule = WeightedAverageRule.Create();
    /// var composite = new WeightedRealtimeComposite(rule);
    /// composite.AddChild(leaf1, 0.3f);
    /// composite.AddChild(leaf2, 0.7f);
    /// composite.OnProgressChanged += (n, p) => Console.WriteLine(p);
    /// </code>
    /// </remarks>
    public sealed class WeightedRealtimeComposite : IProgressComposite<double>, IWeightedProgressComposite<double>, IResettable
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
                        if(readOnlyChildren == null)
                            readOnlyChildren = children.AsReadOnly();
                    }
                }
                return readOnlyChildren;
            }
        }

        /// <inheritdoc/>
        public ICompositionRule<double> Rule
        {
            get
            {
                lock (@lock) 
                    return rule;
            }
        }

        /// <inheritdoc/>
        public event Action<IProgressNode<double>, double> OnProgressChanged
        {
            add { lock (@lock) onProgressChanged += value; }
            remove { lock (@lock) onProgressChanged -= value; }
        }

        private readonly object @lock = new object();
        private readonly List<IProgressNode<double>> children = new List<IProgressNode<double>>();
        private readonly Dictionary<IProgressNode<double>, float> weights = new Dictionary<IProgressNode<double>, float>();
        private IReadOnlyList<IProgressNode<double>> readOnlyChildren;
        private ICompositionRule<double> rule;
        private double cachedProgress;
        private event Action<IProgressNode<double>, double> onProgressChanged;

        /// <param name="rule">复合规则</param>
        public WeightedRealtimeComposite(ICompositionRule<double> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            this.rule = rule;
        }

        /// <summary>添加子节点（权重默认为1）</summary>
        /// <param name="node">子节点</param>
        public void AddChild(IProgressNode<double> node)
        {
            AddChild(node, 1f);
        }

        /// <inheritdoc/>
        public void AddChild(IProgressNode<double> node, float weight)
        {
            if (node == null) return;
            lock (@lock)
            {
                children.Add(node);
                weights[node] = weight;
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
                if (removed)
                {
                    weights.Remove(node);
                    node.OnProgressChanged -= OnChildProgressChanged;
                }
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

        /// <inheritdoc/>
        public void SetWeight(IProgressNode<double> node, float weight)
        {
            if (node == null) return;
            lock (@lock)
            {
                if (!weights.ContainsKey(node)) return;
                weights[node] = weight;
            }
            RecalcProgress();
        }

        /// <inheritdoc/>
        public float GetWeight(IProgressNode<double> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            lock (@lock) return weights.TryGetValue(node, out var w) ? w : 1f;
        }

        private void OnChildProgressChanged(IProgressNode<double> node, double progress)
        {
            RecalcProgress();
        }

        private void RecalcProgress()
        {
            ICompositionRule<double> rule;
            List<IProgressNode<double>> childrenCopy = null;
            Dictionary<IProgressNode<double>, float> weightsCopy = null;
            Func<IProgressNode<double>, float> getWeightDelegate;
            try 
            {
                lock (@lock)
                {
                    rule = this.rule;
                    int count = children.Count;
                    childrenCopy = ListPool.Rent<IProgressNode<double>>();
                    childrenCopy.AddRange(children);
                    weightsCopy = DictionaryPool.Rent<IProgressNode<double>, float>();
                    foreach (var kv in weights)
                        weightsCopy[kv.Key] = kv.Value;
                }

                getWeightDelegate = node => weightsCopy.TryGetValue(node, out var w) ? w : 1f;
                double newProgress = rule.Compute(childrenCopy, getWeightDelegate);
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
                if (childrenCopy != null)
                    ListPool.Return(childrenCopy);
                if (weightsCopy != null)
                    DictionaryPool.Return(weightsCopy);
            }
        }

        public void Reset()
        {
            lock (@lock)
            {
                foreach (var child in children)
                    child.OnProgressChanged -= OnChildProgressChanged;
                children.Clear();
                weights.Clear();
                rule = WeightedAverageRule.Create();
                cachedProgress = 0;
                onProgressChanged = null;
                readOnlyChildren = null;
            }
        }
    }
}
