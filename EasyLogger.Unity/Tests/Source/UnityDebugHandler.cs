using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志库用于接管 <see cref="UnityEngine.Debug"/> 日志流的日志处理器
    /// </summary>
    internal static class UnityDebugHandler
    {
        private class Wrapper : ILogHandler
        {
            public static readonly ILogHandler instance = new Wrapper();

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                string message = context != null ? $"{context}:{format}" : $"{format}";

                switch (logType)
                {
                    case LogType.Exception:
                    case LogType.Error:
                    case LogType.Assert:
                        Debug.Error(message, args);
                        break;
                    case LogType.Warning:
                        Debug.Warning(message, args);
                        break;
                    case LogType.Log:
                        Debug.Info(message, args);
                        break;
                    default:
                        Debug.Info(message, args);
                        break;
                }
            }

            public void LogException(Exception exception, Object context)
            {
                string message = context != null ? $"{context}:{exception}" : $"{exception}";
                Debug.Error(message);
            }
        }

        public static ILogHandler UnityLogHandler => unityLogHandler;
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
        private static ILogHandler unityLogHandler;
#else
        private static readonly ILogHandler unityLogHandler;
#endif

        static UnityDebugHandler()
        {
            unityLogHandler = UnityEngine.Debug.unityLogger.logHandler;
            UnityEngine.Debug.unityLogger.logHandler = Wrapper.instance;
        }

        public static void HelloWorld() { }
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
        public static void SetUnityLogHandler(ILogHandler handler) => unityLogHandler = handler;
#endif
    }
}