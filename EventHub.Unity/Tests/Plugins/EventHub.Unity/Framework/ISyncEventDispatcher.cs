using System;
using UnityEngine.Scripting;

namespace EventHub
{
    /// <summary>
    /// 同步事件分发器接口
    /// </summary>
    /// <remarks>
    /// <para>提供订阅、取消订阅和发布同步事件的方法。</para>
    /// <para>框架级接口采用版本接口，稳定版本的接口不会在后续版本出现更改，且新版本兼容旧版本。</para>
    /// </remarks>
    [Preserve]
    public interface ISyncEventDispatcher
    {
        /// <summary>
        /// 订阅同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        ISubscription Subscribe<TEvent>(Action<TEvent> handler, int priority = 0);

        /// <summary>
        /// 取消订阅同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <returns>取消订阅的事件数量</returns>
        int Unsubscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// 发布同步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        void Publish<TEvent>(TEvent @event);
    }
}
