#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE && !EVENTHUB_DISABLE_UNITY_INNER_EXTENSION

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// 面向 Unity 的扩展类实现
    /// </summary>
    [Preserve]
    public static class UnityExtension
    {
        /// <summary>
        /// 订阅同步事件
        /// </summary>
        /// <param name="component"></param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription Subscribe<TEvent>(this Component component, Action<TEvent> handler, int priority = 0)
        {
            var sub = EventDispatcher.Subscribe(handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

        /// <summary>
        /// 订阅异步事件
        /// </summary>
        /// <param name="component"></param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription Subscribe<TEvent>(this Component component, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            var sub = EventDispatcher.Subscribe(handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

#if !EVENTHUB_DISABLE_INNER_EXTENSION

        /// <summary>
        /// 订阅一次性同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="component"></param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeOnce<TEvent>(this Component component, Action<TEvent> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeOnce(handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

        /// <summary>
        /// 订阅一次性异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="component"></param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeOnce<TEvent>(this Component component, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeOnce(handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

        /// <summary>
        /// 订阅条件同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="component"></param>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeIf<TEvent>(this Component component, Predicate<TEvent> filter, Action<TEvent> handler, int priority = 0) 
        { 
            var sub = EventDispatcher.SubscribeIf(filter, handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

        /// <summary>
        /// 订阅条件异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="component"></param>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeIf<TEvent>(this Component component, Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeIf(filter, handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

#endif

        /// <summary>
        /// 订阅在 Unity 主线程执行的异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="component"></param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeOnMainThread<TEvent>(this Component component, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeOnMainThread(handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

        /// <summary>
        /// 订阅在 Unity 主线程运行的一次性异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="component"></param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeOnceOnMainThread<TEvent>(this Component component, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            var sub = EventDispatcher.SubscribeOnceOnMainThread(handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

        /// <summary>
        /// 订阅在 Unity 主线程运行的条件异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="component"></param>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public static ISubscription SubscribeIfOnMainThread<TEvent>(this Component component, Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeIfOnMainThread(filter, handler, priority);
            SubscriptionMonitor.Instance.Register(component, sub);
            return sub;
        }

        /// <summary>
        /// 为与指定组件关联的所有事件订阅句柄取消订阅
        /// </summary>
        /// <param name="component"></param>
        public static void UnsubscribeAll(this Component component)
        {
            SubscriptionMonitor.Instance.UnsubscribeAll(component);
        }
    }
}

#endif