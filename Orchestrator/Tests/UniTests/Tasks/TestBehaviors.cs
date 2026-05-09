using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tests.Tasks
{
    public class TestBehavior<TIn, TOut> : IBehavior<TIn, TOut>
    {
        private readonly Func<TIn, Func<Task<StepResult<TOut>>>, CancellationToken, Task<StepResult<TOut>>> _handleFunc;

        public string Name { get; }

        public TestBehavior(
            string name,
            Func<TIn, Func<Task<StepResult<TOut>>>, CancellationToken, Task<StepResult<TOut>>> handleFunc)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _handleFunc = handleFunc ?? throw new ArgumentNullException(nameof(handleFunc));
        }

        public async Task<StepResult<TOut>> HandleAsync(TIn input, Func<Task<StepResult<TOut>>> next, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                return await _handleFunc(input, next, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return StepResult<TOut>.Fail(ex);
            }
        }
    }

    public static class TestBehaviors
    {
        public static IBehavior<TIn, TOut> CreateLoggingBehavior<TIn, TOut>(Action<string> logAction)
            => new TestBehavior<TIn, TOut>(
                "LoggingBehavior",
                async (input, next, token) =>
                {
                    logAction("Before execution");
                    var result = await next();
                    logAction("After execution");
                    return result;
                });

        public static IBehavior<TIn, TOut> CreateTimingBehavior<TIn, TOut>(Action<TimeSpan> recordAction)
            => new TestBehavior<TIn, TOut>(
                "TimingBehavior",
                async (input, next, token) =>
                {
                    var start = DateTime.UtcNow;
                    var result = await next();
                    var duration = DateTime.UtcNow - start;
                    recordAction(duration);
                    return result;
                });

        public static IBehavior<TIn, TOut> CreateRetryBehavior<TIn, TOut>(int maxRetries)
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