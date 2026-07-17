#if !EVENTHUB_EXTENSION_ENABLE

using System;

namespace EventHub.Unity
{
    public static partial class EventDispatcher
    {
        /// <summary>
        /// 发布同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        public static void Publish<TEvent>(TEvent @event)
        {
            ThrowErrorIfDisposed();
            GetSyncEventDispatcher().Publish(@event);
        }

        /// <summary>
        /// 订阅同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)
        {
            ThrowErrorIfDisposed();
            return GetSyncEventDispatcher().Subscribe(handler, priority);
        }

        /// <summary>
        /// 取消订阅同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <returns>取消订阅的事件个数</returns>
        public static int Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            ThrowErrorIfDisposed();
            return GetSyncEventDispatcher().Unsubscribe(handler);
        }

        /// <summary>
        /// 取消订阅指定同步事件类型的所有事件
        /// </summary>
        /// <typeparam name="TEvent">同步事件类型</typeparam>
        /// <returns>取消订阅的事件数量</returns>
        public static int UnsubscribeSyncEvents<TEvent>() 
        {
            ThrowErrorIfDisposed();
            return GetSyncEventDispatcherExtension().UnsubscribeSyncEvents<TEvent>();
        }

        /// <summary>
        /// 取消订阅所有同步事件
        /// </summary>
        /// <returns>取消订阅的事件数量</returns>
        public static int UnsubscribeAllSyncEvents() 
        {
            ThrowErrorIfDisposed();
            return GetSyncEventDispatcherExtension().UnsubscribeAll();
        }
    }
}

#endif