using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 处理器异常基类
    /// </summary>
    /// <remarks>
    /// <para>处理器执行期间抛出的异常，携带处理器类型和上下文。</para>
    /// </remarks>
    public class ProcessorException : EasyAttributeException
    {
        /// <summary>
        /// 上下文
        /// </summary>
        public IContext Context => context;

        /// <summary>
        /// 处理器类型
        /// </summary>
        public Type ProcessorType => processorType;

        private readonly IContext context;
        private readonly Type processorType;

        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="processorType">处理器类型</param>
        /// <param name="context">上下文</param>
        /// <param name="innerException">内部异常</param>
        public ProcessorException(string message, Type processorType, IContext context, Exception innerException)
        : base(message, innerException)
        {
            this.context = context;
            this.processorType = processorType;
        }
    }
}
