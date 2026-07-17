namespace ViewPipeline.Unity.Core
{
    internal static class Log
    {
        private class DefaultLogger : ILogger
        {
            public void Error(string message)
            {
                UnityEngine.Debug.LogError(message);
            }

            public void Info(string message)
            {
                UnityEngine.Debug.Log(message);
            }

            public void Warning(string message)
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }

        public static ILogger Logger { internal get => GetLogger(); set => logger = value; }
        private static ILogger logger;
        private static readonly DefaultLogger defaultLogger = new DefaultLogger();

        private static ILogger GetLogger() 
        {
            if (logger != null) return logger;
            return defaultLogger;
        }
    }
}
