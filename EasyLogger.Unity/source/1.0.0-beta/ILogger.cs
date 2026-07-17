namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void Log(LogLevel level, string message, params object[] args);

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void Trace(string message, params object[] args);

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void Info(string message, params object[] args);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void Warning(string message, params object[] args);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void Error(string message, params object[] args);

        /// <summary>
        /// 记录严重错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="args">格式化参数</param>
        void Fatal(string message, params object[] args);
    }
}