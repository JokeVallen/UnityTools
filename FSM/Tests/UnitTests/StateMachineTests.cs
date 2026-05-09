using FSM.Runtime;

namespace FSM.Tests
{
    public class StateMachineTests
    {
        // ── 构建辅助 ─────────────────────────────────

        private IStateMachine<string,TestContext> BuildSimpleMachine(
            TestContext ctx = null,
            string initial = "Idle")
        {
            return StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx ?? new TestContext())
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddState(new TrackingState("Dead"))
                .SetInitialState(initial)
                .Build();
        }

        // ── 生命周期 ─────────────────────────────────

        [Fact]
        public void Start_SetsCurrentStateToInitial()
        {
            var machine = BuildSimpleMachine();
            machine.Start();

            Assert.Equal("Idle", machine.CurrentState.Key);
        }

        [Fact]
        public void Start_CallsOnEnterOnInitialState()
        {
            var state = new TrackingState("Idle");
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(state)
                .SetInitialState("Idle")
                .Build();

            machine.Start();

            Assert.Equal(1, state.EnterCount);
        }

        [Fact]
        public void Start_WhenAlreadyRunning_ThrowsStateMachineException()
        {
            var machine = BuildSimpleMachine();
            machine.Start();

            Assert.Throws<StateMachineException>(() => machine.Start());
        }

        [Fact]
        public void Stop_CallsOnExitOnCurrentState()
        {
            var state = new TrackingState("Idle");
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(state)
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Stop();

            Assert.Equal(1, state.ExitCount);
        }

        [Fact]
        public void Reset_AfterStop_CurrentStateIsNull()
        {
            var machine = BuildSimpleMachine();
            machine.Start();
            machine.Reset();

            Assert.Null(machine.CurrentState);
        }

        [Fact]
        public void Reset_ThenStart_RestartsFromInitialState()
        {
            var machine = BuildSimpleMachine();
            machine.Start();
            machine.Reset();
            machine.Start();

            Assert.Equal("Idle", machine.CurrentState.Key);
        }

        // ── Update ───────────────────────────────────

        [Fact]
        public void Update_CallsOnUpdateOnCurrentState()
        {
            var state = new TrackingState("Idle");
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(state)
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(16));

            Assert.Equal(1, state.UpdateCount);
        }

        [Fact]
        public void Update_WhenNotRunning_DoesNothing()
        {
            var state = new TrackingState("Idle");
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(state)
                .SetInitialState("Idle")
                .Build();

            machine.Update(TimeSpan.FromMilliseconds(16));

            Assert.Equal(0, state.UpdateCount);
        }

        // ── 自动转换 ─────────────────────────────────

        [Fact]
        public void AutoTransition_ConditionMet_ChangesState()
        {
            var ctx = new TestContext { Flag = true };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(16));

            Assert.Equal("Run", machine.CurrentState.Key);
        }

        [Fact]
        public void AutoTransition_ConditionNotMet_DoesNotChangeState()
        {
            var ctx = new TestContext { Flag = false };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(16));

            Assert.Equal("Idle", machine.CurrentState.Key);
        }

        [Fact]
        public void AutoTransition_CallsOnExitThenOnEnter()
        {
            var idle = new TrackingState("Idle");
            var run = new TrackingState("Run");
            var ctx = new TestContext { Flag = true };

            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(idle)
                .AddState(run)
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(16));

            Assert.Equal(1, idle.ExitCount);
            Assert.Equal(1, run.EnterCount);
        }

        // ── 事件驱动转换 ─────────────────────────────

        [Fact]
        public void SendEvent_MatchingTransition_ChangesState()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Dead"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Dead")
                    .OnEvent("Die")
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.SendEvent("Die");

            Assert.Equal("Dead", machine.CurrentState.Key);
        }

        [Fact]
        public void SendEvent_NonMatchingEvent_DoesNotChangeState()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Dead"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Dead")
                    .OnEvent("Die")
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.SendEvent("Jump");

            Assert.Equal("Idle", machine.CurrentState.Key);
        }

        // ── Any 状态 ─────────────────────────────────

        [Fact]
        public void AnyTransition_FromAnyState_ChangesState()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddState(new TrackingState("Dead"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create(InnerStates.AnyState, "Dead")
                    .OnEvent("Die")
                    .Build())
                .SetInitialState("Run")
                .Build();

            machine.Start();
            machine.SendEvent("Die");

            Assert.Equal("Dead", machine.CurrentState.Key);
        }

        // ── 优先级 ───────────────────────────────────

        [Fact]
        public void Priority_LowerValueTransition_ExecutedFirst()
        {
            var ctx = new TestContext { Flag = true };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddState(new TrackingState("Dead"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Dead")
                    .When(c => c.Flag)
                    .WithPriority(10)
                    .Build())
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .WithPriority(1)
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(16));

            Assert.Equal("Run", machine.CurrentState.Key);
        }

        // ── ExitTime ─────────────────────────────────

        [Fact]
        public void ExitTime_NotElapsed_DoesNotTransit()
        {
            var ctx = new TestContext { Flag = true };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .WithExitTime(TimeSpan.FromSeconds(1))
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(500));

            Assert.Equal("Idle", machine.CurrentState.Key);
        }

        [Fact]
        public void ExitTime_Elapsed_Transits()
        {
            var ctx = new TestContext { Flag = true };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .WithExitTime(TimeSpan.FromSeconds(1))
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromSeconds(1.1));

            Assert.Equal("Run", machine.CurrentState.Key);
        }

        // ── Delay ────────────────────────────────────

        [Fact]
        public void Delay_ConditionMetButDelayNotElapsed_DoesNotTransit()
        {
            var ctx = new TestContext { Flag = true };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .WithDelay(TimeSpan.FromSeconds(1))
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(500));

            Assert.Equal("Idle", machine.CurrentState.Key);
        }

        [Fact]
        public void Delay_Elapsed_Transits()
        {
            var ctx = new TestContext { Flag = true };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .WithDelay(TimeSpan.FromSeconds(1))
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Update(TimeSpan.FromSeconds(0.5));
            machine.Update(TimeSpan.FromSeconds(0.6));

            Assert.Equal("Run", machine.CurrentState.Key);
        }

        // ── OneShot ──────────────────────────────────

        [Fact]
        public void OneShot_TransitionFiresOnlyOnce()
        {
            var ctx = new TestContext { Flag = true };
            var run = new TrackingState("Run");

            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(run)
                .AddTransition(Transition<TestContext>.Builder
                    .Create(InnerStates.AnyState, "Run")
                    .OnEvent("Go")
                    .OneShot()
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.SendEvent("Go");
            machine.SendEvent("Go");

            Assert.Equal(1, run.EnterCount);
        }

        // ── ForceTransition ──────────────────────────

        [Fact]
        public void ForceTransition_ChangesStateImmediately()
        {
            var machine = BuildSimpleMachine();
            machine.Start();
            machine.ForceTransition("Run");

            Assert.Equal("Run", machine.CurrentState.Key);
        }

        [Fact]
        public void ForceTransition_UnknownState_ThrowsStateMachineException()
        {
            var machine = BuildSimpleMachine();
            machine.Start();

            Assert.Throws<StateMachineException>(() =>
                machine.ForceTransition("NotExist"));
        }

        [Fact]
        public void SendEvent_NullEventName_ThrowsStateMachineException()
        {
            var machine = BuildSimpleMachine();
            machine.Start();

            Assert.Throws<StateMachineException>(() => machine.SendEvent(null));
        }

        [Fact]
        public void SendEvent_EmptyEventName_ThrowsStateMachineException()
        {
            var machine = BuildSimpleMachine();
            machine.Start();

            Assert.Throws<StateMachineException>(() => machine.SendEvent(""));
        }

        [Fact]
        public void OnStateChanged_FiredAfterTransition()
        {
            IState<string, TestContext> capturedFrom = null;
            IState<string, TestContext> capturedTo = null;

            var ctx = new TestContext { Flag = true };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.OnStateChanged += (from, to) =>
            {
                capturedFrom = from;
                capturedTo = to;
            };

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(16));

            Assert.Equal("Idle", capturedFrom.Key);
            Assert.Equal("Run", capturedTo.Key);
        }

        [Fact]
        public void OnStateChanged_NotFiredWhenNoTransition()
        {
            var fired = false;
            var ctx = new TestContext { Flag = false };
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(ctx)
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .When(c => c.Flag)
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.OnStateChanged += (from, to) => fired = true;

            machine.Start();
            machine.Update(TimeSpan.FromMilliseconds(16));

            Assert.False(fired);
        }

        [Fact]
        public void OnStarted_FiredAfterStart()
        {
            var fired = false;
            var machine = BuildSimpleMachine();
            machine.OnStarted += () => fired = true;
            machine.Start();

            Assert.True(fired);
        }

        [Fact]
        public void OnStopped_FiredAfterStop()
        {
            var fired = false;
            var machine = BuildSimpleMachine();
            machine.OnStopped += () => fired = true;
            machine.Start();
            machine.Stop();

            Assert.True(fired);
        }

        [Fact]
        public void OnStateChanged_FiredAfterForceTransition()
        {
            IState<string, TestContext> capturedFrom = null;
            IState<string, TestContext> capturedTo = null;

            var machine = BuildSimpleMachine();
            machine.OnStateChanged += (from, to) =>
            {
                capturedFrom = from;
                capturedTo = to;
            };

            machine.Start();
            machine.ForceTransition("Run");

            Assert.Equal("Idle", capturedFrom.Key);
            Assert.Equal("Run", capturedTo.Key);
        }

        [Fact]
        public void SendEvent_DuringTransition_IsIgnored()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new TrackingState("Idle"))
                .AddState(new TrackingState("Run"))
                .AddState(new TrackingState("Dead"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .OnEvent("Go")
                    .Build())
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Run", "Dead")
                    .OnEvent("Die")
                    .Build())
                .SetInitialState("Idle")
                .Build();

            machine.OnStateChanged += (from, to) =>
            {
                if (to.Key == "Run")
                    machine.SendEvent("Die");
            };

            machine.Start();
            machine.SendEvent("Go");

            Assert.Equal("Run", machine.CurrentState.Key);
        }

        // ── 测试用 Stub ──────────────────────────────

        private class TrackingState : FSM.Runtime.StateBase<TestContext>
        {
            public override string Key { get; }
            public int EnterCount { get; private set; }
            public int UpdateCount { get; private set; }
            public int ExitCount { get; private set; }

            public TrackingState(string name) => Key = name;

            public override void Enter(TestContext context) => EnterCount++;
            public override void Update(TestContext context, TimeSpan deltaTime) => UpdateCount++;
            public override void Exit(TestContext context) => ExitCount++;
        }
    }
}
