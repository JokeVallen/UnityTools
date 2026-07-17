namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录普通日志
        /// </summary>
        /// <param name="message">日志</param>
        void Info(string message);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志</param>
        void Warning(string message);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志</param>
        void Error(string message);
    }
}
