using System;

namespace FSM.Runtime
{
    /// <summary>
    /// 状态转换的扩展接口
    /// </summary>
    /// <remarks>
    /// <para>在 <see cref="ITransition{TKey, TEvent}"/> 基础上扩展了转换行为配置，提供退出时间、延迟和单次触发三项增强特性。</para>
    /// <para>通过 <see cref="Transition{TKey, TEvent}"/> 的内部 Builder 构建的转换均实现此接口。</para>
    /// <para>当外部持有 <see cref="IStateMachine{TKey, TEvent}.Transitions"/> 列表时，可将元素转换为此接口以访问扩展配置。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识</typeparam>
    /// <typeparam name="TEvent">事件标识类型</typeparam>
    public interface IExtendTransition<TKey, TEvent> : ITransition<TKey, TEvent>
    {
        /// <summary>
        /// 退出时间
        /// </summary>
        /// <remarks>
        /// <para>状态至少运行该时长后，此转换才开始参与评估。</para>
        /// <para>为 <c>null</c> 时不限制，进入状态后立即参与评估。</para>
        /// </remarks>
        TimeSpan? ExitTime { get; }

        /// <summary>
        /// 转换延迟
        /// </summary>
        /// <remarks>
        /// <para>条件满足后等待该时长再执行切换，等待期间条件必须持续满足，否则重新计时。</para>
        /// <para>为 <c>null</c> 时不延迟，条件满足后立即执行切换。</para>
        /// </remarks>
        TimeSpan? Delay { get; }

        /// <summary>
        /// 单次触发
        /// </summary>
        /// <remarks>
        /// <para>为 <c>true</c> 时该转换在整个状态机生命周期内只能触发一次，触发后永久失效直到状态机 <c>Reset</c>。</para>
        /// <para>为 <c>false</c> 时每次进入源状态后均可触发。</para>
        /// </remarks>
        bool IsOneShot { get; }

        /// <summary>
        /// 是否为自动转换
        /// </summary>
        bool Auto { get; }
    }
}
