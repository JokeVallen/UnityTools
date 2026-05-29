using System;
using System.Collections.Concurrent;

namespace EasyProgress.Core
{
    /// <summary>
    /// 进度工具库静态入口
    /// </summary>
    /// <remarks>
    /// <para>提供创建和释放节点的方法，支持自定义管理器注册。</para>
    /// <para>默认注册了一个 <see cref="DefaultProgressManager{T}"/> 实例（T 为 double），使用 <see cref="DefaultLeaf"/> 和 <see cref="WeightedRealtimeComposite"/>。</para>
    /// </remarks>
    public static class Progress
    {
        private static readonly ConcurrentDictionary<Type, IProgressManager> progressManagers = new ConcurrentDictionary<Type, IProgressManager>();
        private static bool disposed;

        static Progress()
        {
            var defaultManager = DefaultProgressManager.CreateDefault();
            RegisterProgressManager(defaultManager);
        }

        /// <summary>
        /// 获取指定进度类型的进度管理器
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <returns>进度管理器</returns>
        public static IProgressManager<T> GetProgressManager<T>() 
        {
            ThrowErrorIfDisposed();
            return GetProgressManagerInternal<T>();
        }

        /// <summary>
        /// 获取指定进度类型的叶子节点管理器
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <returns>叶子节点管理器</returns>
        public static ILeafManager<T> GetLeafManager<T>()
        {
            ThrowErrorIfDisposed();
            return GetLeafManagerInternal<T>();
        }

        /// <summary>
        /// 获取指定进度类型的复合节点管理器
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <returns>复合节点管理器</returns>
        public static ICompositeManager<T> GetCompositeManager<T>()
        {
            ThrowErrorIfDisposed();
            return GetCompositeManagerInternal<T>();
        }

        /// <summary>
        /// 创建指定进度类型的叶子节点
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <returns>叶子节点</returns>
        public static IProgressLeaf<T> CreateLeaf<T>()
        {
            ThrowErrorIfDisposed();
            return GetLeafManagerInternal<T>().AcquireLeaf();
        }

        /// <summary>
        /// 创建指定进度类型的复合节点
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <param name="rule">复合规则</param>
        /// <returns>复合节点</returns>
        public static IProgressComposite<T> CreateComposite<T>(ICompositionRule<T> rule)
        {
            ThrowErrorIfDisposed();
            return GetCompositeManagerInternal<T>().AcquireComposite(rule);
        }

        /// <summary>
        /// 创建带权重的复合节点
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <param name="rule">复合规则</param>
        /// <returns>复合节点</returns>
        public static IWeightedProgressComposite<T> CreateWeightedComposite<T>(ICompositionRule<T> rule)
        {
            ThrowErrorIfDisposed();
            var composite = GetCompositeManagerInternal<T>().AcquireComposite(rule);
            if (composite is IWeightedProgressComposite<T> weighted)
                return weighted;

            throw new InvalidOperationException(
                $@"""The registered composite manager for type '{typeof(T)}' does not produce weighted composites. 
Consider registering a different manager or using CreateComposite instead.""");
        }

        /// <summary>
        /// 释放指定进度类型的叶子节点
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <param name="leaf">叶子节点</param>
        public static void ReleaseLeaf<T>(IProgressLeaf<T> leaf)
        {
            ThrowErrorIfDisposed();
            GetLeafManagerInternal<T>().ReleaseLeaf(leaf);
        }

        /// <summary>
        /// 释放指定进度类型的复合节点
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <param name="composite">复合节点</param>
        public static void ReleaseComposite<T>(IProgressComposite<T> composite)
        {
            ThrowErrorIfDisposed();
            GetCompositeManagerInternal<T>().ReleaseComposite(composite);
        }

        /// <summary>
        /// 注册指定进度类型的进度管理器
        /// </summary>
        /// <param name="type">进度类型</param>
        /// <param name="progressManager">进度管理器</param>
        public static void RegisterProgressManager(Type type, IProgressManager progressManager)
        {
            ThrowErrorIfDisposed();
            if (type == null) return;
            if (progressManager == null) return;
            progressManagers[type] = progressManager;
        }

        /// <summary>
        /// 注册指定进度类型的进度管理器
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        /// <param name="progressManager">进度管理器</param>
        public static void RegisterProgressManager<T>(IProgressManager<T> progressManager)
        {
            ThrowErrorIfDisposed();
            if (progressManager == null) return;
            progressManagers[typeof(T)] = progressManager;
        }

        /// <summary>
        /// 注销指定进度类型的进度管理器
        /// </summary>
        /// <param name="type">进度类型</param>
        public static void UnregisterProgressManager(Type type)
        {
            ThrowErrorIfDisposed();
            if (type == null) return;
            progressManagers.TryRemove(type, out _);
        }

        /// <summary>
        /// 注销指定进度类型的进度管理器
        /// </summary>
        /// <typeparam name="T">进度类型</typeparam>
        public static void UnregisterProgressManager<T>()
        {
            ThrowErrorIfDisposed();
            progressManagers.TryRemove(typeof(T), out _);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <remarks>
        /// <para>注意：释放后访问其它任何公共 API 将触发异常 <see cref="ObjectDisposedException"/>。</para>
        /// </remarks>
        public static void Dispose()
        {
            DisposeInternal();
        }

        private static void DisposeInternal()
        {
            if (disposed) return;
            disposed = true;
            progressManagers.Clear();
            ListPool.Dispose();
            DictionaryPool.Dispose();
        }

        private static void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new System.ObjectDisposedException(nameof(Progress));
        }

        private static IProgressManager<T> GetProgressManagerInternal<T>()
        {
            if (progressManagers.TryGetValue(typeof(T), out var mgr))
            {
                if (mgr is IProgressManager<T> typed)
                    return typed;
            }

            throw new System.NotSupportedException($"Missing the prgress manager implemented the interface '{typeof(IProgressManager<T>)}'.");
        }

        private static ILeafManager<T> GetLeafManagerInternal<T>()
        {
            var progressManager = GetProgressManagerInternal<T>();
            if (!(progressManager is ILeafManager<T> typed1))
                throw new System.InvalidOperationException($"The progress manager doesn't implement the interface '{typeof(ILeafManager<T>)}'.");
            return typed1;
        }

        private static ICompositeManager<T> GetCompositeManagerInternal<T>()
        {
            var progressManager = GetProgressManagerInternal<T>();
            if (!(progressManager is ICompositeManager<T> typed1))
                throw new System.InvalidOperationException($"The progress manager doesn't implement the interface '{typeof(ICompositeManager<T>)}'.");
            return typed1;
        }

#if EASYPROGRESS_TESTS
        public static IProgressManager<T> Test_GetProgressManager<T>() => GetProgressManager<T>();
        public static void Test_DisposeInternal() => DisposeInternal();
#endif
    }
}
