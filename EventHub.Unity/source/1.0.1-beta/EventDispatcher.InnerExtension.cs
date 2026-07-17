#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_DISABLE_INNER_EXTENSION

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace EventHub.Unity
{
    public static partial class EventDispatcher
    {
        /// <summary>
        /// 订阅一次性异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeOnce<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            ThrowErrorIfDisposed();
            return GetAsyncEventDispatcher().SubscribeOnce(handler, priority);
        }

        /// <summary>
        /// 订阅一次性同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeOnce<TEvent>(Action<TEvent> handler, int priority = 0)
        {
            ThrowErrorIfDisposed();
            return GetSyncEventDispatcher().SubscribeOnce(handler, priority);
        }

        /// <summary>
        /// 订阅条件异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeIf<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            ThrowErrorIfDisposed();
            return GetAsyncEventDispatcher().SubscribeIf(filter, handler, priority);
        }

        /// <summary>
        /// 订阅条件同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeIf<TEvent>(Predicate<TEvent> filter, Action<TEvent> handler, int priority = 0)
        {
            ThrowErrorIfDisposed();
            return GetSyncEventDispatcher().SubscribeIf(filter, handler, priority);
        }

        /// <summary>
        /// 发布支持中断的同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        public static void PublishInterruptableEvents<TEvent>(TEvent @event) where TEvent : IInterruptableEvent
        {
            ThrowErrorIfDisposed();
            GetSyncEventDispatcher().PublishInterruptableEvents(@event);
        }

        /// <summary>
        /// 发布支持取消执行的同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        public static void PublishCancelableEvents<TEvent>(TEvent @event) where TEvent : ICancelableEvent
        {
            ThrowErrorIfDisposed();
            GetSyncEventDispatcher().PublishCancelableEvents(@event);
        }
    }
}

#endif