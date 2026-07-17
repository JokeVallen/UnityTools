using System;
using System.Collections.Generic;
using System.Linq;

namespace FSM.Runtime
{
    /// <summary>
    /// 状态机
    /// </summary>
    /// <remarks>
    /// <para>实现 <see cref="IStateMachine{TKey, TEvent}"/>，管理状态集合与转换规则，驱动状态的切换与生命周期。</para>
    /// <para>实例只能通过内部 <see cref="Builder"/> 构建，外部无法直接实例化。</para>
    /// </remarks>
    /// <typeparam name="TKey">状态标识类型</typeparam>
    /// <typeparam name="TEvent">事件标识类型</typeparam>
    public class StateMachine<TKey, TEvent> : IStateMachine<TKey, TEvent>, IResettable
    {
        /// <summary>
        /// 当前状态
        /// </summary>
        /// <remarks>
        /// <para>在 <see cref="Start"/> 之前或 <see cref="Reset"/> 之后为 <c>null</c>。</para>
        /// </remarks>
        public IState<TKey> CurrentState { get; private set; }

        /// <summary>
        /// 运行状态
        /// </summary>
        /// <remarks>
        /// <para><see cref="Start"/> 调用后为 <c>true</c>，<see cref="Stop"/> 或 <see cref="Reset"/> 调用后为 <c>false</c>。</para>
        /// </remarks>
        public bool IsRunning => isRunning;

        /// <summary>
        /// 已注册的状态列表
        /// </summary>
        /// <remarks>
        /// <para>按注册顺序排列，仅供读取，不可修改。</para>
        /// </remarks>
        public IReadOnlyList<IState<TKey>> States => stateList;

        /// <summary>
        /// 已注册的转换列表
        /// </summary>
        /// <remarks>
        /// <para>按优先级升序排列，仅供读取，不可修改。</para>
        /// </remarks>
        public IReadOnlyList<ITransition<TKey, TEvent>> Transitions => transitionList;

        /// <summary>
        /// 状态变更事件
        /// </summary>
        /// <remarks>
        /// <para>每次状态切换完成后触发，第一个参数为切换前的状态，第二个参数为切换后的状态。</para>
        /// <para>在此回调中调用 <see cref="SendEvent"/> 将被静默忽略。</para>
        /// </remarks>
        public event Action<IState<TKey>, IState<TKey>> OnStateChanged;

        /// <summary>
        /// 启动事件
        /// </summary>
        /// <remarks>
        /// <para>状态机成功启动并进入初始状态后触发。</para>
        /// </remarks>
        public event Action OnStarted;

        /// <summary>
        /// 停止事件
        /// </summary>
        /// <remarks>
        /// <para>状态机停止后触发，包括主动调用 <see cref="Stop"/> 和通过 <see cref="Reset"/> 触发的停止。</para>
        /// </remarks>
        public event Action OnStopped;

        private readonly Dictionary<TKey, StateBase<TKey>> states;
        private readonly List<Transition<TKey, TEvent>> transitions;
        private readonly TKey initialState;
        private readonly IReadOnlyList<StateBase<TKey>> stateList;
        private readonly IReadOnlyList<ITransition<TKey, TEvent>> transitionList;
        private bool isTransitioning;
        private bool isRunning;
        private TimeSpan currentStateElapsed;

        private StateMachine(
            Dictionary<TKey, StateBase<TKey>> states,
            List<StateBase<TKey>> orderedStates,
            List<Transition<TKey, TEvent>> transitions,
            TKey initialState)
        {
            this.states = states;
            this.initialState = initialState;
            this.transitions = transitions.OrderBy(t => t.Priority).ToList();
            stateList = orderedStates.AsReadOnly();
            transitionList = this.transitions.AsReadOnly();
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <remarks>
        /// <para>进入初始状态并触发其 <c>OnEnter</c>，随后触发 <see cref="OnStarted"/>。</para>
        /// <para>重复调用将抛出 <see cref="StateMachineException"/>。</para>
        /// </remarks>
        public void Start()
        {
            if (isRunning)
                throw new StateMachineException("StateMachine is already running.");

            if (!states.ContainsKey(initialState))
                throw new StateMachineException($"Initial state '{initialState}' not found.");

            isRunning = true;
            currentStateElapsed = TimeSpan.Zero;
            ResetAllTransitions();
            EnterState(states[initialState]);
            OnStarted?.Invoke();
        }

        /// <summary>
        /// 更新状态机
        /// </summary>
        /// <remarks>
        /// <para>驱动当前状态执行 <c>OnUpdate</c>，并在之后评估所有自动转换条件。</para>
        /// <para>状态机未运行时调用无效果。</para>
        /// </remarks>
        /// <param name="deltaTime">距上次更新的时间间隔</param>
        public void Update(TimeSpan deltaTime)
        {
            if (!isRunning) return;

            currentStateElapsed += deltaTime;
            CurrentState.Update(deltaTime);
            EvaluateAutoTransitions(deltaTime);
        }

        /// <summary>
        /// 停止状态机
        /// </summary>
        /// <remarks>
        /// <para>触发当前状态的 <c>OnExit</c>，随后触发 <see cref="OnStopped"/>。</para>
        /// <para>状态机未运行时调用无效果。</para>
        /// </remarks>
        public void Stop()
        {
            if (!isRunning) return;
            CurrentState?.Exit();
            isRunning = false;
            OnStopped?.Invoke();
        }

        /// <summary>
        /// 重置状态机
        /// </summary>
        /// <remarks>
        /// <para>停止状态机并清空当前状态，重置所有转换的运行时数据。</para>
        /// <para>重置后可重新调用 <see cref="Start"/> 从初始状态启动。</para>
        /// </remarks>
        public void Reset()
        {
            Stop();
            CurrentState = null;
            isTransitioning = false;
            isRunning = false;
            currentStateElapsed = TimeSpan.Zero;
            ResetAllTransitions();
            ResetAllStates();
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <remarks>
        /// <para>触发所有匹配该事件名称的事件驱动转换的条件评估。</para>
        /// <para>状态机未运行时或转换执行期间调用将被静默忽略。</para>
        /// </remarks>
        /// <param name="eventKey">事件标识</param>
        public void SendEvent(TEvent eventKey)
        {
            if (!isRunning) return;

            if (isTransitioning) return;

            EvaluateEventTransitions(eventKey);
        }

        /// <summary>
        /// 强制切换状态
        /// </summary>
        /// <remarks>
        /// <para>忽略所有转换条件，直接切换到目标状态。</para>
        /// <para>在转换执行期间调用将抛出 <see cref="StateMachineException"/>。</para>
        /// <para>目标状态未注册时将抛出 <see cref="StateMachineException"/>。</para>
        /// </remarks>
        /// <param name="stateKey">目标状态标识</param>
        public void ForceTransition(TKey stateKey)
        {
            if (isTransitioning)
                throw new StateMachineException("Cannot force transition while a transition is in progress.");

            if (!states.ContainsKey(stateKey))
                throw new StateMachineException($"State '{stateKey}' not found.");

            ExecuteTransition(states[stateKey], null);
        }

        private void EvaluateAutoTransitions(TimeSpan deltaTime)
        {
            if (isTransitioning) return;

            var candidate = FindAutoTransitionCandidate(deltaTime);
            if (candidate != null)
                ExecuteTransition(states[candidate.ToState], candidate);
        }

        private void EvaluateEventTransitions(TEvent eventKey)
        {
            if (isTransitioning) return;

            var candidate = FindEventTransitionCandidate(eventKey);
            if (candidate != null)
                ExecuteTransition(states[candidate.ToState], candidate);
        }

        private Transition<TKey, TEvent> FindAutoTransitionCandidate(TimeSpan deltaTime)
        {
            foreach (var t in transitions)
            {
                if (!t.Auto) continue;
                if (!InnerStates<TKey>.IsInnerState(t.FromState) && !EqualityComparer<TKey>.Default.Equals(t.FromState, CurrentState.Key)) continue;
                if (t.IsOneShot && t.HasTriggered) continue;
                if (t.ExitTime.HasValue && currentStateElapsed < t.ExitTime.Value) continue;

                if (t.Delay.HasValue)
                {
                    if (!t.ConditionMet)
                    {
                        if (t.CanTransit())
                        {
                            t.ConditionMet = true;
                            t.DelayAccumulator = TimeSpan.Zero;
                        }
                    }

                    if (t.ConditionMet)
                    {
                        t.DelayAccumulator += deltaTime;
                        if (t.DelayAccumulator >= t.Delay.Value)
                            return t;
                    }
                }
                else
                {
                    if (t.CanTransit())
                        return t;
                }
            }

            return null;
        }

        private Transition<TKey, TEvent> FindEventTransitionCandidate(TEvent eventKey)
        {
            foreach (var t in transitions)
            {
                if (!EqualityComparer<TEvent>.Default.Equals(t.EventKey, eventKey)) continue;
                if (!InnerStates<TKey>.IsInnerState(t.FromState) && !EqualityComparer<TKey>.Default.Equals(t.FromState, CurrentState.Key)) continue;
                if (t.IsOneShot && t.HasTriggered) continue;
                if (t.ExitTime.HasValue && currentStateElapsed < t.ExitTime.Value) continue;
                if (t.CanTransit()) return t;
            }

            return null;
        }

        private void ExecuteTransition(StateBase<TKey> nextState, Transition<TKey, TEvent> triggeredTransition)
        {
            isTransitioning = true;

            var previousState = CurrentState;
            CurrentState?.Exit();

            if (triggeredTransition != null)
            {
                if (triggeredTransition.IsOneShot)
                    triggeredTransition.HasTriggered = true;
                triggeredTransition.ConditionMet = false;
                triggeredTransition.DelayAccumulator = TimeSpan.Zero;
            }

            ResetTransitionRuntimeStatesForState(nextState.Key);
            EnterState(nextState);
            OnStateChanged?.Invoke(previousState, CurrentState);
            isTransitioning = false;
        }

        private void EnterState(StateBase<TKey> state)
        {
            CurrentState = state;
            currentStateElapsed = TimeSpan.Zero;
            CurrentState.Enter();
        }

        private void ResetAllTransitions()
        {
            foreach (var t in transitions)
                t.Reset();
        }

        private void ResetAllStates() 
        {
            foreach (var s in states.Values)
                s.Reset();
        }

        private void ResetTransitionRuntimeStatesForState(TKey stateKey)
        {
            foreach (var t in transitions)
            {
                if (EqualityComparer<TKey>.Default.Equals(stateKey, t.FromState) || InnerStates<TKey>.IsInnerState(t.FromState))
                {
                    if (t.IsOneShot && t.HasTriggered) continue;
                    t.Reset();
                }
            }
        }

        /// <summary>
        /// 状态机构建器
        /// </summary>
        /// <remarks>
        /// <para>用于构建 <see cref="StateMachine{TKey, TEvent}"/> 实例，通过链式调用注册状态、转换和初始状态。</para>
        /// <para>必须调用 <see cref="Create"/> 创建构建器实例，最后调用 <see cref="Build"/> 生成状态机。</para>
        /// </remarks>
        public class Builder
        {
            private TKey initialStateName;
            private readonly Dictionary<TKey, StateBase<TKey>> states
                = new Dictionary<TKey, StateBase<TKey>>();
            private readonly List<StateBase<TKey>> orderedStates
                = new List<StateBase<TKey>>();
            private readonly List<Transition<TKey, TEvent>> transitions
                = new List<Transition<TKey, TEvent>>();

            private Builder() { }

            /// <summary>
            /// 创建构建器实例
            /// </summary>
            /// <remarks>
            /// <para>构建器的唯一入口。</para>
            /// </remarks>
            /// <returns>构建器实例</returns>
            public static Builder Create()
            {
                return new Builder();
            }

            /// <summary>
            /// 注册状态
            /// </summary>
            /// <remarks>
            /// <para>同名状态只能注册一次，重复注册将抛出 <see cref="StateMachineException"/>。</para>
            /// <para>状态名称不可与框架保留名称冲突，否则将抛出 <see cref="StateMachineException"/>。</para>
            /// </remarks>
            /// <param name="state">状态实例</param>
            /// <returns>当前构建器实例</returns>
            public Builder AddState(StateBase<TKey> state)
            {
                if (InnerStates<TKey>.IsInnerState(state.Key))
                    throw new StateMachineException($"State name '{state.Key}' is reserved by the framework.");

                if (states.ContainsKey(state.Key))
                    throw new StateMachineException($"State '{state.Key}' is already registered.");

                states[state.Key] = state;
                orderedStates.Add(state);
                return this;
            }

            /// <summary>
            /// 注册转换
            /// </summary>
            /// <remarks>
            /// <para>同一转换实例只能注册一次，重复注册将抛出 <see cref="StateMachineException"/>。</para>
            /// </remarks>
            /// <param name="transition">转换实例</param>
            /// <returns>当前构建器实例</returns>
            public Builder AddTransition(Transition<TKey, TEvent> transition)
            {
                if (transitions.Contains(transition))
                    throw new StateMachineException("Transition instance is already registered.");

                transitions.Add(transition);
                return this;
            }

            /// <summary>
            /// 设置初始状态
            /// </summary>
            /// <remarks>
            /// <para>指定状态机启动时进入的状态名称，必须已通过 <see cref="AddState"/> 注册。</para>
            /// </remarks>
            /// <param name="stateName">初始状态名称</param>
            /// <returns>当前构建器实例</returns>
            public Builder SetInitialState(TKey stateName)
            {
                initialStateName = stateName;
                return this;
            }

            /// <summary>
            /// 构建状态机实例
            /// </summary>
            /// <remarks>
            /// <para>根据当前配置生成 <see cref="IContextStateMachine{TKey, TEvent, TContext}"/> 实例。</para>
            /// <para>以下情况将抛出 <see cref="StateMachineException"/>：上下文为 <c>null</c>、未设置初始状态、初始状态未注册、转换的源状态或目标状态未注册。</para>
            /// </remarks>
            /// <returns>状态机实例</returns>
            public StateMachine<TKey, TEvent> Build()
            {
                if (initialStateName == null)
                    throw new StateMachineException("Initial state must be set before building.");

                if (!states.ContainsKey(initialStateName))
                    throw new StateMachineException($"Initial state '{initialStateName}' is not registered.");

                foreach (var t in transitions)
                {
                    if (!states.ContainsKey(t.ToState))
                        throw new StateMachineException(
                            $"Transition target state '{t.ToState}' is not registered.");

                    if (!InnerStates<TKey>.IsInnerState(t.FromState) && !states.ContainsKey(t.FromState))
                        throw new StateMachineException(
                            $"Transition source state '{t.FromState}' is not registered.");
                }

                return new StateMachine<TKey, TEvent>(states, orderedStates, transitions, initialStateName);
            }
        }
    }
}