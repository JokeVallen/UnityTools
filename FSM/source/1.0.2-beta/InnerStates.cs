using System.Collections.Generic;

namespace FSM.Runtime
{
    /// <summary>
    /// 内置状态标识
    /// </summary>
    /// <remarks>
    /// <para>定义内部使用的保留状态标识，这些标识不得用于用户自定义状态。</para>
    /// <para>可通过 <see cref="IsInnerState"/> 在注册状态前校验标识合法性。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    public static class InnerStates<TKey>
    {
        /// <summary>
        /// 代表任意状态的标识
        /// </summary>
        /// <remarks>
        /// <para>用于定义从任意当前状态均可触发的转换，常用于全局事件处理，例如死亡、暂停等。</para>
        /// <para>以此为 <c>FromState</c> 注册的转换，无论状态机当前处于何种状态都会参与评估，且优先于当前状态的转换。</para>
        /// </remarks>
        public static TKey AnyState
        {
            get
            {
                if ((flags & 1 << 0) == 0) throw new System.InvalidOperationException($"The '{nameof(AnyState)}' has never been set.");
                return anyState;
            }
            set 
            {
                flags |= 1 << 0;
                anyState = value;
            }
        }
        private static TKey anyState;

        private static int flags;

        /// <summary>
        /// 判断是否为内置保留状态标识
        /// </summary>
        /// <remarks>
        /// <para>用于在注册状态前校验标识合法性，防止用户自定义状态与内置保留状态标识冲突。</para>
        /// <para>建议在自定义 <see cref="IContextState{TKey,TContext}"/> 实现或 Builder 注册时调用。</para>
        /// </remarks>
        /// <param name="state">待校验的状态标识</param>
        /// <returns>是否为内置保留状态标识</returns>
        public static bool IsInnerState(TKey state) => (flags & 1 << 0) != 0 && EqualityComparer<TKey>.Default.Equals(state, AnyState);
    }
}
