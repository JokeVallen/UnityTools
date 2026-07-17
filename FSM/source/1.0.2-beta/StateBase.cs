using System;

namespace FSM.Runtime
{
    /// <summary>
    /// 附带上下文的状态基类
    /// </summary>
    /// <remarks>
    /// <para>提供 <see cref="IState{TKey}"/> 的默认实现，三个生命周期方法均为虚方法，子类按需重写。</para>
    /// <para>适合大多数状态实现场景，对更新逻辑有强制要求时可改用 <see cref="StateBehaviour{TKey}"/>。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识</typeparam>
    public abstract class StateBase<TKey> : IState<TKey>, IResettable
    {
        /// <summary>
        /// 状态标识
        /// </summary>
        /// <remarks>
        /// <para>子类必须实现此属性以提供唯一的状态标识。</para>
        /// </remarks>
        public abstract TKey Key { get; }

        /// <summary>
        /// 进入状态回调
        /// </summary>
        /// <remarks>
        /// <para>默认实现为空，子类按需重写。</para>
        /// </remarks>
        public virtual void Enter() { }

        /// <summary>
        /// 状态更新回调
        /// </summary>
        /// <remarks>
        /// <para>默认实现为空，子类按需重写。</para>
        /// </remarks>
        /// <param name="deltaTime">距上次更新的时间间隔</param>
        public virtual void Update(TimeSpan deltaTime) { }

        /// <summary>
        /// 退出状态回调
        /// </summary>
        /// <remarks>
        /// <para>默认实现为空，子类按需重写。</para>
        /// </remarks>
        public virtual void Exit() { }

        /// <summary>
        /// 重置状态类的内部状态
        /// </summary>
        public virtual void Reset() { }
    }
}
