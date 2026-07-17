namespace FSM
{
    /// <summary>
    /// 附带上下文的状态转换接口
    /// </summary>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TEvent">事件标识类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public interface IContextTransition<TKey, TEvent, TContext> : ITransition
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
        /// 事件名称
        /// </summary>
        TEvent EventKey{ get; }

        /// <summary>
        /// 转换条件判断
        /// </summary>
        /// <param name="context">共享上下文</param>
        /// <returns>是否允许转换</returns>
        bool CanTransit(TContext context);
    }
}
