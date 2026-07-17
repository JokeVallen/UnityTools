namespace EasyLogger.Unity
{
    internal sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();
        private NullLogger() { }
        public void Log(LogLevel level, string message, params object[] args) { }
        public void Trace(string message, params object[] args) { }
        public void Info(string message, params object[] args) { }
        public void Warning(string message, params object[] args) { }
        public void Error(string message, params object[] args) { }
        public void Fatal(string message, params object[] args) { }
    }
}