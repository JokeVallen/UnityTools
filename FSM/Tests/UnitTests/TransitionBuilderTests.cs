using FSM.Runtime;

namespace FSM.Tests
{
    public class TransitionBuilderTests
    {
        [Fact]
        public void Create_NullFromState_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                Transition<TestContext>.Builder.Create(null, "B"));
        }

        [Fact]
        public void Create_NullToState_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                Transition<TestContext>.Builder.Create("A", null));
        }

        [Fact]
        public void Build_WithRequiredFields_CreatesTransition()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .Build();

            Assert.Equal("A", t.FromState);
            Assert.Equal("B", t.ToState);
        }

        [Fact]
        public void Build_DefaultPriority_IsZero()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .Build();

            Assert.Equal(0, t.Priority);
        }

        [Fact]
        public void Build_DefaultEventName_IsNull()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .Build();

            Assert.Null(t.EventName);
        }

        [Fact]
        public void Build_WithPriority_SetsCorrectly()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .WithPriority(5)
                .Build();

            Assert.Equal(5, t.Priority);
        }

        [Fact]
        public void Build_OnEvent_SetsEventName()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .OnEvent("Jump")
                .Build();

            Assert.Equal("Jump", t.EventName);
        }

        [Fact]
        public void Build_Auto_SetsEventNameToNull()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .OnEvent("Jump")
                .Auto()
                .Build();

            Assert.Null(t.EventName);
        }

        [Fact]
        public void Build_WithExitTime_SetsCorrectly()
        {
            var exitTime = TimeSpan.FromSeconds(1);
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .WithExitTime(exitTime)
                .Build();

            Assert.Equal(exitTime, t.ExitTime);
        }

        [Fact]
        public void Build_WithDelay_SetsCorrectly()
        {
            var delay = TimeSpan.FromMilliseconds(200);
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .WithDelay(delay)
                .Build();

            Assert.Equal(delay, t.Delay);
        }

        [Fact]
        public void Build_OneShot_SetsCorrectly()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .OneShot()
                .Build();

            Assert.True(t.IsOneShot);
        }

        [Fact]
        public void Build_EmptyFromState_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                Transition<TestContext>.Builder
                    .Create("", "B")
                    .Build());
        }

        [Fact]
        public void Build_EmptyToState_ThrowsStateMachineException()
        {
            Assert.Throws<StateMachineException>(() =>
                Transition<TestContext>.Builder
                    .Create("A", "")
                    .Build());
        }
    }
}
