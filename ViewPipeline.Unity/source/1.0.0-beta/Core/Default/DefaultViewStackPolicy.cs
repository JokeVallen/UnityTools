using System;
using System.Collections;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 默认的视图层级导航与堆栈管理策略
    /// </summary>
    internal sealed class DefaultViewStackPolicy : IViewStackPolicy
    {
        /// <inheritdoc/>
        public int Count => stackList.Count;

        private readonly LinkedList<IView> stackList = new LinkedList<IView>();
        private readonly Dictionary<IView, LinkedListNode<IView>> nodeMap = new Dictionary<IView, LinkedListNode<IView>>();

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">当传入的 <paramref name="view"/> 为空时抛出。</exception>
        public void Push(IView view)
        {
            if (nodeMap.ContainsKey(view)) return;
            var node = stackList.AddLast(view);
            nodeMap[view] = node;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">当传入的 <paramref name="view"/> 为空时抛出。</exception>
        public void Pop(IView view)
        {
            if (nodeMap.TryGetValue(view, out var node))
            {
                stackList.Remove(node);
                nodeMap.Remove(view);
            }
        }

        /// <inheritdoc/>
        public IEnumerator<IView> GetEnumerator()
        {
            return stackList.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
