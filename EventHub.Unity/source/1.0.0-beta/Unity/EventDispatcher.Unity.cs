#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace EventHub.Unity
{
    public static partial class EventDispatcher
    {
        /// <summary>
        /// 事件订阅句柄监视器
        /// </summary>
        public static ISubscriptionMonitor SubscriptionMonitor => EventHub.Unity.SubscriptionMonitor.Instance;

        /// <summary>
        /// 事件订阅句柄监视器配置
        /// </summary>
        public static ISubscriptionMonitorConfig SubscriptionMonitorConfig => EventHub.Unity.SubscriptionMonitorConfig.Instance;

#if !EVENTHUB_DISABLE_UNITY_INNER_EXTENSION

        /// <summary>
        /// 订阅在 Unity 主线程执行的异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeOnMainThread<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            return GetAsyncEventDispatcher().SubscribeOnMainThread(handler, priority);
        }

        /// <summary>
        /// 订阅在 Unity 主线程执行的一次性异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeOnceOnMainThread<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            return GetAsyncEventDispatcher().SubscribeOnceOnMainThread(handler, priority);
        }

        /// <summary>
        /// 订阅在 Unity 主线程执行的条件异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeIfOnMainThread<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            return GetAsyncEventDispatcher().SubscribeIfOnMainThread(filter, handler, priority);
        }

#endif
    }
}

#endif