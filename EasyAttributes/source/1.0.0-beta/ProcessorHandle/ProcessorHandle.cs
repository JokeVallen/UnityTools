namespace EasyAttributes.Core
{
    /// <summary>
    /// 处理器句柄
    /// </summary>
    /// <remarks>
    /// <para>
    /// 通过静态工厂方法创建，提供 <see cref="Continue"/>、<see cref="Aborted"/>、<see cref="AbortedAll"/> 三个单例，
    /// 以及 <see cref="Abort(object)"/> 和 <see cref="AbortAll(object)"/> 携带返回值。
    /// </para>
    /// </remarks>
    public sealed class ProcessorHandle : IProcessorHandle
    {
        /// <inheritdoc/>
        public bool IsAborted { get; }
        /// <inheritdoc/>
        public bool SkipAfterCallbacks { get; }
        /// <inheritdoc/>
        public object Result { get; }

        private ProcessorHandle(bool isAborted, bool skipAfterCallbacks, object result)
        {
            IsAborted = isAborted;
            SkipAfterCallbacks = skipAfterCallbacks;
            Result = result;
        }

        /// <summary>
        /// 继续执行
        /// </summary>
        public static readonly IProcessorHandle Continue
        = new ProcessorHandle(isAborted: false, skipAfterCallbacks: false, result: null);

        /// <summary>
        /// 中止并运行After
        /// </summary>
        public static readonly IProcessorHandle Aborted
        = new ProcessorHandle(isAborted: true, skipAfterCallbacks: false, result: null);

        /// <summary>
        /// 完全中止
        /// </summary>
        public static readonly IProcessorHandle AbortedAll
        = new ProcessorHandle(isAborted: true, skipAfterCallbacks: true, result: null);

        /// <summary>
        /// 中止并返回结果
        /// </summary>
        /// <param name="result">替换结果</param>
        public static IProcessorHandle Abort(object result)
        => new ProcessorHandle(isAborted: true, skipAfterCallbacks: false, result: result);

        /// <summary>
        /// 完全中止并返回结果
        /// </summary>
        /// <param name="result">替换结果</param>
        public static IProcessorHandle AbortAll(object result)
        => new ProcessorHandle(isAborted: true, skipAfterCallbacks: true, result: result);
    }
}
