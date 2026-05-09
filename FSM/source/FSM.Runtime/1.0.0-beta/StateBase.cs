using System;

namespace FSM.Runtime
{
    /// <summary>
    /// 状态基类
    /// </summary>
    /// <remarks>
    /// <para>提供 <see cref="IState{TKey, TContext}"/> 的默认实现，三个生命周期方法均为空虚方法，子类按需重写。</para>
    /// <para>适合大多数状态实现场景，对更新逻辑有强制要求时可改用 <see cref="StateBehaviour{TContext}"/>。</para>
    /// <code>
    /// public class IdleState : StateBase&lt;MyContext&gt;
    /// {
    ///     public override string Name => "Idle";
    ///
    ///     public override void OnEnter(MyContext context)
    ///     {
    ///         // 初始化逻辑
    ///     }
    /// }
    /// </code>
    /// </remarks>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public abstract class StateBase<TContext> : IState<string, TContext> where TContext : class
    {
        /// <summary>
        /// 状态名称
        /// </summary>
        /// <remarks>
        /// <para>子类必须实现此属性以提供唯一的状态标识。</para>
        /// </remarks>
        public abstract string Key { get; }

        /// <summary>
        /// 进入状态回调
        /// </summary>
        /// <remarks>
        /// <para>默认实现为空，子类按需重写。</para>
        /// </remarks>
        /// <param name="context">共享上下文</param>
        public virtual void Enter(TContext context) { }

        /// <summary>
        /// 状态更新回调
        /// </summary>
        /// <remarks>
        /// <para>默认实现为空，子类按需重写。</para>
        /// </remarks>
        /// <param name="context">共享上下文</param>
        /// <param name="deltaTime">距上次更新的时间间隔</param>
        public virtual void Update(TContext context, TimeSpan deltaTime) { }

        /// <summary>
        /// 退出状态回调
        /// </summary>
        /// <remarks>
        /// <para>默认实现为空，子类按需重写。</para>
        /// </remarks>
        /// <param name="context">共享上下文</param>
        public virtual void Exit(TContext context) { }
    }
}
