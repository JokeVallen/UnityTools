#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// 基于事件系统的组件
    /// </summary>
    /// <remarks>
    /// <para>提供订阅事件的功能，对事件订阅句柄进行托管，并在销毁时自动取消订阅。</para>
    /// <para></para>
    /// </remarks>
    [Preserve]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    internal sealed class EventHubBehaviour : MonoBehaviour
    {
        private readonly List<ISubscription> subscriptions = new List<ISubscription>();

        /// <summary>
        /// 订阅同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0)
        {
            var sub = EventDispatcher.Subscribe(handler, priority);
            subscriptions.Add(sub);
            return sub;
        }

        /// <summary>
        /// 订阅异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        public ISubscription Subscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0)
        {
            var sub = EventDispatcher.Subscribe(handler, priority);
            subscriptions.Add(sub);
            return sub;
        }

        /// <summary>
        /// 订阅一次性同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <remarks>
        /// <para>订阅后，委托仅执行一次，执行完毕后自动取消订阅。</para>
        /// </remarks>
        public ISubscription SubscribeOnce<TEvent>(Action<TEvent> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeOnce(handler, priority);
            subscriptions.Add(sub);
            return sub;
        }

        /// <summary>
        /// 订阅一次性异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <remarks>
        /// <para>订阅后，委托仅执行一次，执行完毕后自动取消订阅。</para>
        /// </remarks>
        public ISubscription SubscribeOnce<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeOnce(handler, priority);
            subscriptions.Add(sub);
            return sub;
        }

        /// <summary>
        /// 订阅条件同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <remarks>
        /// <para>仅当 <paramref name="filter"/> 返回 true 时，才会调用 <paramref name="handler"/>。</para>
        /// </remarks>
        public ISubscription SubscribeIf<TEvent>(Predicate<TEvent> filter, Action<TEvent> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeIf(filter, handler, priority);
            subscriptions.Add(sub);
            return sub;
        }

        /// <summary>
        /// 订阅条件异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <remarks>
        /// <para>仅当 <paramref name="filter"/> 返回 true 时，才会调用 <paramref name="handler"/>。</para>
        /// </remarks>
        public ISubscription SubscribeIf<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            var sub = EventDispatcher.SubscribeIf(filter, handler, priority);
            subscriptions.Add(sub);
            return sub;
        }

        /// <summary>
        /// 订阅在 Unity 主线程运行的一次性异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <remarks>
        /// <para>订阅后，委托仅执行一次，执行完毕后自动取消订阅。</para>
        /// </remarks>
        public ISubscription SubscribeOnceOnMainThread<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            var sub = EventDispatcher.MainThread.SubscribeOnce(handler, priority);
            subscriptions.Add(sub);
            return sub;
        }

        /// <summary>
        /// 订阅在 Unity 主线程运行的条件异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <remarks>
        /// <para>仅当 <paramref name="filter"/> 返回 true 时，才会调用 <paramref name="handler"/>。</para>
        /// </remarks>
        public ISubscription SubscribeIfOnMainThread<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0) 
        {
            var sub = EventDispatcher.MainThread.SubscribeIf(filter, handler, priority);
            subscriptions.Add(sub);
            return sub;
        }

        /// <summary>
        /// 取消所有订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            try
            {
                foreach (var sub in subscriptions)
                    sub.Dispose();
                subscriptions.Clear();
            }
            catch (Exception ex) 
            {
                EventDispatcherLog.LogError($"The method '{nameof(UnsubscribeAll)}' triggered an exception: {ex.Message}");
            }
        }

        private void Awake()
        {
            hideFlags = HideFlags.HideInInspector | HideFlags.NotEditable;
        }

        private void OnDestroy()
        {
            UnsubscribeAll();
        }
    }
}

#endif