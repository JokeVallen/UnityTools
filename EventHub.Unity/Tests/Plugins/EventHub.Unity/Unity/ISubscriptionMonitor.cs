#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE

using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// 事件订阅句柄监视器
    /// </summary>
    [Preserve]
    public interface ISubscriptionMonitor: IDisposable
    {
        /// <summary>
        /// 开启自动清理计时器
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        void StartTimer(CancellationToken cancellationToken = default);

        /// <summary>
        /// 停止自动清理计时器
        /// </summary>
        void StopTimer();

        /// <summary>
        /// 注册组件及与之关联的事件订阅句柄
        /// </summary>
        /// <param name="component">组件</param>
        /// <param name="subscription">事件订阅句柄</param>
        void Register(Component component, ISubscription subscription);

        /// <summary>
        /// 注册组件及与之关联的事件订阅句柄
        /// </summary>
        /// <param name="component">组件</param>
        /// <param name="subscription1">事件订阅句柄1</param>
        /// <param name="subscription2">事件订阅句柄2</param>
        void Register(Component component, ISubscription subscription1, ISubscription subscription2);

        /// <summary>
        /// 注册组件及与之关联的事件订阅句柄
        /// </summary>
        /// <param name="component">组件</param>
        /// <param name="subscriptions">三个及以上的事件句柄</param>
        void Register(Component component, params ISubscription[] subscriptions);

        /// <summary>
        /// 为与指定组件关联的所有事件订阅句柄取消订阅
        /// </summary>
        /// <param name="component">组件</param>
        void UnsubscribeAll(Component component);
    }
}

#endif