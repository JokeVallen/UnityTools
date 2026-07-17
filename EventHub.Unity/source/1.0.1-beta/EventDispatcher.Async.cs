#if !EVENTHUB_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace EventHub.Unity
{
    public static partial class EventDispatcher
    {
        /// <summary>
        /// 异步串行发布异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        public static UniTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            ThrowErrorIfDisposed();
            return GetAsyncEventDispatcher().PublishAsync(@event, cancellationToken);
        }

        /// <summary>
        /// 订阅异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription Subscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            ThrowErrorIfDisposed();
            return GetAsyncEventDispatcher().Subscribe(handler, priority);
        }

        /// <summary>
        /// 取消订阅异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <returns>取消订阅的事件数量</returns>
        public static int Unsubscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler)
        {
            ThrowErrorIfDisposed();
            return GetAsyncEventDispatcher().Unsubscribe(handler);
        }

        /// <summary>
        /// 取消订阅指定异步事件类型的所有事件
        /// </summary>
        /// <typeparam name="TEvent">异步事件类型</typeparam>
        /// <returns>取消订阅的事件数量</returns>
        public static int UnsubscribeAsyncEvents<TEvent>() 
        {
            ThrowErrorIfDisposed();
            return GetAsyncEventDispatcherExtension().UnsubscribeAsyncEvents<TEvent>();
        }

        /// <summary>
        /// 取消订阅所有异步事件
        /// </summary>
        /// <returns>取消订阅的事件数量</returns>
        public static int UnsubscribeAllAsyncEvents() 
        {
            ThrowErrorIfDisposed();
            return GetAsyncEventDispatcherExtension().UnsubscribeAll();
        }
    }
}

#endif