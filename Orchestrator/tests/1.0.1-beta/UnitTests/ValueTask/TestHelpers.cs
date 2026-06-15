// ======================== ValueTaskOrchestratorTests.cs ========================
// ValueTask 版本编排器完整单元测试
// 测试框架: xUnit
// 命名空间: Orchestrator.Tests.ValueTasks

using Orchestrator.ValueTasks;

namespace Orchestrator.Tests.ValueTasks
{
    // ======================== 测试辅助类 ========================

    /// <summary>
    /// 测试辅助类 - 空上下文（排除存储开销干扰）
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

    /// <summary>
    /// 测试辅助类 - 空步骤（支持自定义返回状态）
    /// </summary>
    internal sealed class NullStep : IValueTaskStep<string>
    {
        private readonly string _key;
        private readonly IReadOnlyCollection<IStep<string>> _dependencies;
        private readonly StepFlow _resultFlow;

        public NullStep(string key, StepFlow resultFlow = StepFlow.Continue, IValueTaskStep<string>[] dependencies = null)
        {
            _key = key;
            _resultFlow = resultFlow;
            _dependencies = dependencies?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => _key;
        public IReadOnlyCollection<IStep<string>> Dependencies => _dependencies;

        public ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            return new ValueTask<StepResult>(
                _resultFlow == StepFlow.Continue ? StepResult.Continue() :
                _resultFlow == StepFlow.Break ? StepResult.Break() :
                StepResult.Fail(new InvalidOperationException("Step failed")));
        }
    }

    /// <summary>
    /// 测试辅助类 - 记录执行顺序的步骤
    /// </summary>
    internal sealed class RecordStep : IValueTaskStep<string>
    {
        private readonly string _key;
        private readonly IReadOnlyCollection<IStep<string>> _dependencies;
        private readonly List<string> _executionLog;

        public RecordStep(string key, List<string> executionLog, IValueTaskStep<string>[] dependencies = null)
        {
            _key = key;
            _executionLog = executionLog;
            _dependencies = dependencies?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => _key;
        public IReadOnlyCollection<IStep<string>> Dependencies => _dependencies;

        public async ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            _executionLog.Add(_key);
            await Task.CompletedTask;
            return StepResult.Continue();
        }
    }

    /// <summary>
    /// 测试辅助类 - 慢步骤
    /// </summary>
    internal sealed class SlowStep : IValueTaskStep<string>
    {
        private readonly string _key;
        private readonly int _delayMs;
        private readonly IReadOnlyCollection<IStep<string>> _dependencies;

        public SlowStep(string key, int delayMs, IValueTaskStep<string>[] dependencies = null)
        {
            _key = key;
            _delayMs = delayMs;
            _dependencies = dependencies?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => _key;
        public IReadOnlyCollection<IStep<string>> Dependencies => _dependencies;

        public async ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            await Task.Delay(_delayMs, token);
            return StepResult.Continue();
        }
    }

    /// <summary>
    /// 测试辅助类 - 失败步骤
    /// </summary>
    internal sealed class FailStep : IValueTaskStep<string>
    {
        private readonly string _key;
        private readonly Exception _exception;
        private readonly IReadOnlyCollection<IStep<string>> _dependencies;

        public FailStep(string key, Exception exception, IValueTaskStep<string>[] dependencies = null)
        {
            _key = key;
            _exception = exception;
            _dependencies = dependencies?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => _key;
        public IReadOnlyCollection<IStep<string>> Dependencies => _dependencies;

        public ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            return new ValueTask<StepResult>(StepResult.Fail(_exception));
        }
    }

    /// <summary>
    /// 测试辅助类 - 设置上下文数据的步骤
    /// </summary>
    internal sealed class SetDataStep : IValueTaskStep<string>
    {
        private readonly string _key;
        private readonly string _dataKey;
        private readonly int _dataValue;
        private readonly IReadOnlyCollection<IStep<string>> _dependencies;

        public SetDataStep(string key, string dataKey, int dataValue, IValueTaskStep<string>[] dependencies = null)
        {
            _key = key;
            _dataKey = dataKey;
            _dataValue = dataValue;
            _dependencies = dependencies?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => _key;
        public IReadOnlyCollection<IStep<string>> Dependencies => _dependencies;

        public ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            context.Set(_dataKey, _dataValue);
            return new ValueTask<StepResult>(StepResult.Continue());
        }
    }

    /// <summary>
    /// 测试辅助类 - 获取上下文数据的步骤
    /// </summary>
    internal sealed class GetDataStep : IValueTaskStep<string>
    {
        private readonly string _key;
        private readonly string _dataKey;
        private readonly int _expectedValue;
        private readonly IReadOnlyCollection<IStep<string>> _dependencies;

        public GetDataStep(string key, string dataKey, int expectedValue, IValueTaskStep<string>[] dependencies = null)
        {
            _key = key;
            _dataKey = dataKey;
            _expectedValue = expectedValue;
            _dependencies = dependencies?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
        }

        public string Key => _key;
        public IReadOnlyCollection<IStep<string>> Dependencies => _dependencies;

        public ValueTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
        {
            var value = context.Get<string, int>(_dataKey);
            if (value.HasValue && value.Value == _expectedValue)
                return new ValueTask<StepResult>(StepResult.Continue());
            return new ValueTask<StepResult>(StepResult.Fail(new InvalidOperationException("Data mismatch")));
        }
    }

    /// <summary>
    /// 测试辅助类 - 空行为
    /// </summary>
    internal sealed class NullBehavior : IValueTaskBehavior<string>
    {
        public async ValueTask<StepResult> HandleAsync(
            ITypedPipelineContext context,
            ValueTaskBehaviorStepper<string> stepper,
            CancellationToken token)
        {
            return await stepper.NextAsync(token);
        }
    }

    /// <summary>
    /// 测试辅助类 - 记录执行顺序的行为
    /// </summary>
    internal sealed class RecordBehavior : IValueTaskBehavior<string>
    {
        private readonly string _name;
        private readonly List<string> _executionLog;

        public RecordBehavior(string name, List<string> executionLog)
        {
            _name = name;
            _executionLog = executionLog;
        }

        public async ValueTask<StepResult> HandleAsync(
            ITypedPipelineContext context,
            ValueTaskBehaviorStepper<string> stepper,
            CancellationToken token)
        {
            _executionLog.Add($"{_name}_Before");
            var result = await stepper.NextAsync(token);
            _executionLog.Add($"{_name}_After");
            return result;
        }
    }
}