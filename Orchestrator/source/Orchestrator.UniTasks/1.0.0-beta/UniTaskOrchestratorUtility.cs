using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Orchestrator.UniTasks
{
    internal static class UniTaskOrchestratorUtility
    {
        public static Func<TIn, CancellationToken, UniTask<StepResult<TOut>>> CompilePipeline<TIn, TOut>(IUniTaskStep<TIn, TOut> step, List<IUniTaskBehavior<TIn, TOut>> behaviors)
        {
            Func<TIn, CancellationToken, UniTask<StepResult<TOut>>> inner = (input, ct) => step.ExecuteAsync(input, ct);

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
    }
}
