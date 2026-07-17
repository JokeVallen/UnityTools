namespace EasyLogger.Unity
{
    /// <summary>
    /// 控制台日志记录器
    /// </summary>
    /// <remarks>
    /// <para>封装 <see cref="UnityEngine.Debug"/> 控制台输出。</para>
    /// <para>非线程安全</para>
    /// </remarks>
    public sealed class ConsoleLogger : LoggerBase, IUnityThreadLogger
    {
        public ConsoleLogger(LoggerConfig config) : base(config) { }
        public void DisposeOnUnityThread() { }

        protected override void DoLog(LogLevel level, string message, params object[] args)
        {
            string formatted = FormatMessageByFormatProvider(message, args);
            formatted = FormatMessageByFormatter(level, formatted);
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Info:
                    UnityDebugHandler.UnityLogHandler.LogFormat(UnityEngine.LogType.Log, null, formatted);
                    break;
                case LogLevel.Warning:
                    UnityDebugHandler.UnityLogHandler.LogFormat(UnityEngine.LogType.Warning, null, formatted);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    UnityDebugHandler.UnityLogHandler.LogFormat(UnityEngine.LogType.Error, null, formatted);
                    break;
                default:
                    UnityDebugHandler.UnityLogHandler.LogFormat(UnityEngine.LogType.Log, null, formatted);
                    break;
            }
        }
    }
}