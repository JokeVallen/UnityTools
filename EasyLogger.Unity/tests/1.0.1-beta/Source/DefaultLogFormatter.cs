using System;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 默认日志格式化器
    /// </summary>
    public sealed class DefaultLogFormatter : ILogFormatter
    {
        /// <inheritdoc/>
        public string Format(LogLevel level, string message)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level.ToString().ToUpper()}] {message}";
        }
    }
}
