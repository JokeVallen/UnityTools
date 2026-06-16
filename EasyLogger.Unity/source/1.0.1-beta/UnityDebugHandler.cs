using System;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志工具类接管 <see cref="UnityEngine.Debug"/> 日志流的媒介
    /// </summary>
    internal static class UnityDebugHandler
    {
        private class Proxy : UnityEngine.ILogHandler
        {
            public static readonly UnityEngine.ILogHandler instance = new Proxy();

            private Proxy() { }

            public void LogFormat(UnityEngine.LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                string message = context != null ? $"{context}:{format}" : $"{format}";

                switch (logType)
                {
                    case UnityEngine.LogType.Exception:
                    case UnityEngine.LogType.Error:
                    case UnityEngine.LogType.Assert:
                        LogUtility.Error(message, args);
                        break;
                    case UnityEngine.LogType.Warning:
                        LogUtility.Warning(message, args);
                        break;
                    case UnityEngine.LogType.Log:
                        LogUtility.Info(message, args);
                        break;
                    default:
                        LogUtility.Info(message, args);
                        break;
                }
            }

            public void LogException(Exception exception, UnityEngine.Object context)
            {
                string message = context != null ? $"{context}:{exception}" : $"{exception}";
                LogUtility.Error(message);
            }
        }

        public static UnityEngine.ILogHandler UnityLogHandler => unityLogHandler;
        private static UnityEngine.ILogHandler unityLogHandler;

        public static void Dispose()
        {
            UnityEngine.Debug.unityLogger.logHandler = unityLogHandler;
            unityLogHandler = null;
        }

        public static void Initialize()
        {
            unityLogHandler = UnityEngine.Debug.unityLogger.logHandler;
            UnityEngine.Debug.unityLogger.logHandler = Proxy.instance;
        }
    }
}