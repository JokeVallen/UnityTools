using System;

namespace Orchestrator
{
    /// <summary>步骤执行结果</summary>
    public readonly struct StepResult
    {
        /// <summary>步骤流转状态</summary>
        public StepFlow Flow { get; }

        /// <summary>步骤执行过程中捕获的异常</summary>
        public Exception Exception { get; }

        private StepResult(StepFlow flow, Exception exception)
        {
            Flow = flow;
            Exception = exception;
        }

        /// <summary>创建成功结果</summary>
        /// <returns>继续执行的结果</returns>
        public static StepResult Continue() => new StepResult(StepFlow.Continue, null);

        /// <summary>创建中断结果</summary>
        /// <returns>中断执行的结果</returns>
        public static StepResult Break() => new StepResult(StepFlow.Break, null);

        /// <summary>创建失败结果</summary>
        /// <param name="ex">异常实例</param>
        /// <returns>失败结果</returns>
        public static StepResult Fail(Exception ex) => new StepResult(StepFlow.Fail, ex);
    }
}