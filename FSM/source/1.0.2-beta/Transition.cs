using System;

namespace FSM.Runtime
{
    /// <summary>
    /// 状态转换基类
    /// </summary>
    /// <remarks>
    /// <para>实现 <see cref="IExtendTransition{TKey, TEvent}"/>，提供完整的转换路径配置与条件判断。</para>
    /// <para>实例只能通过内部 <see cref="Builder"/> 构建，外部无法直接实例化。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TEvent">事件标识类型</typeparam>
    public class Transition<TKey, TEvent> : IExtendTransition<TKey, TEvent>, IResettable
    {
        /// <summary>
        /// 源状态标识
        /// </summary>
        /// <remarks>
        /// <para>通过 <see cref="Builder.Create"/> 设置，构建后不可修改。</para>
        /// </remarks>
        public TKey FromState { get; private set; }

        /// <summary>
        /// 目标状态标识
        /// </summary>
        /// <remarks>
        /// <para>通过 <see cref="Builder.Create"/> 设置，构建后不可修改。</para>
        /// </remarks>
        public TKey ToState { get; private set; }

        /// <summary>
        /// 优先级
        /// </summary>
        /// <remarks>
        /// <para>通过 <see cref="Builder.WithPriority"/> 设置，默认值为 0。</para>
        /// </remarks>
        public int Priority { get; private set; }

        /// <summary>
        /// 事件标识
        /// </summary>
        /// <remarks>
        /// <para>通过 <see cref="Builder.OnEvent"/> 设置。</para>
        /// </remarks>
        public TEvent EventKey { get; private set; }

        /// <summary>
        /// 退出时间
        /// </summary>
        /// <remarks>
        /// <para>通过 <see cref="Builder.WithExitTime"/> 设置，默认为 <c>null</c>。</para>
        /// </remarks>
        public TimeSpan? ExitTime { get; private set; }

        /// <summary>
        /// 转换延迟
        /// </summary>
        /// <remarks>
        /// <para>通过 <see cref="Builder.WithDelay"/> 设置，默认为 <c>null</c>。</para>
        /// </remarks>
        public TimeSpan? Delay { get; private set; }

        /// <summary>
        /// 单次触发
        /// </summary>
        /// <remarks>
        /// <para>通过 <see cref="Builder.OneShot"/> 设置，默认为 <c>false</c>。</para>
        /// </remarks>
        public bool IsOneShot { get; private set; }

        /// <summary>
        /// 是否为自动转换
        /// </summary>
        /// <remarks>
        /// <para>通过 <see cref="Builder.Auto"/> 设置，默认为 true。</para>
        /// </remarks>
        public bool Auto { get; private set; }

        internal TimeSpan DelayAccumulator { get; set; }
        internal bool ConditionMet { get; set; }
        internal bool HasTriggered { get; set; }
        private readonly Func<bool> condition;

        private Transition(Func<bool> condition)
        {
            this.condition = condition;
        }

        /// <summary>
        /// 转换条件判断
        /// </summary>
        /// <remarks>
        /// <para>条件未设置时默认返回 <c>true</c>，即无条件转换。</para>
        /// </remarks>
        /// <returns>是否允许转换</returns>
        public bool CanTransit()
        {
            if (condition == null) return true;
            return condition();
        }

        /// <summary>
        /// 重置状态转换类的内部状态
        /// </summary>
        public void Reset()
        {
            DelayAccumulator = TimeSpan.Zero;
            ConditionMet = false;
            HasTriggered = false;
        }

        /// <summary>
        /// 转换构建器
        /// </summary>
        /// <remarks>
        /// <para>用于构建 <see cref="Transition{TKey, TEvent}"/> 实例，通过链式调用配置转换属性。</para>
        /// <para>必须调用 <see cref="Create"/> 创建构建器实例，最后调用 <see cref="Build"/> 生成转换。</para>
        /// </remarks>
        public class Builder
        {
            private readonly TKey fromState;
            private readonly TKey toState;
            private Func<bool> condition;
            private int priority;
            private TEvent eventKey;
            private TimeSpan? exitTime;
            private TimeSpan? delay;
            private bool isOneShot;
            private bool auto = true;
            private bool built;

            private Builder(TKey fromState, TKey toState)
            {
                this.fromState = fromState;
                this.toState = toState;
            }

            /// <summary>
            /// 创建构建器实例
            /// </summary>
            /// <remarks>
            /// <para>构建器的唯一入口，同时设置转换的源状态和目标状态。</para>
            /// </remarks>
            /// <param name="fromState">源状态标识</param>
            /// <param name="toState">目标状态标识</param>
            /// <returns>构建器实例</returns>
            public static Builder Create(TKey fromState, TKey toState)
            {
                return new Builder(fromState, toState);
            }

            /// <summary>
            /// 设置转换条件
            /// </summary>
            /// <remarks>
            /// <para>未调用此方法时默认为无条件转换。</para>
            /// </remarks>
            /// <param name="condition">条件委托</param>
            /// <returns>当前构建器实例</returns>
            public Builder When(Func<bool> condition)
            {
                ThrowErrorIfBuilt();
                this.condition = condition;
                return this;
            }

            /// <summary>
            /// 设置优先级
            /// </summary>
            /// <remarks>
            /// <para>未调用此方法时默认优先级为 0。</para>
            /// </remarks>
            /// <param name="priority">优先级数值</param>
            /// <returns>当前构建器实例</returns>
            public Builder WithPriority(int priority)
            {
                ThrowErrorIfBuilt();
                this.priority = priority;
                return this;
            }

            /// <summary>
            /// 设置退出时间
            /// </summary>
            /// <remarks>
            /// <para>状态至少运行该时长后此转换才参与评估。</para>
            /// </remarks>
            /// <param name="exitTime">退出时间</param>
            /// <returns>当前构建器实例</returns>
            public Builder WithExitTime(TimeSpan exitTime)
            {
                ThrowErrorIfBuilt();
                this.exitTime = exitTime;
                return this;
            }

            /// <summary>
            /// 设置转换延迟
            /// </summary>
            /// <remarks>
            /// <para>条件满足后等待该时长再执行切换。</para>
            /// </remarks>
            /// <param name="delay">延迟时长</param>
            /// <returns>当前构建器实例</returns>
            public Builder WithDelay(TimeSpan delay)
            {
                ThrowErrorIfBuilt();
                this.delay = delay;
                return this;
            }

            /// <summary>
            /// 设置单次触发
            /// </summary>
            /// <remarks>
            /// <para>调用后该转换在状态机生命周期内只能触发一次。</para>
            /// </remarks>
            /// <returns>当前构建器实例</returns>
            public Builder OneShot()
            {
                ThrowErrorIfBuilt();
                isOneShot = true;
                return this;
            }

            /// <summary>
            /// 设置为自动转换
            /// </summary>
            /// <remarks>
            /// <para>将事件名称重置为 <c>null</c>，每次 <c>Update</c> 自动评估条件。</para>
            /// </remarks>
            /// <returns>当前构建器实例</returns>
            public Builder Auto()
            {
                ThrowErrorIfBuilt();
                auto = true;
                return this;
            }

            /// <summary>
            /// 设置为事件驱动转换
            /// </summary>
            /// <remarks>
            /// <para>仅在通过 <c>SendEvent</c> 发送匹配事件时才评估条件。</para>
            /// </remarks>
            /// <param name="eventKey">事件标识</param>
            /// <returns>当前构建器实例</returns>
            public Builder OnEvent(TEvent eventKey)
            {
                ThrowErrorIfBuilt();
                this.eventKey = eventKey;
                auto = false;
                return this;
            }

            /// <summary>
            /// 构建转换实例
            /// </summary>
            /// <remarks>
            /// <para>根据当前配置生成 <see cref="Transition{TKey, TEvent}"/> 实例。</para>
            /// </remarks>
            /// <returns>转换实例</returns>
            public Transition<TKey, TEvent> Build()
            {
                ThrowErrorIfBuilt();
                built = true;
                return new Transition<TKey, TEvent>(condition)
                {
                    FromState = fromState,
                    ToState = toState,
                    Priority = priority,
                    EventKey = eventKey,
                    ExitTime = exitTime,
                    Delay = delay,
                    IsOneShot = isOneShot,
                    Auto = auto
                };
            }

            private void ThrowErrorIfBuilt()
            {
                if (built)
                    throw new StateMachineException("The transition builder cannot be reuesd.");
            }
        }
    }
}