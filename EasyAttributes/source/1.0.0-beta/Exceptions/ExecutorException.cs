using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 执行器异常
    /// </summary>
    /// <remarks>
    /// <para>执行器调度层面的异常基类。</para>
    /// </remarks>
    public class ExecutorException : EasyAttributeException
    {
        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="message">消息</param>
        public ExecutorException(string message) : base(message){ }

        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="innerException">内部异常</param>
        public ExecutorException(string message, Exception innerException) : base(message, innerException){ }
    }
}
