using System;

namespace FSM.Runtime
{
    /// <summary>
    /// 状态机异常
    /// </summary>
    /// <remarks>
    /// <para>框架运行时抛出的统一异常类型，调用方可通过捕获此类型精确处理框架内部错误。</para>
    /// <para>常见触发场景包括：重复启动、转换中强制切换、注册保留名称、构建配置不完整等。</para>
    /// </remarks>
    public class StateMachineException : Exception
    {
        /// <summary>
        /// 状态机异常
        /// </summary>
        /// <remarks>
        /// <para>使用指定错误消息构造异常实例。</para>
        /// </remarks>
        /// <param name="message">错误消息</param>
        public StateMachineException(string message) : base(message) { }

        /// <summary>
        /// 状态机异常
        /// </summary>
        /// <remarks>
        /// <para>使用指定错误消息和内部异常构造异常实例，适用于包装底层异常的场景。</para>
        /// </remarks>
        /// <param name="message">错误消息</param>
        /// <param name="inner">内部异常</param>
        public StateMachineException(string message, Exception inner) : base(message, inner) { }
    }
}
