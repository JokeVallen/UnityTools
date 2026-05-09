using FSM.Runtime;

namespace FSM.Tests
{
    public class StateMachineBuilderTests
    {
        private IState<string,TestContext> MakeState(string name) =>
            new StubState(name);

        [Fact]
        public void AddTransition_DuplicateInstance_ThrowsStateMachineException()
        {
            var transition = Transition<TestContext>.Builder
                .Create("Idle", "Run")
                .Build();

            Assert.Throws<StateMachineException>(() =>
                StateMachine<TestContext>.Builder
                    .Create()
                    .WithContext(new TestContext())
                    .AddState(new StubState("Idle"))
                    .AddState(new StubState("Run"))
                    .AddTransition(transition)
                    .AddTransition(transition));
        }

        [Fact]
        public void States_PreservesRegistrationOrder()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new StubState("Idle"))
                .AddState(new StubState("Run"))
                .AddState(new StubState("Dead"))
                .SetInitialState("Idle")
                .Build();

            Assert.Equal("Idle", machine.States[0].Key);
            Assert.Equal("Run", machine.States[1].Key);
            Assert.Equal("Dead", machine.States[2].Key);
        }

        [Fact]
        public void Build_ValidConfiguration_ReturnsMachine()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(MakeState("Idle"))
                .SetInitialState("Idle")
                .Build();

            Assert.NotNull(machine);
        }

        [Fact]
        public void Build_NoInitialState_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                StateMachine<TestContext>.Builder
                    .Create()
                    .WithContext(new TestContext())
                    .AddState(MakeState("Idle"))
                    .Build());
        }

        [Fact]
        public void Build_InitialStateNotRegistered_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                StateMachine<TestContext>.Builder
                    .Create()
                    .WithContext(new TestContext())
                    .AddState(MakeState("Idle"))
                    .SetInitialState("NotExist")
                    .Build());
        }

        [Fact]
        public void AddState_DuplicateName_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                StateMachine<TestContext>.Builder
                    .Create()
                    .WithContext(new TestContext())
                    .AddState(MakeState("Idle"))
                    .AddState(MakeState("Idle")));
        }

        [Fact]
        public void Build_NullContext_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                StateMachine<TestContext>.Builder
                    .Create()
                    .AddState(new StubState("Idle"))
                    .SetInitialState("Idle")
                    .Build());
        }

        [Fact]
        public void Build_TransitionToStateNotRegistered_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                StateMachine<TestContext>.Builder
                    .Create()
                    .WithContext(new TestContext())
                    .AddState(new StubState("Idle"))
                    .AddTransition(Transition<TestContext>.Builder
                        .Create("Idle", "NotExist")
                        .Build())
                    .SetInitialState("Idle")
                    .Build());
        }

        [Fact]
        public void Build_TransitionFromStateNotRegistered_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                StateMachine<TestContext>.Builder
                    .Create()
                    .WithContext(new TestContext())
                    .AddState(new StubState("Idle"))
                    .AddTransition(Transition<TestContext>.Builder
                        .Create("NotExist", "Idle")
                        .Build())
                    .SetInitialState("Idle")
                    .Build());
        }

        [Fact]
        public void IsRunning_BeforeStart_IsFalse()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new StubState("Idle"))
                .SetInitialState("Idle")
                .Build();

            Assert.False(machine.IsRunning);
        }

        [Fact]
        public void IsRunning_AfterStart_IsTrue()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new StubState("Idle"))
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            Assert.True(machine.IsRunning);
        }

        [Fact]
        public void IsRunning_AfterStop_IsFalse()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new StubState("Idle"))
                .SetInitialState("Idle")
                .Build();

            machine.Start();
            machine.Stop();
            Assert.False(machine.IsRunning);
        }

        [Fact]
        public void States_ContainsAllRegisteredStates()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new StubState("Idle"))
                .AddState(new StubState("Run"))
                .SetInitialState("Idle")
                .Build();

            Assert.Equal(2, machine.States.Count);
        }

        [Fact]
        public void Transitions_ContainsAllRegisteredTransitions()
        {
            var machine = StateMachine<TestContext>.Builder
                .Create()
                .WithContext(new TestContext())
                .AddState(new StubState("Idle"))
                .AddState(new StubState("Run"))
                .AddTransition(Transition<TestContext>.Builder
                    .Create("Idle", "Run")
                    .Build())
                .SetInitialState("Idle")
                .Build();

            Assert.Single(machine.Transitions);
        }

        // ── 测试用 Stub ──────────────────────────────
        private class StubState : FSM.Runtime.StateBase<TestContext>
        {
            public override string Key { get; }
            public StubState(string name) => Key = name;
        }
    }
}
