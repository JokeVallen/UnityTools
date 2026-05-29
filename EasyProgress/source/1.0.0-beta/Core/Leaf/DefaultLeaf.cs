using System;

namespace EasyProgress.Core
{
    /// <summary>
    /// 默认叶子节点
    /// </summary>
    /// <remarks>
    /// <para>进度值范围为 [0, 1]，使用 double 类型。</para>
    /// <para>线程安全：使用锁保护内部状态，事件在锁外触发。</para>
    /// <para>示例：</para>
    /// <code>
    /// var leaf = new DefaultLeaf();
    /// leaf.OnProgressChanged += (node, p) => Console.WriteLine(p);
    /// leaf.Report(0.5);
    /// leaf.Complete();
    /// </code>
    /// </remarks>
    public sealed class DefaultLeaf : IProgressLeaf<double>, IResettable
    {
        private readonly object @lock = new object();
        private double progress;
        private event Action<IProgressNode<double>, double> onProgressChanged;

        /// <inheritdoc/>
        public double Progress
        {
            get { lock (@lock) return progress; }
        }

        /// <inheritdoc/>
        public event Action<IProgressNode<double>, double> OnProgressChanged
        {
            add { lock (@lock) onProgressChanged += value; }
            remove { lock (@lock) onProgressChanged -= value; }
        }

        /// <inheritdoc/>
        /// <remarks>值会被限制在 [0,1] 范围内，若与当前进度差异小于 1e-9 则忽略。</remarks>
        public void Report(double value)
        {
            double newProgress = Math.Max(0, Math.Min(1, value));
            Action<IProgressNode<double>, double> handler = null;
            lock (@lock)
            {
                if (Math.Abs(progress - newProgress) < 1e-9) return;
                progress = newProgress;
                handler = onProgressChanged;
            }
            handler?.Invoke(this, newProgress);
        }

        /// <inheritdoc/>
        /// <remarks>等同于 Report(1.0)</remarks>
        public void Complete() => Report(1.0);

        /// <inheritdoc/>
        public void Reset()
        {
            lock (@lock)
            {
                progress = 0;
                onProgressChanged = null;
            }
        }
    }
}
