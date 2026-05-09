using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrator.Tests.Tasks
{
    public class TestStep<TIn, TOut> : IStep<TIn, TOut>
    {
        private readonly Func<TIn, CancellationToken, Task<StepResult<TOut>>> _executeFunc;

        public string Name { get; }
        public IReadOnlyCollection<IStep> Dependencies { get; }   // 返回 IStep 列表

        public TestStep(
            string name,
            Func<TIn, CancellationToken, Task<StepResult<TOut>>> executeFunc,
            IReadOnlyList<IStep<TIn, TOut>> dependencies = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _executeFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
            // 转换为基接口列表，依赖项在引擎中通过 Builder 添加，确保类型安全
            Dependencies = dependencies?.Cast<IStep>().ToList();
            if (Dependencies == null) Dependencies = Array.Empty<IStep>();
        }

        public async Task<StepResult<TOut>> ExecuteAsync(TIn input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                return await _executeFunc(input, token);
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

    public static class TestSteps
    {
        public static IStep<string, string> CreateSuccessStep(string name, string output = "success")
            => new TestStep<string, string>(
                name,
                (input, token) => Task.FromResult(StepResult<string>.Continue(output)));

        public static IStep<string, string> CreateFailingStep(string name, Exception exception)
            => new TestStep<string, string>(
                name,
                (input, token) => Task.FromResult(StepResult<string>.Fail(exception)));

        public static IStep<string, string> CreateBrokenStep(string name, string output = "break")
            => new TestStep<string, string>(
                name,
                (input, token) => Task.FromResult(StepResult<string>.Break(output)));

        public static IStep<string, string> CreateSlowStep(string name, int delayMs, string output = "slow")
            => new TestStep<string, string>(
                name,
                async (input, token) =>
                {
                    await Task.Delay(delayMs, token);
                    return StepResult<string>.Continue(output);
                });

        public static IStep<string, string> CreateStepWithDependencies(
            string name,
            Func<Task<StepResult<string>>> executeFunc,
            params IStep<string, string>[] dependencies)
            => new TestStep<string, string>(
                name,
                (input, token) => executeFunc(),
                dependencies);
    }
}