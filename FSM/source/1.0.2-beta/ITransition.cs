namespace FSM
{
    /// <summary>
    /// 状态转换统一接口
    /// </summary>
    public interface ITransition { }

    /// <summary>
    /// 状态转换接口
    /// </summary>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TEvent">事件标识类型</typeparam>
    public interface ITransition<TKey, TEvent> : ITransition
    {
        /// <summary>
        /// 源状态标识
        /// </summary>
        TKey FromState { get; }

        /// <summary>
        /// 目标状态标识
        /// </summary>
        TKey ToState { get; }

        /// <summary>
        /// 优先级
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 事件标识
        /// </summary>
        TEvent EventKey { get; }

        /// <summary>
        /// 转换条件判断
        /// </summary>
        /// <returns>是否允许转换</returns>
        bool CanTransit();
    }
}
