using System;

namespace FSM
{
    /// <summary>
    /// 附带上下文的状态接口
    /// </summary>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public interface IContextState<TKey, TContext> : IState
    {
        /// <summary>
        /// 状态标识
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// 进入状态回调
        /// </summary>
        /// <param name="context">共享上下文</param>
        void Enter(TContext context);

        /// <summary>
        /// 状态更新回调
        /// </summary>
        /// <param name="context">共享上下文</param>
        /// <param name="deltaTime">距上次更新的时间间隔</param>
        void Update(TContext context, TimeSpan deltaTime);

        /// <summary>
        /// 退出状态回调
        /// </summary>
        /// <param name="context">共享上下文</param>
        void Exit(TContext context);
    }
}
