using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 上下文异常
    /// </summary>
    /// <remarks>
    /// <para>上下文操作相关的异常基类，携带引发异常的 <see cref="IContext"/> 实例。</para>
    /// </remarks>
    public class ContextException : EasyAttributeException
    {
        /// <summary>
        /// 上下文
        /// </summary>
        public IContext Context => context;
        private readonly IContext context;

        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="context">上下文</param>
        public ContextException(string message, IContext context) : base(message)
        {
            this.context = context;
        }

        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="innerException">内部异常</param>
        /// <param name="context">上下文</param>
        public ContextException(string message, Exception innerException, IContext context) : base(message, innerException)
        {
            this.context = context;
        }
    }
}
