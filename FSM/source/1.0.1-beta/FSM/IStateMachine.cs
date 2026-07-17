using System;
using System.Collections.Generic;

namespace FSM
{
    /// <summary>
    /// 状态机统一接口
    /// </summary>
    public interface IStateMachine { }

    /// <summary>
    /// 状态机接口
    /// </summary>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TEvent">事件标识类型</typeparam>
    public interface IStateMachine<TKey, TEvent> : IStateMachine
    {
        /// <summary>
        /// 当前状态
        /// </summary>
        IState<TKey> CurrentState { get; }

        /// <summary>
        /// 运行状态
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 已注册的状态列表
        /// </summary>
        IReadOnlyList<IState<TKey>> States { get; }

        /// <summary>
        /// 已注册的转换列表
        /// </summary>
        IReadOnlyList<ITransition<TKey, TEvent>> Transitions { get; }

        /// <summary>
        /// 状态变更事件
        /// </summary>
        event Action<IState<TKey>, IState<TKey>> OnStateChanged;

        /// <summary>
        /// 启动事件
        /// </summary>
        event Action OnStarted;

        /// <summary>
        /// 停止事件
        /// </summary>
        event Action OnStopped;

        /// <summary>
        /// 启动状态机
        /// </summary>
        void Start();

        /// <summary>
        /// 更新状态机
        /// </summary>
        /// <param name="deltaTime">更新时间间隔</param>
        void Update(TimeSpan deltaTime);

        /// <summary>
        /// 停止状态机
        /// </summary>
        void Stop();

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventKey">事件标识</param>
        void SendEvent(TEvent eventKey);

        /// <summary>
        /// 强制切换状态
        /// </summary>
        /// <param name="stateKey">目标状态标识</param>
        void ForceTransition(TKey stateKey);
    }
}
