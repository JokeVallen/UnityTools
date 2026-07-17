using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 后置处理异常
    /// </summary>
    /// <remarks>
    /// <para>处理器 <c>After</c> 阶段抛出的异常。</para>
    /// </remarks>
    public sealed class ProcessorAfterException : ProcessorException
    {
        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="processorType">处理器类型</param>
        /// <param name="context">上下文</param>
        /// <param name="innerException">内部异常</param>
        public ProcessorAfterException(Type processorType, IContext context, Exception innerException)
        : base($"An exception occurred in After of processor '{processorType.FullName}'.", processorType, context, innerException)
        { }
    }
}
