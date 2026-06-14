using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.ValueTasks
{
    /// <summary>步进器</summary>
    /// <typeparam name="TKey">步骤唯一标识的类型</typeparam>
    public readonly struct ValueTaskBehaviorStepper<TKey>
    {
        private readonly IValueTaskBehavior<TKey>[] behaviors;
        private readonly int index;
        private readonly IValueTaskStep<TKey> step;
        private readonly ITypedPipelineContext context;

        internal ValueTaskBehaviorStepper(IValueTaskBehavior<TKey>[] behaviors, int index, IValueTaskStep<TKey> step, ITypedPipelineContext context)
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
        public async ValueTask<StepResult> NextAsync(CancellationToken token)
        {
            if (index >= behaviors.Length)
                return await step.ExecuteAsync(context, token);

            var behavior = behaviors[index];
            var stepper = new ValueTaskBehaviorStepper<TKey>(behaviors, index + 1, step, context);
            return await behavior.HandleAsync(context, stepper, token);
        }
    }
}
