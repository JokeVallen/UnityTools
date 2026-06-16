namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志数据单元
    /// </summary>
    internal readonly struct LogMessage
    {
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// 原始日志信息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 日志上下文
        /// </summary>
        public Optional<LogContext> Context { get; }

        /// <summary>
        /// 格式化参数
        /// </summary>
        public object[] Args { get; }

        public LogMessage(LogLevel level, string message, Optional<LogContext> context, params object[] args)
        {
            Level = level;
            Message = message;
            Context = context;
            Args = args;
        }
    }
}