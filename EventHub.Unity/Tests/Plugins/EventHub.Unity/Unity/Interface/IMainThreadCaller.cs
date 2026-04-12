#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// Unity 主线程调用接口
    /// </summary>
    /// <remarks>
    /// <para>该接口用于为涉及使用 Unity API 的行为提供与事件系统的适配。</para>
    /// <para>通过该接口调用的方法你可以安全使用 Unity API 而无需考虑线程切换的问题。</para>
    /// </remarks>
    [Preserve]
    public interface IMainThreadCaller
    {
        /// <summary>
        /// 异步串行发布异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <remarks>
        /// <para>按优先级降序，异步串行执行所有订阅者，等待前一个完成后再执行下一个。</para>
        /// <para>基于快照的发布模式。</para>
        /// <para>若 <paramref name="cancellationToken"/> 被取消，则停止后续订阅者执行。</para>
        /// <para>示例：</para>
        /// <code>
        /// await EventDispatcher.PublishAsync(new MyEvent());
        /// </code>
        /// </remarks>
        UniTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步并行发布异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <remarks>
        /// <para>所有订阅者并发执行，不保证顺序。</para>
        /// <para>若任意订阅者抛出异常（<see cref="OperationCanceledException"/> 除外），则会聚合为 <see cref="AggregateException"/> 抛出。</para>
        /// <para>基于快照的发布模式。</para>
        /// <para>示例：</para>
        /// <code>
        /// await EventDispatcher.PublishParallelAsync(new MyEvent());
        /// </code>
        /// </remarks>
        UniTask PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);

        /// <summary>
        /// 订阅一次性异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <remarks>
        /// <para>订阅后，委托仅执行一次，执行完毕后自动取消订阅。</para>
        /// <para>示例：</para>
        /// <code>
        /// EventDispatcher.SubscribeOnce&lt;MyEvent&gt;(async (e, ct) => { /* 只执行一次 */ });
        /// </code>
        /// </remarks>
        ISubscription SubscribeOnce<TEvent>(Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);

        /// <summary>
        /// 订阅条件异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="filter">前置过滤条件</param>
        /// <param name="handler">事件委托</param>
        /// <param name="priority">优先级</param>
        /// <remarks>
        /// <para>仅当 <paramref name="filter"/> 返回 true 时，才会调用 <paramref name="handler"/>。</para>
        /// <para>示例：</para>
        /// <code>
        /// EventDispatcher.SubscribeIf&lt;MyEvent&gt;(e => e.Value > 10, async (e, ct) => { });
        /// </code>
        /// </remarks>
        ISubscription SubscribeIf<TEvent>(Predicate<TEvent> filter, Func<TEvent, CancellationToken, UniTask> handler, int priority = 0);
    }
}

#endif