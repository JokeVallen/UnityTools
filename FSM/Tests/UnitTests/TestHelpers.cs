using FSM.Runtime;

namespace FSM.Tests
{
    public class StubState : StateBase<TestContext>
    {
        public override string Key { get; }
        public StubState(string name) => Key = name;
    }

    public class TrackingState : StateBase<TestContext>
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