#if !EVENTHUB_EXTENSION_ENABLE

using System;

namespace EventHub.Unity
{
    public static partial class EventDispatcher
    {
        /// <summary>
        /// 是否已释放
        /// </summary>
        public static bool Disposed => disposed;
        private static bool disposed;

        /// <summary>
        /// 尝试同步清理未使用的锁对象
        /// </summary>
        /// <returns>成功清理的锁对象的个数</returns>
        public static int TryCleanupUnusedLocks()
        {
            ThrowErrorIfDisposed();
            return GetCleanable().TryCleanupUnusedLocks();
        }

        /// <summary>
        /// 尝试同步清理未使用的订阅者集合
        /// </summary>
        /// <returns>成功清理的订阅者集合的个数</returns>
        public static int TryCleanupUnusedCollections()
        {
            ThrowErrorIfDisposed();
            return GetCleanable().TryCleanupUnusedCollections();
        }

        /// <summary>
        /// 尝试同步清理未使用的锁对象和订阅者集合
        /// </summary>
        /// <returns>成功清理的锁对象和订阅者集合的总个数</returns>
        public static int TryCleanupUnusedLocksAndCollections()
        {
            ThrowErrorIfDisposed();
            return GetCleanable().TryCleanupUnusedLocksAndCollections();
        }

        /// <summary>
        /// 取消订阅所有事件
        /// </summary>
        /// <returns>取消订阅的事件数量</returns>
        public static int UnsubscribeAllEvents() 
        {
            ThrowErrorIfDisposed();
            return UnsubscribeAllSyncEvents() + UnsubscribeAllAsyncEvents();
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        /// <remarks>
        /// <para>注意：外部注入的可替换组件的生命周期不受本工具库管控，工具库只会置空对它们的引用，可替换组件的释放由使用者自行负责。</para>
        /// </remarks>
        public static void Dispose()
        {
            DisposeInternal();
        }

        /// <summary>
        /// 安全释放所有资源
        /// </summary>
        /// <remarks>
        /// <para>注意：外部注入的可替换组件的生命周期不受本工具库管控，工具库只会置空对它们的引用，可替换组件的释放由使用者自行负责。</para>
        /// <para>安全释放会避免在其它线程访问事件集合过程中触发释放而导致异常，但这意味着可能会比 <see cref="Dispose"/> 方法更慢更久。</para>
        /// </remarks>
        public static void SafeDispose() 
        {
            SafeDisposeInternal();
        }

        private static void ThrowErrorIfDisposed() 
        {
            if (disposed)
                throw new ObjectDisposedException("EventHub.Unity.EventDispatcher");
        }

        private static void DisposeInternal() 
        {
            if (disposed) return;
            disposed = true;

            defaultDispatcher.Dispose();
            dispatcher = null;
            asyncDispatcher = null;
            syncDispatcher = null;
            parallelizable = null;
        }

        private static void SafeDisposeInternal()
        {
            if (disposed) return;
            disposed = true;

            defaultDispatcher.SafeDispose();
            dispatcher = null;
            asyncDispatcher = null;
            syncDispatcher = null;
            parallelizable = null;
        }
    }
}

#endif