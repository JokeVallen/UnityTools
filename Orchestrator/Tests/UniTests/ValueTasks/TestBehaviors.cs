namespace Orchestrator.Tests.ValueTasks
{
    public class TestBehavior<TIn, TOut> : IValueTaskBehavior<TIn, TOut>
    {
        private readonly Func<TIn, Func<ValueTask<StepResult<TOut>>>, CancellationToken, ValueTask<StepResult<TOut>>> _handleFunc;

        public string Name { get; }

        public TestBehavior(
            string name,
            Func<TIn, Func<ValueTask<StepResult<TOut>>>, CancellationToken, ValueTask<StepResult<TOut>>> handleFunc)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _handleFunc = handleFunc ?? throw new ArgumentNullException(nameof(handleFunc));
        }

        public ValueTask<StepResult<TOut>> HandleAsync(TIn input, Func<ValueTask<StepResult<TOut>>> next, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                return _handleFunc(input, next, token);
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

    public static class TestBehaviors
    {
        public static IValueTaskBehavior<TIn, TOut> CreateLoggingBehavior<TIn, TOut>(Action<string> logAction)
            => new TestBehavior<TIn, TOut>(
                "LoggingBehavior",
                async (input, next, token) =>
                {
                    logAction("Before execution");
                    var result = await next();
                    logAction("After execution");
                    return result;
                });

        public static IValueTaskBehavior<TIn, TOut> CreateTimingBehavior<TIn, TOut>(Action<TimeSpan> recordAction)
            => new TestBehavior<TIn, TOut>(
                "TimingBehavior",
                async (input, next, token) =>
                {
                    var start = DateTime.UtcNow;
                    var result = await next();
                    recordAction(DateTime.UtcNow - start);
                    return result;
                });

        public static IValueTaskBehavior<TIn, TOut> CreateRetryBehavior<TIn, TOut>(int maxRetries)
            => new TestBehavior<TIn, TOut>(
                "RetryBehavior",
                async (input, next, token) =>
                {
                    int attempts = 0;
                    while (true)
                    {
                        try
                        {
                            attempts++;
                            return await next();
                        }
                        catch
                        {
                            if (attempts >= maxRetries)
                                throw;
                        }
                    }
                });
    }
}