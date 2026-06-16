using System;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志记录器基类
    /// </summary>
    public abstract class LoggerBase : ILogger
    {
        /// <summary>
        /// 日志记录器配置
        /// </summary>
        protected LoggerConfig Config => config;
        private readonly LoggerConfig config;

        /// <param name="config">日志记录器配置</param>
        protected LoggerBase(LoggerConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            this.config = config;
        }

        /// <inheritdoc/>
        public void Log(LogLevel level, string message, params object[] args)
        {
            if ((config.Levels & level) == LogLevel.None) return;
            DoLog(level, message, args);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        protected abstract void DoLog(LogLevel level, string message, params object[] args);

        /// <summary>
        /// 通过格式化提供者格式化日志
        /// </summary>
        /// <param name="message">原始日志信息</param>
        /// <param name="args">格式化参数</param>
        /// <returns>通过 <see cref="IFormatProvider"/> 格式化的日志信息。</returns>
        protected string FormatMessageByFormatProvider(string message, object[] args)
        {
            if (args == null || args.Length == 0) return message;
            return config.FormatProvider == null
                    ? string.Format(message, args)
                    : string.Format(config.FormatProvider, message, args);
        }

        /// <summary>
        /// 通过格式化器格式化日志
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="message">日志信息</param>
        /// <returns>通过 <see cref="ILogFormatter"/> 格式化的日志信息。</returns>
        protected string FormatMessageByFormatter(LogLevel level, string message)
        {
            if (config.Formatter == null) return message;
            return config.Formatter.Format(level, message);
        }
    }
}
