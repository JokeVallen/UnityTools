namespace EasyLogger.Unity
{
    internal sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();
        private NullLogger() { }
        public void Log(LogLevel level, string message, params object[] args) { }
    }
}