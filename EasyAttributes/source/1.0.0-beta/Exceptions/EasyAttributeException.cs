using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 框架异常基类
    /// </summary>
    /// <remarks>
    /// <para>所有 EasyAttributes 框架产生的异常均继承此类。</para>
    /// <para>调用方可捕获此基类以统一处理框架层面的错误。</para>
    /// </remarks>
    public abstract class EasyAttributeException : Exception
    {
        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="message">消息</param>
        protected EasyAttributeException(string message) : base(message) { }

        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="innerException">内部异常</param>
        protected EasyAttributeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
