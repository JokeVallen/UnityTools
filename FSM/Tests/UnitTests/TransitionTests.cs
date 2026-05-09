using FSM.Runtime;

namespace FSM.Tests
{
    public class TransitionTests
    {
        [Fact]
        public void CanTransit_NullCondition_ReturnsTrue()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .Build();

            Assert.True(t.CanTransit(new TestContext()));
        }

        [Fact]
        public void CanTransit_ConditionTrue_ReturnsTrue()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .When(ctx => ctx.Flag)
                .Build();

            Assert.True(t.CanTransit(new TestContext { Flag = true }));
        }

        [Fact]
        public void CanTransit_ConditionFalse_ReturnsFalse()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .When(ctx => ctx.Flag)
                .Build();

            Assert.False(t.CanTransit(new TestContext { Flag = false }));
        }

        [Fact]
        public void ResetRuntimeState_ResetsAllRuntimeFields()
        {
            var t = Transition<TestContext>.Builder
                .Create("A", "B")
                .Build();

            t.DelayAccumulator = TimeSpan.FromSeconds(1);
            t.ConditionMet = true;
            t.HasTriggered = true;

            t.ResetRuntimeState();

            Assert.Equal(TimeSpan.Zero, t.DelayAccumulator);
            Assert.False(t.ConditionMet);
            Assert.False(t.HasTriggered);
        }
    }
}
