using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub
{
    /// <summary>
    /// 异步事件分发器接口
    /// </summary>
    /// <remarks>
    /// <para>提供订阅、取消订阅和发布异步事件的方法。</para>
    /// <para>框架级接口采用版本接口，稳定版本的接口不会在后续版本出现更改，且新版本兼容旧版本。</para>
    /// </remarks>
    [Preserve]
    public interface IAsyncEventDispatcher
    {
        /// <summary>
        /// 订阅异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <returns>事件订阅句柄</returns>
        ISubscription Subscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);

        /// <summary>
        /// 取消订阅异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <returns>取消订阅的事件数量</returns>
        int Unsubscribe<TEvent>(Func<TEvent, CancellationToken, UniTask> handler);

        /// <summary>
        /// 异步串行发布异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        UniTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
    }
}