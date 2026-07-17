using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 日志门面类
    /// </summary>
    internal static class Log
    {
        private class DefaultLogger : ILogger
        {
            public void Error(string message)
            {
                Console.WriteLine($"[Error]:{message}");
            }

            public void Info(string message)
            {
                Console.WriteLine($"[Info]:{message}");
            }

            public void Warning(string message)
            {
                Console.WriteLine($"[Warn]:{message}");
            }
        }

        /// <summary>
        /// 日志记录器
        /// </summary>
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
