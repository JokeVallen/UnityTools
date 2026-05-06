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
            this.config = config ?? throw new ArgumentNullException(nameof(config));
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

        /// <inheritdoc/>
        public void Trace(string message, params object[] args) => Log(LogLevel.Trace, message, args);

        /// <inheritdoc/>
        public void Info(string message, params object[] args) => Log(LogLevel.Info, message, args);

        /// <inheritdoc/>
        public void Warning(string message, params object[] args) => Log(LogLevel.Warning, message, args);

        /// <inheritdoc/>
        public void Error(string message, params object[] args) => Log(LogLevel.Error, message, args);

        /// <inheritdoc/>
        public void Fatal(string message, params object[] args) => Log(LogLevel.Fatal, message, args);

#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
        /// <summary>
        /// 处理异常的辅助方法
        /// </summary>
        /// <param name="ex">异常信息</param>
        internal protected void HandleError(Exception ex)
        {
            if (ex == null) return;
            config.OnError?.Invoke(ex);
            if (config.ThrowOnError) throw ex;
        }
#else
        /// <summary>
        /// 处理异常的辅助方法
        /// </summary>
        /// <param name="ex">异常信息</param>
        protected void HandleError(Exception ex)
        {
            if (ex == null) return;
            config.OnError?.Invoke(ex);
            if (config.ThrowOnError) throw ex;
        }
#endif

        /// <summary>
        /// 通过格式化提供者格式化日志
        /// </summary>
        /// <param name="message">原始日志信息</param>
        /// <param name="args">格式化参数</param>
        /// <returns>通过 <see cref="IFormatProvider"/> 格式化的日志信息。</returns>
        protected string FormatMessageByFormatProvider(string message, object[] args)
        {
            if (args == null || args.Length == 0) return message;
            try
            {
                return config.FormatProvider == null
                    ? string.Format(message, args)
                    : string.Format(config.FormatProvider, message, args);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return $"{message} [Arguments: {string.Join(", ", args)}]";
            }
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
            try
            {
                return config.Formatter.Format(level, message);
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return message;
            }
        }
    }
}