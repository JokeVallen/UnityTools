using System;
using System.Collections.Generic;

namespace FSM
{
    /// <summary>
    /// 附带上下文的状态机接口
    /// </summary>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TEvent">事件标识类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public interface IContextStateMachine<TKey, TEvent, TContext> : IStateMachine
    {
        /// <summary>
        /// 共享上下文
        /// </summary>
        TContext Context { get; }

        /// <summary>
        /// 当前状态
        /// </summary>
        IContextState<TKey, TContext> CurrentState { get; }

        /// <summary>
        /// 运行状态
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 已注册的状态列表
        /// </summary>
        IReadOnlyList<IContextState<TKey, TContext>> States { get; }

        /// <summary>
        /// 已注册的转换列表
        /// </summary>
        IReadOnlyList<IContextTransition<TKey, TEvent, TContext>> Transitions { get; }

        /// <summary>
        /// 状态变更事件
        /// </summary>
        event Action<IContextState<TKey, TContext>, IContextState<TKey, TContext>> OnStateChanged;

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
