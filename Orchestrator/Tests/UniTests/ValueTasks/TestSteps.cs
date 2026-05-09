using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    public class TestStep<TIn, TOut> : IValueTaskStep<TIn, TOut>
    {
        private readonly Func<TIn, CancellationToken, ValueTask<StepResult<TOut>>> _executeFunc;

        public string Name { get; }
        public IReadOnlyCollection<IStep> Dependencies { get; }

        public TestStep(
            string name,
            Func<TIn, CancellationToken, ValueTask<StepResult<TOut>>> executeFunc,
            IReadOnlyList<IValueTaskStep<TIn, TOut>> dependencies = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _executeFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
            Dependencies = dependencies?.Select(d => (IStep)d).ToList();
            if (Dependencies == null) Dependencies = Array.Empty<IStep>();
        }

        public ValueTask<StepResult<TOut>> ExecuteAsync(TIn input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                return _executeFunc(input, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ValueTask<StepResult<TOut>>(StepResult<TOut>.Fail(ex));
            }
        }
    }

    public static class TestSteps
    {
        public static IValueTaskStep<string, string> CreateSuccessStep(string name, string output = "success")
            => new TestStep<string, string>(
                name,
                (input, token) => new ValueTask<StepResult<string>>(StepResult<string>.Continue(output)));

        public static IValueTaskStep<string, string> CreateFailingStep(string name, Exception exception)
            => new TestStep<string, string>(
                name,
                (input, token) => new ValueTask<StepResult<string>>(StepResult<string>.Fail(exception)));

        public static IValueTaskStep<string, string> CreateBrokenStep(string name, string output = "break")
            => new TestStep<string, string>(
                name,
                (input, token) => new ValueTask<StepResult<string>>(StepResult<string>.Break(output)));

        public static IValueTaskStep<string, string> CreateSlowStep(string name, int delayMs, string output = "slow")
            => new TestStep<string, string>(
                name,
                async (input, token) =>
                {
                    await Task.Delay(delayMs, token);
                    return StepResult<string>.Continue(output);
                });

        public static IValueTaskStep<string, string> CreateStepWithDependencies(
            string name,
            Func<ValueTask<StepResult<string>>> executeFunc,
            params IValueTaskStep<string, string>[] dependencies)
            => new TestStep<string, string>(
                name,
                (input, token) => executeFunc(),
                dependencies);
    }
}