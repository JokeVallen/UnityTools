using System.Threading;
using Cysharp.Threading.Tasks;

namespace Orchestrator.UniTasks
{
    /// <summary>步进器</summary>
    /// <typeparam name="TKey">步骤唯一标识的类型</typeparam>
    public readonly struct UniTaskBehaviorStepper<TKey>
    {
        private readonly IUniTaskBehavior<TKey>[] behaviors;
        private readonly int index;
        private readonly IUniTaskStep<TKey> step;
        private readonly ITypedPipelineContext context;

        internal UniTaskBehaviorStepper(IUniTaskBehavior<TKey>[] behaviors, int index, IUniTaskStep<TKey> step, ITypedPipelineContext context)
        {
            this.behaviors = behaviors;
            this.index = index;
            this.step = step;
            this.context = context;
        }

        /// <summary>
        /// 步进到下一个行为
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>步骤结果</returns>
        public async UniTask<StepResult> NextAsync(CancellationToken token)
        {
            if (index >= behaviors.Length)
                return await step.ExecuteAsync(context, token);

            var behavior = behaviors[index];
            var stepper = new UniTaskBehaviorStepper<TKey>(behaviors, index + 1, step, context);
            return await behavior.HandleAsync(context, stepper, token);
        }
    }
}
