#if !EVENTHUB_EXTENSION_ENABLE

using System;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// 事件分发器
    /// </summary>
    [Preserve]
    public static partial class EventDispatcher
    {
        /// <summary>
        /// 复合事件分发器实例
        /// </summary>
        public static IEventDispatcher Dispatcher
        {
            set
            {
                ThrowErrorIfDisposed(); 
                dispatcher = value;
            }
        }
        private static IEventDispatcher dispatcher = defaultDispatcher;

        /// <summary>
        /// 异步事件分发器实例
        /// </summary>
        public static IAsyncEventDispatcher AsyncDispatcher
        {
            set
            {
                ThrowErrorIfDisposed(); 
                asyncDispatcher = value;
            }
        }
        private static IAsyncEventDispatcher asyncDispatcher = null;

        /// <summary>
        /// 同步事件分发器实例
        /// </summary>
        public static ISyncEventDispatcher SyncDispatcher
        {
            set
            {
                ThrowErrorIfDisposed(); 
                syncDispatcher = value;
            }
        }
        private static ISyncEventDispatcher syncDispatcher = null;

        /// <summary>
        /// 并发事件分发器实例
        /// </summary>
        public static IParallelizable Parallelizable
        {
            set
            {
                ThrowErrorIfDisposed(); 
                parallelizable = value;
            }
        }
        private static IParallelizable parallelizable = null;

        /// <summary>
        /// 日志记录器
        /// </summary>
        public static ILogger Logger
        {
            set
            {
                ThrowErrorIfDisposed(); 
                EventDispatcherLog.Logger = value;
            }
        }

        /// <summary>
        /// 是否启用日志记录
        /// </summary>
        public static bool LogEnabled
        {
            get
            {
                ThrowErrorIfDisposed();
                return EventDispatcherLog.Enabled;
            }
            set
            {
                ThrowErrorIfDisposed();
                EventDispatcherLog.Enabled = value;
            }
        }

        /// <summary>
        /// 异常捕获事件
        /// </summary>
        public static event Action<Type, Delegate, Exception> OnError
        {
            add
            {
                ThrowErrorIfDisposed();
                ExceptionCatcher.OnError += value;
            }
            remove
            {
                ThrowErrorIfDisposed(); 
                ExceptionCatcher.OnError -= value;
            }
        }

        /// <summary>
        /// 是否启用异常捕获
        /// </summary>
        public static bool ExceptionCatchEnabled
        {
            get
            {
                ThrowErrorIfDisposed();
                return ExceptionCatcher.Enabled;
            }
            set
            {
                ThrowErrorIfDisposed();
                ExceptionCatcher.Enabled = value;
            }
        }

        private static readonly EventDispatcherInternal defaultDispatcher = new EventDispatcherInternal();

        private static ISyncEventDispatcher GetSyncEventDispatcher()
        {
            if (syncDispatcher != null) return syncDispatcher;
            if (dispatcher != null && dispatcher is ISyncEventDispatcher syncEventDispatcher) return syncEventDispatcher;
            return defaultDispatcher;
        }

        private static ISyncEventDispatcherExtension GetSyncEventDispatcherExtension()
        {
            if (asyncDispatcher is ISyncEventDispatcherExtension typed1) return typed1;
            if (dispatcher is ISyncEventDispatcherExtension typed2) return typed2;
            return defaultDispatcher;
        }

        private static IAsyncEventDispatcher GetAsyncEventDispatcher()
        {
            if (asyncDispatcher != null) return asyncDispatcher;
            if (dispatcher != null && dispatcher is IAsyncEventDispatcher asyncEventDispatcher) return asyncEventDispatcher;
            return defaultDispatcher;
        }

        private static IAsyncEventDispatcherExtension GetAsyncEventDispatcherExtension() 
        {
            if (asyncDispatcher is IAsyncEventDispatcherExtension typed1) return typed1;
            if (dispatcher is IAsyncEventDispatcherExtension typed2) return typed2;
            return defaultDispatcher;
        }

        private static IParallelizable GetParallelizable()
        {
            if (parallelizable != null) return parallelizable;
            if (dispatcher != null && dispatcher is IParallelizable disParallelizable) return disParallelizable;
            if (defaultDispatcher is IParallelizable defaultParallelizable) return defaultParallelizable;
            throw new NotSupportedException("The current event dispatcher does not support parallel publishing.");
        }

        private static ICleanable GetCleanable() 
        {
            if (dispatcher != null && dispatcher is ICleanable disCleanable) return disCleanable;
            if (defaultDispatcher is ICleanable defaultCleanable) return defaultCleanable;
            throw new NotSupportedException("The current event dispatcher does not support cleaning.");
        }

#if EVENTHUB_TESTS
        public static void ClearErrorEvents()
        {
            ExceptionCatcher.Clear();
        }

        public static void ResetForTesting()
        {
            disposed = false;
            if (dispatcher is EventDispatcherInternal typed) typed.Dispose();
            dispatcher = new EventDispatcherInternal();
            asyncDispatcher = null;
            syncDispatcher = null;
            parallelizable = null;
            EventDispatcherLog.ResetForTesting();
            ExceptionCatcher.ResetForTesting();
        }
#endif
    }
}

#endif