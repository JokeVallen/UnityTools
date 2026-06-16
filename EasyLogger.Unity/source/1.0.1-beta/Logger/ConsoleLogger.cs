namespace EasyLogger.Unity
{
    /// <summary>
    /// 控制台日志记录器
    /// </summary>
    /// <remarks>
    /// <para>封装 <see cref="UnityEngine.Debug"/> 控制台输出。</para>
    /// </remarks>
    public sealed class ConsoleLogger : LoggerBase, ILoggerWithContext
    {
        /// <param name="config">日志器配置</param>
        public ConsoleLogger(LoggerConfig config) : base(config) { }

        /// <inheritdoc/>
        protected override void DoLog(LogLevel level, string message, params object[] args)
        {
            string formatted = FormatMessageByFormatProvider(message, args);
            formatted = FormatMessageByFormatter(level, formatted);
            UnityDebugHandler.UnityLogHandler.LogFormat(GetLogType(level), null, formatted);
        }

        /// <inheritdoc/>
        public void Log(LogLevel level, string message, LogContext context, params object[] args)
        {
            string formatted = FormatMessageByFormatProvider(message, args);
            formatted = FormatMessageByFormatter(level, formatted);

            var unityObject = context.UserData as UnityEngine.Object;
            if (!string.IsNullOrEmpty(context.FilePath) && context.LineNumber > 0)
            {
                string relativePath = NormalizePath(context.FilePath);
                formatted = $"{formatted} ({relativePath}:{context.LineNumber})";
            }

            UnityDebugHandler.UnityLogHandler.LogFormat(GetLogType(level), unityObject, formatted);
        }

        private static string NormalizePath(string fullPath)
        {
            if (fullPath.StartsWith("Assets")) return fullPath;
            string dataPath = UnityEngine.Application.dataPath;
            if (fullPath.StartsWith(dataPath))
                return "Assets" + fullPath.Substring(dataPath.Length);
            return fullPath;
        }

        private static UnityEngine.LogType GetLogType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Info:
                    return UnityEngine.LogType.Log;
                case LogLevel.Warning:
                    return UnityEngine.LogType.Warning;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    return UnityEngine.LogType.Error;
                default:
                    return UnityEngine.LogType.Log;
            }
        }
    }
}
