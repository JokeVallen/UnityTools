using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Orchestrator;
using Orchestrator.UniTasks;

// 步骤工厂
public class SyncStep : IUniTaskStep<string>
{
    private readonly string key;
    private readonly string outputKey;
    private readonly string outputValue;
    private readonly IReadOnlyCollection<IStep<string>> deps;

    public SyncStep(string key, string outputKey = "result", string outputValue = null, IUniTaskStep<string>[] deps = null)
    {
        this.key = key;
        this.outputKey = outputKey;
        this.outputValue = outputValue ?? "ok";
        this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
    }

    public string Key => key;
    public IReadOnlyCollection<IStep<string>> Dependencies => deps;

    public UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        context.Set(outputKey, outputValue);
        return UniTask.FromResult(StepResult.Continue());
    }
}

public class SyncStep<T> : IUniTaskStep<string>
{
    public string Key => key;
    public IReadOnlyCollection<IStep<string>> Dependencies => deps;
    private readonly string key;
    private readonly string outputKey;
    private readonly T outputValue;
    private readonly IReadOnlyCollection<IStep<string>> deps;

    public SyncStep(string key, string outputKey, T outputValue, IUniTaskStep<string>[] deps = null)
    {
        this.key = key;
        this.outputKey = outputKey;
        this.outputValue = outputValue;
        this.deps = deps;
    }

    public UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        context.Set(outputKey, outputValue);
        return UniTask.FromResult(StepResult.Continue());
    }
}

public class FailStep : IUniTaskStep<string>
{
    private readonly string key;
    private readonly Exception exception;
    private readonly IReadOnlyCollection<IStep<string>> deps;

    public FailStep(string key, Exception exception, IUniTaskStep<string>[] deps = null)
    {
        this.key = key;
        this.exception = exception;
        this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
    }

    public string Key => key;
    public IReadOnlyCollection<IStep<string>> Dependencies => deps;

    public UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        return UniTask.FromResult(StepResult.Fail(exception));
    }
}

public class BreakStep : IUniTaskStep<string>
{
    private readonly string key;
    private readonly IReadOnlyCollection<IStep<string>> deps;

    public BreakStep(string key, IUniTaskStep<string>[] deps = null)
    {
        this.key = key;
        this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
    }

    public string Key => key;
    public IReadOnlyCollection<IStep<string>> Dependencies => deps;

    public UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        return UniTask.FromResult(StepResult.Break());
    }
}

public class SlowStep : IUniTaskStep<string>
{
    private readonly string key;
    private readonly int delayMs;
    private readonly string outputKey;
    private readonly object outputValue;
    private readonly IReadOnlyCollection<IStep<string>> deps;

    public SlowStep(string key, int delayMs, string outputKey = "result", object outputValue = null, IUniTaskStep<string>[] deps = null)
    {
        this.key = key;
        this.delayMs = delayMs;
        this.outputKey = outputKey;
        this.outputValue = outputValue ?? "slow";
        this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
    }

    public string Key => key;
    public IReadOnlyCollection<IStep<string>> Dependencies => deps;

    public async UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        await UniTask.Delay(delayMs, cancellationToken: token);
        context.Set(outputKey, outputValue);
        return StepResult.Continue();
    }
}

public class RecordStep : IUniTaskStep<string>
{
    private readonly string key;
    private readonly List<string> order;
    private readonly IReadOnlyCollection<IStep<string>> deps;

    public RecordStep(string key, List<string> order, IUniTaskStep<string>[] deps = null)
    {
        this.key = key;
        this.order = order;
        this.deps = deps?.Select(d => (IStep<string>)d).ToArray() ?? Array.Empty<IStep<string>>();
    }

    public string Key => key;
    public IReadOnlyCollection<IStep<string>> Dependencies => deps;

    public UniTask<StepResult> ExecuteAsync(ITypedPipelineContext context, CancellationToken token)
    {
        order.Add(key);
        context.Set(key, key);
        return UniTask.FromResult(StepResult.Continue());
    }
}

// 行为工厂
public class LoggingBehavior : IUniTaskBehavior<string>
{
    private readonly Action<string> logAction;

    public LoggingBehavior(Action<string> logAction = null)
    {
        this.logAction = logAction;
    }

    public async UniTask<StepResult> HandleAsync(ITypedPipelineContext context, UniTaskBehaviorStepper<string> stepper, CancellationToken token)
    {
        logAction?.Invoke("Before");
        var result = await stepper.NextAsync(token);
        logAction?.Invoke("After");
        return result;
    }
}

public class TimingBehavior : IUniTaskBehavior<string>
{
    private readonly Action<TimeSpan> record;

    public TimingBehavior(Action<TimeSpan> record = null)
    {
        this.record = record;
    }

    public async UniTask<StepResult> HandleAsync(ITypedPipelineContext context, UniTaskBehaviorStepper<string> stepper, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        var result = await stepper.NextAsync(token);
        sw.Stop();
        record?.Invoke(sw.Elapsed);
        return result;
    }
}

public class RetryBehavior : IUniTaskBehavior<string>
{
    private readonly int maxRetries;

    public RetryBehavior(int maxRetries)
    {
        this.maxRetries = maxRetries;
    }

    public async UniTask<StepResult> HandleAsync(ITypedPipelineContext context, UniTaskBehaviorStepper<string> stepper, CancellationToken token)
    {
        int attempts = 0;
        while (true)
        {
            attempts++;
            var result = await stepper.NextAsync(token);
            if (result.Flow != StepFlow.Fail || attempts >= maxRetries)
                return result;
        }
    }
}

public sealed class EmptyContext : ITypedPipelineContext
{
    public string Tag { get; private set; }
    public void Set<TKey, TValue>(TKey key, TValue value) { }
    public Optional<TValue> Get<TKey, TValue>(TKey key) => Optional<TValue>.None;
    public void AddStepExecutionResult<TStepKey>(StepExecutionResult<TStepKey> stepExecutionResult) { }
    public Optional<StepExecutionResult<TStepKey>> GetStepExecutionResult<TStepKey>(TStepKey key) => Optional<StepExecutionResult<TStepKey>>.None;
    public bool Remove<TKey, TValue>(TKey key) => false;
    public bool ContainsKey<TKey, TValue>(TKey key) => false;
    public void Clear() { }
    public IEnumerable<StepExecutionResult<TStepKey>> GetAllStepExecutionResults<TStepKey>() => Array.Empty<StepExecutionResult<TStepKey>>();

    public void SetTag(string tag)
    {
        Tag = tag;
    }
}