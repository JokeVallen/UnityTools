using System;

namespace FSM
{
    /// <summary>
    /// 状态统一接口
    /// </summary>
    public interface IState { }

    /// <summary>
    /// 状态接口
    /// </summary>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    public interface IState<TKey> : IState
    {
        /// <summary>
        /// 状态标识
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// 进入状态回调
        /// </summary>
        void Enter();

        /// <summary>
        /// 状态更新回调
        /// </summary>
        /// <param name="deltaTime">更新时间间隔</param>
        void Update(TimeSpan deltaTime);

        /// <summary>
        /// 退出状态回调
        /// </summary>
        void Exit();
    }
}
