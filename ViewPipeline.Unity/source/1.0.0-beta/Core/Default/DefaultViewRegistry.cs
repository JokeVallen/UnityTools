using System;
using System.Collections;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 默认的视图活跃状态注册表
    /// </summary>
    internal sealed class DefaultViewRegistry : IViewRegistry
    {
        /// <inheritdoc/>
        public int Count => registrySet.Count;

        private readonly HashSet<IView> registrySet = new HashSet<IView>();

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">当传入的 <paramref name="view"/> 为空时抛出。</exception>
        public void Register(IView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            registrySet.Add(view);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">当传入的 <paramref name="view"/> 为空时抛出。</exception>
        public void Unregister(IView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            registrySet.Remove(view);
        }

        /// <inheritdoc/>
        public IEnumerator<IView> GetEnumerator()
        {
            return registrySet.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
