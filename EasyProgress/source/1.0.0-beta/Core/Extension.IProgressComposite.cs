using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyProgress.Core
{
    /// <summary>
    /// 叶子节点作用域，用于 using 模式自动管理临时叶子节点。
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    public struct LeafScope<T> : IDisposable
    {
        private readonly IProgressComposite<T> composite;
        private readonly IProgressLeaf<T> leaf;
        private readonly ILeafManager<T> leafManager;
        private bool disposed;

        internal LeafScope(IProgressComposite<T> composite, IProgressLeaf<T> leaf, ILeafManager<T> leafManager)
        {
            this.composite = composite;
            this.leaf = leaf;
            this.leafManager = leafManager;
            disposed = false;
        }

        /// <summary>报告进度</summary>
        public void Report(T value)
        {
            ThrowErrorIfDisposed();
            leaf.Report(value);
        }

        /// <summary>标记完成</summary>
        public void Complete()
        {
            ThrowErrorIfDisposed();
            leaf.Complete();
        }

        /// <summary>获取底层叶子节点（高级用法）</summary>
        public IProgressLeaf<T> Leaf
        {
            get
            {
                ThrowErrorIfDisposed(); 
                return leaf;
            }
        }

        /// <summary>释放作用域，自动移除叶子节点并归还到池中</summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            composite.RemoveChild(leaf);
            leafManager.ReleaseLeaf(leaf);
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new System.ObjectDisposedException(nameof(Progress));
        }
    }

    /// <summary>
    /// 组合节点作用域，用于 using 模式自动管理临时组合节点。
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    public struct CompositeScope<T> : IDisposable
    {
        private readonly IProgressComposite<T> parent;
        private readonly IProgressComposite<T> composite;
        private readonly ILeafManager<T> leafManager;
        private readonly ICompositeManager<T> compositeManager;
        private bool disposed;

        internal CompositeScope(IProgressComposite<T> parent, IProgressComposite<T> composite, ILeafManager<T> leafManager, ICompositeManager<T> compositeManager)
        {
            this.parent = parent;
            this.composite = composite;
            this.leafManager = leafManager;
            this.compositeManager = compositeManager;
            disposed = false;
        }

        /// <summary>获取内部的组合节点</summary>
        public IProgressComposite<T> Composite
        {
            get
            {
                ThrowErrorIfDisposed();
                return composite;
            }
        }

        /// <summary>释放作用域，自动从父节点移除组合节点并归还到池中</summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            composite.ReleaseTree(leafManager, compositeManager);
            parent.RemoveChild(composite);
            compositeManager.ReleaseComposite(composite);
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(CompositeScope<T>));
        }
    }

    /// <summary>
    /// 静态扩展方法
    /// </summary>
    public static partial class Extension
    {
        /// <summary>
        /// 释放所有一级叶子节点
        /// </summary>
        /// <typeparam name="T">进度值类型</typeparam>
        /// <param name="composite"></param>
        /// <remarks>
        /// <para>注意：叶子节点管理器将使用 <see cref="Progress.GetLeafManager{T}"/> 获取的全局管理器。</para>
        /// </remarks>
        public static void ReleaseLeafChildren<T>(this IProgressComposite<T> composite)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            ReleaseLeafChildrenInternal(composite, Progress.GetLeafManager<T>());
        }

        /// <summary>
        /// 释放所有一级叶子节点
        /// </summary>
        /// <typeparam name="T">进度值类型</typeparam>
        /// <param name="composite"></param>
        /// <param name="leafManager">叶子节点管理器</param>
        public static void ReleaseLeafChildren<T>(this IProgressComposite<T> composite, ILeafManager<T> leafManager)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (leafManager == null) throw new ArgumentNullException(nameof(leafManager));
            ReleaseLeafChildrenInternal(composite, leafManager);
        }

        /// <summary>
        /// 释放整个子树中的所有节点
        /// </summary>
        /// <param name="composite"></param>
        /// <remarks>
        /// 递归地将所有叶子节点归还给通过 <see cref="Progress.GetLeafManager{T}"/> 获取的叶子节点管理器，所有子组合节点归还给通过 <see cref="Progress.GetCompositeManager{T}"/> 获取的复合节点管理器。
        /// 当前节点本身不会被释放。
        /// </remarks>
        public static void ReleaseTree<T>(this IProgressComposite<T> composite)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            ReleaseTreeInternal(composite, Progress.GetLeafManager<T>(), Progress.GetCompositeManager<T>());
        }

        /// <summary>
        /// 释放整个子树中的所有节点
        /// </summary>
        /// <param name="composite"></param>
        /// <param name="leafManager">叶子节点管理器</param>
        /// <remarks>
        /// 递归地将所有叶子节点归还给 <paramref name="leafManager"/>，所有子组合节点归还给通过 <see cref="Progress.GetCompositeManager{T}"/> 获取的复合节点管理器。
        /// 当前节点本身不会被释放。
        /// </remarks>
        public static void ReleaseTree<T>(this IProgressComposite<T> composite, ILeafManager<T> leafManager)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (leafManager == null) throw new ArgumentNullException(nameof(leafManager));
            ReleaseTreeInternal(composite, leafManager, Progress.GetCompositeManager<T>());
        }

        /// <summary>
        /// 释放整个子树中的所有节点
        /// </summary>
        /// <param name="composite"></param>
        /// <param name="compositeManager">复合节点管理器</param>
        /// <remarks>
        /// 递归地将所有叶子节点归还给通过 <see cref="Progress.GetLeafManager{T}"/> 获取的叶子节点管理器，所有子组合节点归还给 <paramref name="compositeManager"/>。
        /// 当前节点本身不会被释放。
        /// </remarks>
        public static void ReleaseTree<T>(this IProgressComposite<T> composite, ICompositeManager<T> compositeManager)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (compositeManager == null) throw new ArgumentNullException(nameof(compositeManager));
            ReleaseTreeInternal(composite, Progress.GetLeafManager<T>(), compositeManager);
        }

        /// <summary>
        /// 释放整个子树中的所有节点
        /// </summary>
        /// <param name="composite"></param>
        /// <param name="leafManager">叶子节点管理器</param>
        /// <param name="compositeManager">复合节点管理器</param>
        /// <remarks>
        /// 递归地将所有叶子节点归还给 <paramref name="leafManager"/>，所有子组合节点归还给 <paramref name="compositeManager"/>。
        /// 当前节点本身不会被释放。
        /// </remarks>
        public static void ReleaseTree<T>(this IProgressComposite<T> composite, ILeafManager<T> leafManager, ICompositeManager<T> compositeManager)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (leafManager == null) throw new ArgumentNullException(nameof(leafManager));
            if (compositeManager == null) throw new ArgumentNullException(nameof(compositeManager));
            ReleaseTreeInternal(composite, leafManager, compositeManager);
        }

        /// <summary>
        /// 执行一个具有进度的同步任务，任务完成后自动清理临时叶子节点。
        /// </summary>
        /// <param name="composite"></param>
        /// <param name="work">执行任务的委托，参数为进度报告器</param>
        /// <remarks>
        /// <para>注意：叶子节点管理器将使用 <see cref="Progress.GetLeafManager{T}"/> 获取的全局管理器。</para>
        /// </remarks>
        public static void RunWithProgress<T>(this IProgressComposite<T> composite, Action<IProgressLeaf<T>> work)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (work == null) throw new ArgumentNullException(nameof(work));
            RunWithProgressInternal(composite, work, Progress.GetLeafManager<T>());
        }

        /// <summary>
        /// 执行一个具有进度的同步任务，任务完成后自动清理临时叶子节点。
        /// </summary>
        /// <param name="composite"></param>
        /// <param name="work">执行任务的委托，参数为进度报告器（支持 Report 和 Complete）</param>
        /// <param name="leafManager">叶子节点管理器</param>
        public static void RunWithProgress<T>(this IProgressComposite<T> composite, Action<IProgressLeaf<T>> work, ILeafManager<T> leafManager)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (work == null) throw new ArgumentNullException(nameof(work));
            if (leafManager == null) throw new ArgumentNullException(nameof(leafManager));
            RunWithProgressInternal(composite, work, leafManager);
        }

        /// <summary>
        /// 执行一个具有进度的异步任务，任务完成后自动清理临时叶子节点。
        /// </summary>
        /// <param name="composite"></param>
        /// <param name="work">异步任务委托，参数为进度报告器</param>
        /// <remarks>
        /// <para>注意：叶子节点管理器将使用 <see cref="Progress.GetLeafManager{T}"/> 获取的全局管理器。</para>
        /// </remarks>
        public static Task RunWithProgressAsync<T>(this IProgressComposite<T> composite, Func<IProgressLeaf<T>, Task> work) 
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (work == null) throw new ArgumentNullException(nameof(work));
            return RunWithProgressAsyncInternal(composite, work, Progress.GetLeafManager<T>());
        }

        /// <summary>
        /// 执行一个具有进度的异步任务，任务完成后自动清理临时叶子节点。
        /// </summary>
        /// <param name="composite"></param>
        /// <param name="work">异步任务委托，参数为进度报告器</param>
        /// <param name="leafManager">叶子节点管理器</param>
        public static Task RunWithProgressAsync<T>(this IProgressComposite<T> composite, Func<IProgressLeaf<T>, Task> work, ILeafManager<T> leafManager)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (work == null) throw new ArgumentNullException(nameof(work));
            if (leafManager == null) throw new ArgumentNullException(nameof(leafManager));
            return RunWithProgressAsyncInternal(composite, work, leafManager);
        }

        /// <summary>
        /// 开始一个进度作用域，返回一个可释放的作用域对象。
        /// 作用域生命周期内自动管理临时叶子节点的添加和最终清理。
        /// </summary>
        /// <typeparam name="T">进度值类型</typeparam>
        /// <param name="composite"></param>
        /// <returns>可释放的作用域对象</returns>
        /// <remarks>
        /// <para>注意：叶子节点管理器将使用 <see cref="Progress.GetLeafManager{T}"/> 获取的全局管理器。</para>
        /// </remarks>
        public static LeafScope<T> BeginProgress<T>(this IProgressComposite<T> composite)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            return BeginProgressInternal(composite, Progress.GetLeafManager<T>());
        }

        /// <summary>
        /// 开始一个进度作用域，返回一个可释放的作用域对象。
        /// 作用域生命周期内自动管理临时叶子节点的添加和最终清理。
        /// </summary>
        /// <typeparam name="T">进度值类型</typeparam>
        /// <param name="composite"></param>
        /// <param name="leafManager">叶子节点管理器</param>
        /// <returns>可释放的作用域对象</returns>
        public static LeafScope<T> BeginProgress<T>(this IProgressComposite<T> composite, ILeafManager<T> leafManager)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (leafManager == null) throw new ArgumentNullException(nameof(leafManager));
            return BeginProgressInternal(composite, leafManager);
        }

        /// <summary>
        /// 开始一个组合节点作用域，使用指定的规则。
        /// </summary>
        /// <typeparam name="T">进度值类型</typeparam>
        /// <param name="parent">父组合节点</param>
        /// <param name="rule">组合规则</param>
        /// <returns>可释放的作用域对象</returns>
        /// <remarks>
        /// <para>注意：叶子节点管理器将使用 <see cref="Progress.GetLeafManager{T}"/> 获取的全局管理器，复合节点管理器将使用 <see cref="Progress.GetCompositeManager{T}"/> 获取的全局管理器。</para>
        /// </remarks>
        public static CompositeScope<T> BeginComposite<T>(this IProgressComposite<T> parent, ICompositionRule<T> rule)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            return BeginCompositeInternal(parent, rule, Progress.GetLeafManager<T>(), Progress.GetCompositeManager<T>());
        }

        /// <summary>
        /// 开始一个组合节点作用域，使用指定的规则。
        /// </summary>
        /// <typeparam name="T">进度值类型</typeparam>
        /// <param name="parent">父组合节点</param>
        /// <param name="rule">组合规则</param>
        /// <param name="leafManager">叶子节点管理器</param>
        /// <returns>可释放的作用域对象</returns>
        /// <remarks>
        /// <para>注意：叶子节点管理器将使用 <paramref name="leafManager"/>，复合节点管理器将使用 <see cref="Progress.GetCompositeManager{T}"/> 获取的全局管理器。</para>
        /// </remarks>
        public static CompositeScope<T> BeginComposite<T>(this IProgressComposite<T> parent, ICompositionRule<T> rule, ILeafManager<T> leafManager)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            if (leafManager == null) throw new ArgumentNullException(nameof(leafManager));
            return BeginCompositeInternal(parent, rule, leafManager, Progress.GetCompositeManager<T>());
        }

        /// <summary>
        /// 开始一个组合节点作用域，使用指定的规则。
        /// </summary>
        /// <typeparam name="T">进度值类型</typeparam>
        /// <param name="parent">父组合节点</param>
        /// <param name="rule">组合规则</param>
        /// <param name="compositeManager">复合节点管理器</param>
        /// <returns>可释放的作用域对象</returns>
        /// <remarks>
        /// <para>注意：叶子节点管理器将使用 <see cref="Progress.GetLeafManager{T}"/> 获取的全局管理器，复合节点管理器将使用 <paramref name="compositeManager"/>。</para>
        /// </remarks>
        public static CompositeScope<T> BeginComposite<T>(this IProgressComposite<T> parent, ICompositionRule<T> rule, ICompositeManager<T> compositeManager)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            if (compositeManager == null) throw new ArgumentNullException(nameof(compositeManager));
            return BeginCompositeInternal(parent, rule, Progress.GetLeafManager<T>(), compositeManager);
        }

        /// <summary>
        /// 开始一个组合节点作用域，使用指定的规则和管理器。
        /// </summary>
        /// <typeparam name="T">进度值类型</typeparam>
        /// <param name="parent">父组合节点</param>
        /// <param name="rule">组合规则</param>
        /// <param name="leafManager">叶子节点管理器</param>
        /// <param name="compositeManager">组合节点管理器</param>
        /// <returns>可释放的作用域对象</returns>
        public static CompositeScope<T> BeginComposite<T>(this IProgressComposite<T> parent, ICompositionRule<T> rule, ILeafManager<T> leafManager, ICompositeManager<T> compositeManager)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            if (leafManager == null) throw new ArgumentNullException (nameof(leafManager));
            if (compositeManager == null) throw new ArgumentNullException(nameof(compositeManager));
            return BeginCompositeInternal(parent, rule, leafManager, compositeManager);
        }

        /// <summary>
        /// 批量添加子节点
        /// </summary>
        public static void AddChildren<T>(this IProgressComposite<T> composite, IProgressNode<T> node1, IProgressNode<T> node2)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (node1 != null) composite.AddChild(node1);
            if (node2 != null) composite.AddChild(node2);
        }

        /// <summary>
        /// 批量添加子节点
        /// </summary>
        public static void AddChildren<T>(this IProgressComposite<T> composite, IProgressNode<T> node1, IProgressNode<T> node2, IProgressNode<T> node3)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (node1 != null) composite.AddChild(node1);
            if (node2 != null) composite.AddChild(node2);
            if (node3 != null) composite.AddChild(node3);
        }

        /// <summary>
        /// 批量添加子节点
        /// </summary>
        public static void AddChildren<T>(this IProgressComposite<T> composite, params IProgressNode<T>[] nodes)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (nodes == null) return;
            int length = nodes.Length;
            for (int i = 0; i < length; i++)
            {
                var node = nodes[i];
                if (node == null) continue;
                composite.AddChild(node);
            }
        }

        /// <summary>
        /// 批量添加带权重的子节点
        /// </summary>
        /// <remarks>
        /// <para>注意：仅当组合节点实现 <see cref="IWeightedProgressComposite{T}"/> 时可用。</para>
        /// </remarks>
        public static void AddChildren<T>(this IWeightedProgressComposite<T> composite, (IProgressNode<T> node, float weight) weightedNodes1, (IProgressNode<T> node, float weight) weightedNodes2)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (weightedNodes1.node != null) composite.AddChild(weightedNodes1.node, weightedNodes1.weight);
            if (weightedNodes2.node != null) composite.AddChild(weightedNodes2.node, weightedNodes2.weight);
        }

        /// <summary>
        /// 批量添加带权重的子节点
        /// </summary>
        /// <remarks>
        /// <para>注意：仅当组合节点实现 <see cref="IWeightedProgressComposite{T}"/> 时可用。</para>
        /// </remarks>
        public static void AddChildren<T>(this IWeightedProgressComposite<T> composite, (IProgressNode<T> node, float weight) weightedNodes1, (IProgressNode<T> node, float weight) weightedNodes2, (IProgressNode<T> node, float weight) weightedNodes3)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (weightedNodes1.node != null) composite.AddChild(weightedNodes1.node, weightedNodes1.weight);
            if (weightedNodes2.node != null) composite.AddChild(weightedNodes2.node, weightedNodes2.weight);
            if (weightedNodes3.node != null) composite.AddChild(weightedNodes3.node, weightedNodes3.weight);
        }

        /// <summary>
        /// 批量添加带权重的子节点
        /// </summary>
        /// <remarks>
        /// <para>注意：仅当组合节点实现 <see cref="IWeightedProgressComposite{T}"/> 时可用。</para>
        /// </remarks>
        public static void AddChildren<T>(this IWeightedProgressComposite<T> composite, params (IProgressNode<T> node, float weight)[] weightedNodes)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            if (weightedNodes == null) return;
            int length = weightedNodes.Length;
            for (int i = 0; i < length; i++)
            {
                var (node, weight) = weightedNodes[i];
                if (node == null) continue;
                composite.AddChild(node, weight);
            }       
        }

        private static void ReleaseLeafChildrenInternal<T>(IProgressComposite<T> composite, ILeafManager<T> leafManager) 
        {
            List<IProgressNode<T>> temp = ListPool.Rent<IProgressNode<T>>();
            try
            {
                foreach (var child in composite.Children)
                {
                    if (child is IProgressLeaf<T> leaf)
                    {
                        leafManager.ReleaseLeaf(leaf);
                        temp.Add(leaf);
                    }
                }

                for (int i = 0; i < temp.Count; i++)
                    composite.RemoveChild(temp[i]);
            }
            finally 
            {
                ListPool.Return(temp);
            }
        }

        private static void ReleaseTreeInternal<T>(IProgressComposite<T> composite, ILeafManager<T> leafManager, ICompositeManager<T> compositeManager)
        {
            List<IProgressNode<T>> temp = ListPool.Rent<IProgressNode<T>>();
            try
            {
                foreach (var child in composite.Children)
                {
                    if (child is IProgressLeaf<T> leaf)
                    {
                        leafManager.ReleaseLeaf(leaf);
                        temp.Add(leaf);
                    }
                    else if (child is IProgressComposite<T> inner)
                    {
                        ReleaseTreeInternal(inner, leafManager, compositeManager);
                        compositeManager.ReleaseComposite(inner);
                        temp.Add(inner);
                    }
                }

                for (int i = 0; i < temp.Count; i++)
                    composite.RemoveChild(temp[i]);
            }
            finally 
            {
                ListPool.Return(temp);
            }
        }

        private static void RunWithProgressInternal<T>(IProgressComposite<T> composite, Action<IProgressLeaf<T>> work, ILeafManager<T> leafManager)
        {
            var leaf = leafManager.AcquireLeaf();
            composite.AddChild(leaf);
            try
            {
                work(leaf);
            }
            finally
            {
                composite.RemoveChild(leaf);
                leafManager.ReleaseLeaf(leaf);
            }
        }

        private static async Task RunWithProgressAsyncInternal<T>(IProgressComposite<T> composite, Func<IProgressLeaf<T>, Task> work, ILeafManager<T> leafManager) 
        {
            var leaf = leafManager.AcquireLeaf();
            composite.AddChild(leaf);
            try
            {
                await work(leaf).ConfigureAwait(false);
            }
            finally
            {
                composite.RemoveChild(leaf);
                leafManager.ReleaseLeaf(leaf);
            }
        }

        private static LeafScope<T> BeginProgressInternal<T>(IProgressComposite<T> composite, ILeafManager<T> leafManager) 
        {
            var leaf = leafManager.AcquireLeaf();
            composite.AddChild(leaf);
            return new LeafScope<T>(composite, leaf, leafManager);
        }

        private static CompositeScope<T> BeginCompositeInternal<T>(IProgressComposite<T> parent, ICompositionRule<T> rule, ILeafManager<T> leafManager, ICompositeManager<T> compositeManager)
        {
            var composite = compositeManager.AcquireComposite(rule);
            parent.AddChild(composite);
            return new CompositeScope<T>(parent, composite, leafManager, compositeManager);
        }
    }
}
