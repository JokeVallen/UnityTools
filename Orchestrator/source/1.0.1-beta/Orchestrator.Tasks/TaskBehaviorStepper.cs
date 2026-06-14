using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tasks
{
    /// <summary>步进器</summary>
    /// <typeparam name="TKey">步骤唯一标识的类型</typeparam>
    public readonly struct TaskBehaviorStepper<TKey>
    {
        private readonly ITaskBehavior<TKey>[] behaviors;
        private readonly int index;
        private readonly ITaskStep<TKey> step;
        private readonly ITypedPipelineContext context;

        internal TaskBehaviorStepper(ITaskBehavior<TKey>[] behaviors, int index, ITaskStep<TKey> step, ITypedPipelineContext context)
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
        public async Task<StepResult> NextAsync(CancellationToken token)
        {
            if (index >= behaviors.Length)
                return await step.ExecuteAsync(context, token);

            var behavior = behaviors[index];
            var stepper = new TaskBehaviorStepper<TKey>(behaviors, index + 1, step, context);
            return await behavior.HandleAsync(context, stepper, token);
        }
    }
}
