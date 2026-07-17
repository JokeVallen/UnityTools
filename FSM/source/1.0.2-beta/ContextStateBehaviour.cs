using System;

namespace FSM.Runtime
{
    /// <summary>
    /// 附带上下文的状态行为基类
    /// </summary>
    /// <remarks>
    /// <para>在 <see cref="ContextStateBase{TKey, TContext}"/> 基础上强制要求子类实现 <see cref="ContextStateBase{TKey, TContext}.Update"/>，适合对更新逻辑有强制约束的场景。</para>
    /// <para>若状态不需要更新逻辑，应使用 <see cref="ContextStateBase{TKey, TContext}"/> 而非此类。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public abstract class ContextStateBehaviour<TKey, TContext> : ContextStateBase<TKey, TContext>
    {
        /// <summary>
        /// 状态更新回调
        /// </summary>
        /// <remarks>
        /// <para>子类必须实现此方法以提供状态的持续更新逻辑。</para>
        /// </remarks>
        /// <param name="context">共享上下文</param>
        /// <param name="deltaTime">距上次更新的时间间隔</param>
        public abstract override void Update(TContext context, TimeSpan deltaTime);
    }
}
