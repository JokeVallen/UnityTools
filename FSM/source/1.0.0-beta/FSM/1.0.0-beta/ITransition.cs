namespace FSM
{
    /// <summary>
    /// 状态转换接口
    /// </summary>
    /// <remarks>
    /// <para>描述从一个状态到另一个状态的转换路径。</para>
    /// <para>转换分为两种触发方式：自动转换（<see cref="EventName"/> 为 <c>null</c>）和事件驱动转换（<see cref="EventName"/> 有值）。</para>
    /// <para>自动转换在每次 <c>Update</c> 时评估条件；事件驱动转换仅在匹配的事件发送时评估条件。</para>
    /// <para>当多条转换同时满足条件时，按 <see cref="Priority"/> 升序取优先级最高者执行，同优先级按注册顺序。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public interface ITransition<TKey, TContext> where TContext : class
    {
        /// <summary>
        /// 源状态标识
        /// </summary>
        /// <remarks>
        /// <para>转换的起始状态标识，仅当状态机当前状态的标识与该标识匹配时，此转换才参与评估。</para>
        /// </remarks>
        TKey FromState { get; }

        /// <summary>
        /// 目标状态标识
        /// </summary>
        /// <remarks>
        /// <para>转换成功后切换到的目标状态标识，必须已在状态机中注册。</para>
        /// </remarks>
        TKey ToState { get; }

        /// <summary>
        /// 优先级
        /// </summary>
        /// <remarks>
        /// <para>数值越小优先级越高，多条转换同时满足条件时优先执行数值最小的转换。</para>
        /// <para>默认值为 0，同优先级按注册顺序评估。</para>
        /// </remarks>
        int Priority { get; }

        /// <summary>
        /// 事件名称
        /// </summary>
        /// <remarks>
        /// <para>为 <c>null</c> 时表示自动转换，每次 <c>Update</c> 自动评估条件。</para>
        /// <para>有值时表示事件驱动转换，仅在通过 <c>SendEvent</c> 发送匹配事件时才评估条件。</para>
        /// </remarks>
        string EventName { get; }

        /// <summary>
        /// 转换条件判断
        /// </summary>
        /// <remarks>
        /// <para>返回 <c>true</c> 时允许执行转换，返回 <c>false</c> 时跳过该转换继续评估下一条。</para>
        /// <para>条件未设置时默认返回 <c>true</c>，即无条件转换。</para>
        /// </remarks>
        /// <param name="context">共享上下文</param>
        /// <returns>是否允许转换</returns>
        bool CanTransit(TContext context);
    }
}
