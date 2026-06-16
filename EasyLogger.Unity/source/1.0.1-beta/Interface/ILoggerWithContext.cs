namespace EasyLogger.Unity
{
    /// <summary>
    /// 附带上下文的日志记录器接口
    /// </summary>
    public interface ILoggerWithContext : ILogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="context">日志上下文</param>
        /// <param name="args">格式化参数</param>
        void Log(LogLevel level, string message, LogContext context, params object[] args);
    }
}
