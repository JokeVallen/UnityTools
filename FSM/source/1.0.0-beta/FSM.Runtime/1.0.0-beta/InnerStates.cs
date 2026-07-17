namespace FSM.Runtime
{
    /// <summary>
    /// 内置状态常量
    /// </summary>
    /// <remarks>
    /// <para>定义内部使用的保留状态名称，这些名称不得用于用户自定义状态。</para>
    /// <para>可通过 <see cref="IsInnerState"/> 在注册状态前校验名称合法性。</para>
    /// </remarks>
    public static class InnerStates
    {
        /// <summary>
        /// 任意状态
        /// </summary>
        /// <remarks>
        /// <para>用于定义从任意当前状态均可触发的转换，常用于全局事件处理，例如死亡、暂停等。</para>
        /// <para>以此为 <c>FromState</c> 注册的转换，无论状态机当前处于何种状态都会参与评估，且优先于当前状态的转换。</para>
        /// <code>
        /// var transition = Transition&lt;MyContext&gt;.Builder
        ///     .Create(InnerStates.AnyState, "Dead")
        ///     .OnEvent("Die")
        ///     .Build();
        /// </code>
        /// </remarks>
        public const string AnyState = "__Any__";

        /// <summary>
        /// 判断是否为内置保留状态名称
        /// </summary>
        /// <remarks>
        /// <para>用于在注册状态前校验名称合法性，防止用户自定义状态与内置保留名称冲突。</para>
        /// <para>建议在自定义 <see cref="IState{TKey,TContext}"/> 实现或 Builder 注册时调用。</para>
        /// <code>
        /// if (InnerStates.IsInnerState(myStateName))
        ///     throw new InvalidOperationException("名称与内置保留状态冲突");
        /// </code>
        /// </remarks>
        /// <param name="name">待校验的状态名称</param>
        /// <returns>是否为内置保留名称</returns>
        public static bool IsInnerState(string name) => name == AnyState;
    }
}
