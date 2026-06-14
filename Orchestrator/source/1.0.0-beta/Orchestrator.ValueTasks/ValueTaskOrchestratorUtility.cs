using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.ValueTasks
{
    public static class ValueTaskOrchestratorUtility
    {
        public static Func<TIn, CancellationToken, ValueTask<StepResult<TOut>>> CompilePipeline<TIn, TOut>(IValueTaskStep<TIn, TOut> step, List<IValueTaskBehavior<TIn, TOut>> behaviors)
        {
            Func<TIn, CancellationToken, ValueTask<StepResult<TOut>>> inner = (input, ct) => step.ExecuteAsync(input, ct);

            if (behaviors == null || behaviors.Count == 0)
                return inner;

            for (int i = behaviors.Count - 1; i >= 0; i--)
            {
                var behavior = behaviors[i];
                var currentInner = inner;
                inner = (input, ct) => behavior.HandleAsync(input, () => currentInner(input, ct), ct);
            }

            return inner;
        }

        public static async ValueTask<T[]> WhenAll<T>(ValueTask<T>[] tasks)
        {
            int length = tasks.Length;
            bool allSync = true;
            for (int i = 0; i < length; i++)
            {
                if (!tasks[i].IsCompleted)
                {
                    allSync = false;
                    break;
                }
            }

            if (allSync)
            {
                var results = new T[length];
                for (int i = 0; i < length; i++)
                    results[i] = tasks[i].Result;
                return results;
            }

            var taskArray = new Task<T>[length];
            for (int i = 0; i < length; i++)
                taskArray[i] = tasks[i].AsTask();

            return await Task.WhenAll(taskArray).ConfigureAwait(false);
        }
    }
}
