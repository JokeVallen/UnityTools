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
        /// 格式化参数
        /// </summary>
        public object[] Args { get; }

        public LogMessage(LogLevel level, string message, params object[] args)
        {
            Level = level;
            Message = message;
            Args = args;
        }
    }
}