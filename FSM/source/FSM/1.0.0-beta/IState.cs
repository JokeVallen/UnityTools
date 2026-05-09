using System;

namespace FSM
{
    /// <summary>
    /// 状态接口
    /// </summary>
    /// <remarks>
    /// <para>状态机的基本组成单元，表示一个独立的状态。</para>
    /// <para>每个状态具有唯一名称，并提供三个生命周期回调：进入、更新、退出。</para>
    /// <para>泛型参数 <typeparamref name="TContext"/> 为状态机共享的上下文类型，所有生命周期回调均可访问该上下文。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public interface IState<TKey, TContext> where TContext : class
    {
        /// <summary>
        /// 状态标识
        /// </summary>
        /// <remarks>
        /// <para>在同一状态机中唯一标识该状态，用于注册、查找和转换目标的匹配。</para>
        /// </remarks>
        TKey Key { get; }

        /// <summary>
        /// 进入状态回调
        /// </summary>
        /// <remarks>
        /// <para>当状态机切换到该状态时触发一次，可用于初始化状态相关数据。</para>
        /// <para>在上一个状态的 <see cref="Exit"/> 执行完毕后调用。</para>
        /// </remarks>
        /// <param name="context">共享上下文</param>
        void Enter(TContext context);

        /// <summary>
        /// 状态更新回调
        /// </summary>
        /// <remarks>
        /// <para>每次状态机 <c>Update</c> 时触发，在转换评估之前执行。</para>
        /// <para>可用于持续性逻辑处理，例如计时、轮询、行为驱动等。</para>
        /// </remarks>
        /// <param name="context">共享上下文</param>
        /// <param name="deltaTime">距上次更新的时间间隔</param>
        void Update(TContext context, TimeSpan deltaTime);

        /// <summary>
        /// 退出状态回调
        /// </summary>
        /// <remarks>
        /// <para>当状态机离开该状态时触发一次，在下一个状态的 <see cref="Enter"/> 之前执行。</para>
        /// <para>可用于清理状态相关资源或重置临时数据。</para>
        /// </remarks>
        /// <param name="context">共享上下文</param>
        void Exit(TContext context);
    }
}
