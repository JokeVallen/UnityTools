using System;
using System.Collections.Generic;

namespace FSM
{
    /// <summary>
    /// 状态机接口
    /// </summary>
    /// <remarks>
    /// <para>管理状态集合与转换规则，驱动状态的切换与生命周期。</para>
    /// <para>状态机需手动驱动，调用方通过 <see cref="Update"/> 推进状态逻辑与转换评估。</para>
    /// <para>生命周期顺序为：<see cref="Start"/> → 若干次 <see cref="Update"/> → <see cref="Stop"/>，可通过 <see cref="Reset"/> 回到初始状态后重新 <see cref="Start"/>。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public interface IStateMachine<TKey, TContext> where TContext : class
    {
        /// <summary>
        /// 共享上下文
        /// </summary>
        /// <remarks>
        /// <para>所有状态和转换共享的数据对象，用于在状态之间传递业务数据。</para>
        /// </remarks>
        TContext Context { get; }

        /// <summary>
        /// 当前状态
        /// </summary>
        /// <remarks>
        /// <para>状态机启动后指向当前活跃的状态实例。</para>
        /// <para>在 <see cref="Start"/> 之前或 <see cref="Reset"/> 之后为 <c>null</c>。</para>
        /// </remarks>
        IState<TKey, TContext> CurrentState { get; }

        /// <summary>
        /// 运行状态
        /// </summary>
        /// <remarks>
        /// <para><see cref="Start"/> 调用后为 <c>true</c>，<see cref="Stop"/> 或 <see cref="Reset"/> 调用后为 <c>false</c>。</para>
        /// </remarks>
        bool IsRunning { get; }

        /// <summary>
        /// 已注册的状态列表
        /// </summary>
        /// <remarks>
        /// <para>按注册顺序排列，仅供读取，不可修改。</para>
        /// </remarks>
        IReadOnlyList<IState<TKey, TContext>> States { get; }

        /// <summary>
        /// 已注册的转换列表
        /// </summary>
        /// <remarks>
        /// <para>按优先级升序排列，仅供读取，不可修改。</para>
        /// </remarks>
        IReadOnlyList<ITransition<TKey, TContext>> Transitions { get; }

        /// <summary>
        /// 状态变更事件
        /// </summary>
        /// <remarks>
        /// <para>每次状态切换完成后触发，包括自动转换、事件驱动转换和强制切换。</para>
        /// <para>第一个参数为切换前的状态，第二个参数为切换后的状态。</para>
        /// <para>在 <see cref="OnStateChanged"/> 回调中调用 <see cref="SendEvent"/> 将被静默忽略。</para>
        /// </remarks>
        event Action<IState<TKey, TContext>, IState<TKey, TContext>> OnStateChanged;

        /// <summary>
        /// 启动事件
        /// </summary>
        /// <remarks>
        /// <para>状态机成功启动并进入初始状态后触发。</para>
        /// </remarks>
        event Action OnStarted;

        /// <summary>
        /// 停止事件
        /// </summary>
        /// <remarks>
        /// <para>状态机停止后触发，包括主动调用 <see cref="Stop"/> 和通过 <see cref="Reset"/> 触发的停止。</para>
        /// </remarks>
        event Action OnStopped;

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <remarks>
        /// <para>将状态机切换到运行状态，进入初始状态并触发其 <c>OnEnter</c>，随后触发 <see cref="OnStarted"/>。</para>
        /// </remarks>
        void Start();

        /// <summary>
        /// 更新状态机
        /// </summary>
        /// <remarks>
        /// <para>驱动当前状态执行 <c>OnUpdate</c>，并在之后评估所有自动转换条件。</para>
        /// <para>状态机未运行时调用无效果。</para>
        /// </remarks>
        /// <param name="deltaTime">距上次更新的时间间隔</param>
        void Update(TimeSpan deltaTime);

        /// <summary>
        /// 停止状态机
        /// </summary>
        /// <remarks>
        /// <para>触发当前状态的 <c>OnExit</c>，将状态机切换到停止状态，随后触发 <see cref="OnStopped"/>。</para>
        /// <para>状态机未运行时调用无效果。</para>
        /// </remarks>
        void Stop();

        /// <summary>
        /// 重置状态机
        /// </summary>
        /// <remarks>
        /// <para>停止状态机并清空当前状态，重置所有转换的运行时数据。</para>
        /// <para>重置后可重新调用 <see cref="Start"/> 从初始状态启动。</para>
        /// </remarks>
        void Reset();

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <remarks>
        /// <para>触发所有匹配该事件名称的事件驱动转换的条件评估。</para>
        /// <para>状态机未运行时调用无效果。</para>
        /// <para>在状态切换过程中或 <see cref="OnStateChanged"/> 回调中调用将被静默忽略。</para>
        /// </remarks>
        /// <param name="eventName">事件名称</param>
        void SendEvent(string eventName);

        /// <summary>
        /// 强制切换状态
        /// </summary>
        /// <remarks>
        /// <para>忽略所有转换条件，直接切换到目标状态，依次触发当前状态 <c>OnExit</c> 和目标状态 <c>OnEnter</c>。</para>
        /// </remarks>
        /// <param name="stateKey">目标状态标识</param>
        void ForceTransition(TKey stateKey);
    }
}
