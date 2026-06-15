using Orchestrator.Tasks;

namespace Orchestrator.Tests.Tasks
{
    internal static class BuilderExtensions
    {
        public static TaskOrchestrator<TKey>.Builder AddSteps<TKey>(
            this TaskOrchestrator<TKey>.Builder builder,
            IEnumerable<ITaskStep<TKey>> steps)
        {
            foreach (var step in steps)
                builder.AddStep(step);
            return builder;
        }
    }

    /// <summary>
    /// 测试辅助类 - 空步骤
    /// </summary>
    internal sealed class NullStep : ITaskStep<string>
    {
        private readonly string _key;
        private readonly IReadOnlyCollection<IStep<string>> _dependencies;
        private readonly StepFlow _resultFlow;

        public NullStep(string key, StepFlow resultFlow = StepFlow.Continue, ITaskStep<string>[] dependencies = null)
        {
            _key = key;
            _resultFlow = resultFlow;
            _dependencies = dependencies?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => _key;
        public IReadOnlyCollection<IStep<string>> Dependencies => _dependencies;

        public Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            return Task.FromResult(_resultFlow == StepFlow.Continue ? StepResult.Continue() :
                                   _resultFlow == StepFlow.Break ? StepResult.Break() :
                                   StepResult.Fail(new InvalidOperationException("Step failed")));
        }
    }

    /// <summary>
    /// 测试辅助类 - 记录执行顺序的步骤
    /// </summary>
    internal sealed class RecordStep : ITaskStep<string>
    {
        private readonly string _key;
        private readonly IReadOnlyCollection<IStep<string>> _dependencies;
        private readonly List<string> _executionLog;

        public RecordStep(string key, List<string> executionLog, ITaskStep<string>[] dependencies = null)
        {
            _key = key;
            _executionLog = executionLog;
            _dependencies = dependencies?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => _key;
        public IReadOnlyCollection<IStep<string>> Dependencies => _dependencies;

        public async Task<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            _executionLog.Add(_key);
            await Task.CompletedTask;
            return StepResult.Continue();
        }
    }

    /// <summary>
    /// 测试辅助类 - 空行为
    /// </summary>
    internal sealed class NullBehavior : ITaskBehavior<string>
    {
        private readonly string _name;

        public NullBehavior(string name = null)
        {
            _name = name ?? "NullBehavior";
        }

        public async Task<StepResult> HandleAsync(
            ITypedPipelineContext context,
            TaskBehaviorStepper<string> stepper,
            CancellationToken token)
        {
            return await stepper.NextAsync(token);
        }
    }

    /// <summary>
    /// 测试辅助类 - 记录执行顺序的行为
    /// </summary>
    internal sealed class RecordBehavior : ITaskBehavior<string>
    {
        private readonly string _name;
        private readonly List<string> _executionLog;

        public RecordBehavior(string name, List<string> executionLog)
        {
            _name = name;
            _executionLog = executionLog;
        }

        public async Task<StepResult> HandleAsync(
            ITypedPipelineContext context,
            TaskBehaviorStepper<string> stepper,
            CancellationToken token)
        {
            _executionLog.Add($"{_name}_Before");
            var result = await stepper.NextAsync(token);
            _executionLog.Add($"{_name}_After");
            return result;
        }
    }

    /// <summary>
    /// 测试辅助类 - 空上下文
    /// </summary>
    internal sealed class TestContext : ITypedPipelineContext
    {
        private readonly TypedPipelineContext _inner = new TypedPipelineContext();

        public void Set<TKey, TValue>(TKey key, TValue value) => _inner.Set(key, value);
        public Optional<TValue> Get<TKey, TValue>(TKey key) => _inner.Get<TKey, TValue>(key);
        public bool Remove<TKey, TValue>(TKey key) => _inner.Remove<TKey, TValue>(key);
        public bool ContainsKey<TKey, TValue>(TKey key) => _inner.ContainsKey<TKey, TValue>(key);
        public void AddStepExecutionResult<TStepKey>(StepExecutionResult<TStepKey> stepExecutionResult)
            => _inner.AddStepExecutionResult(stepExecutionResult);
        public Optional<StepExecutionResult<TStepKey>> GetStepExecutionResult<TStepKey>(TStepKey key)
            => _inner.GetStepExecutionResult<TStepKey>(key);
        public IEnumerable<StepExecutionResult<TStepKey>> GetAllStepExecutionResults<TStepKey>()
            => _inner.GetAllStepExecutionResults<TStepKey>();
        public void Clear() => _inner.Clear();
    }
}
