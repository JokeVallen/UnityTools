#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal static class EventDispatcherLog
    {
        private class NullLogger : ILogger
        {
            public static readonly NullLogger Instance = new NullLogger();
            private NullLogger() { }
            public void LogError(Type eventType, Delegate originalHandler, Exception exception) { }
            public void LogError(Exception exception) { }
            public void LogError(string message) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message) { }
        }

        private class DefaultLogger : ILogger
        {
            void ILogger.LogError(Type eventType, Delegate handler, Exception exception)
            {
                UnityEngine.Debug.LogError($"[EventHub] ({handler.Method.Name})({eventType.Name}){exception}");
            }

            void ILogger.LogError(Exception exception)
            {
                UnityEngine.Debug.LogError($"[EventHub] {exception}");
            }

            void ILogger.LogError(string message)
            {
                UnityEngine.Debug.LogError($"[EventHub] {message}");
            }

            void ILogger.LogInfo(string message)
            {
                UnityEngine.Debug.Log($"[EventHub] {message}");
            }

            void ILogger.LogWarning(string message)
            {
                UnityEngine.Debug.LogWarning($"[EventHub] {message}");
            }
        }

        public static bool Enabled = true;

        public static ILogger Logger
        {
            get => logger;
            set 
            {
                if (disposed) return;
                logger = value;
            }
        }
        private static ILogger logger = defaultLogger;

        private static bool disposed;
        private static readonly ILogger defaultLogger = new DefaultLogger();

        public static void LogError(Type eventType, Delegate handler, Exception exception)
        {
            if (!Enabled) return;
            try
            {
                var log = GetLogger();
                log.LogError(eventType, handler, exception);
            }
            catch (Exception ex)
            { 
                UnityEngine.Debug.LogError($"The method '{nameof(LogError)}' triggered an exception: {ex.Message}.");
            }
        }

        public static void LogError(Exception exception) 
        {
            if (!Enabled) return;
            try
            {
                var log = GetLogger();
                log.LogError(exception);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"The method '{nameof(LogError)}' triggered an exception: {ex.Message}.");
            }
        }

        public static void LogError(string message, [CallerMemberName] string methodName = null) 
        {
            if (!Enabled) return;
            try
            {
                var log = GetLogger();
                log.LogError($"[{methodName}]: {message}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"The method '{nameof(LogError)}' triggered an exception: {ex.Message}.");
            }
        }

        public static void LogWarning(string message, [CallerMemberName] string methodName = null)
        {
            if (!Enabled) return;
            try
            {
                var log = GetLogger();
                log.LogWarning($"[{methodName}]: {message}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"The method '{nameof(LogWarning)}' triggered an exception: {ex.Message}.");
            }
        }

        public static void LogInfo(string message, [CallerMemberName] string methodName = null)
        {
            if (!Enabled) return;
            try
            {
                var log = GetLogger();
                log.LogInfo($"[{methodName}]: {message}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"The method '{nameof(LogInfo)}' triggered an exception: {ex.Message}.");
            }
        }

        public static void Dispose() 
        {
            if (disposed) return;
            disposed = true;
            logger = NullLogger.Instance;
        }

        private static ILogger GetLogger() 
        {
            if (logger != null) return logger;
            return defaultLogger;
        }

#if EVENTHUB_TESTS
        internal static void ResetForTesting()
        {
            disposed = false;
            logger = defaultLogger;
            Enabled = true;
        }
#endif
    }
}

#endif