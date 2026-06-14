// StepFactory.cs && BehaviorFactory.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Orchestrator;
using Orchestrator.UniTasks;

public class SyncStep : IUniTaskStep<string, string>
{
    private readonly string _name, _output;
    private readonly IReadOnlyCollection<IStep> _deps;

    public SyncStep(string name, string output = "ok", IUniTaskStep<string, string>[] deps = null)
    {
        _name = name;
        _output = output;
        _deps = deps?.Select(d => (IStep)d).ToList();
        if (_deps == null) _deps = Array.Empty<IStep>();
    }

    public string Name => _name;
    public IReadOnlyCollection<IStep> Dependencies => _deps;

    public UniTask<StepResult<string>> ExecuteAsync(string input, CancellationToken token)
    {
        return UniTask.FromResult(StepResult<string>.Continue(_output));
    }
}

public class FailStep : IUniTaskStep<string, string>
{
    private readonly string _name;
    private readonly Exception _ex;
    private readonly IReadOnlyCollection<IStep> _deps;

    public FailStep(string name, Exception ex, IUniTaskStep<string, string>[] deps = null)
    {
        _name = name;
        _ex = ex;
        _deps = deps?.Select(d => (IStep)d).ToList();
        if (_deps == null) _deps = Array.Empty<IStep>();
    }

    public string Name => _name;
    public IReadOnlyCollection<IStep> Dependencies => _deps;

    public UniTask<StepResult<string>> ExecuteAsync(string input, CancellationToken token)
    {
        return UniTask.FromResult(StepResult<string>.Fail(_ex));
    }
}

// ---------- 基本步骤实现 ----------
public class TestStep<TIn, TOut> : IUniTaskStep<TIn, TOut>
{
    private readonly Func<TIn, CancellationToken, UniTask<StepResult<TOut>>> _func;
    public string Name { get; }
    public IReadOnlyCollection<IStep> Dependencies { get; }

    public TestStep(string name, Func<TIn, CancellationToken, UniTask<StepResult<TOut>>> func,
        IUniTaskStep<TIn, TOut>[] deps = null)
    {
        Name = name;
        _func = func;
        Dependencies = deps?.Select(d => (IStep)d).ToArray() ?? Array.Empty<IStep>();
    }

    public UniTask<StepResult<TOut>> ExecuteAsync(TIn input, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        try { return _func(input, token); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return UniTask.FromResult(StepResult<TOut>.Fail(ex)); }
    }
}

public static class TestSteps
{
    public static IUniTaskStep<string, string> CreateSuccessStep(string name, string output = "success")
        => new TestStep<string, string>(name, (input, token) => UniTask.FromResult(StepResult<string>.Continue(output)));

    public static IUniTaskStep<string, string> CreateFailingStep(string name, Exception exception)
        => new TestStep<string, string>(name, (input, token) => UniTask.FromResult(StepResult<string>.Fail(exception)));

    public static IUniTaskStep<string, string> CreateBrokenStep(string name, string output = "break")
        => new TestStep<string, string>(name, (input, token) => UniTask.FromResult(StepResult<string>.Break(output)));

    public static IUniTaskStep<string, string> CreateSlowStep(string name, int delayMs, string output = "slow")
        => new TestStep<string, string>(name, async (input, token) =>
        {
            await UniTask.Delay(delayMs, cancellationToken: token);
            return StepResult<string>.Continue(output);
        });

    public static IUniTaskStep<string, string> CreateStepWithDependencies(string name,
        Func<UniTask<StepResult<string>>> executeFunc, params IUniTaskStep<string, string>[] deps)
        => new TestStep<string, string>(name, (input, token) => executeFunc(), deps);
}

public class LoggingBehavior<TIn, TOut> : IUniTaskBehavior<TIn, TOut>
{
    public UniTask<StepResult<TOut>> HandleAsync(
        TIn input, Func<UniTask<StepResult<TOut>>> next, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return next(); // 直接转发，模拟日志
    }
}

public class TimingBehavior<TIn, TOut> : IUniTaskBehavior<TIn, TOut>
{
    public async UniTask<StepResult<TOut>> HandleAsync(
        TIn input, Func<UniTask<StepResult<TOut>>> next, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var sw = Stopwatch.StartNew();
        var result = await next();
        sw.Stop();
        return result;
    }
}

// ---------- 行为实现 ----------
public class TestBehavior<TIn, TOut> : IUniTaskBehavior<TIn, TOut>
{
    private readonly Func<TIn, Func<UniTask<StepResult<TOut>>>, CancellationToken, UniTask<StepResult<TOut>>> _func;
    public string Name { get; }

    public TestBehavior(string name,
        Func<TIn, Func<UniTask<StepResult<TOut>>>, CancellationToken, UniTask<StepResult<TOut>>> func)
    {
        Name = name;
        _func = func;
    }

    public UniTask<StepResult<TOut>> HandleAsync(TIn input, Func<UniTask<StepResult<TOut>>> next, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        try { return _func(input, next, token); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return UniTask.FromResult(StepResult<TOut>.Fail(ex)); }
    }
}

public static class TestBehaviors
{
    public static IUniTaskBehavior<TIn, TOut> CreateLoggingBehavior<TIn, TOut>(Action<string> logAction)
        => new TestBehavior<TIn, TOut>("Logging", async (input, next, token) =>
        {
            logAction("Before");
            var result = await next();
            logAction("After");
            return result;
        });

    public static IUniTaskBehavior<TIn, TOut> CreateTimingBehavior<TIn, TOut>(Action<TimeSpan> record)
        => new TestBehavior<TIn, TOut>("Timing", async (input, next, token) =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = await next();
            sw.Stop();
            record(sw.Elapsed);
            return result;
        });

    public static IUniTaskBehavior<TIn, TOut> CreateRetryBehavior<TIn, TOut>(int maxRetries)
        => new TestBehavior<TIn, TOut>("Retry", async (input, next, token) =>
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    attempts++;
                    return await next();
                }
                catch when (attempts >= maxRetries) { throw; }
            }
        });
}