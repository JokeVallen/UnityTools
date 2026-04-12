#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal static class EventDispatcherLog
    {
        private class DefaultLogger : ILogger
        {
            void ILogger.LogError(Type eventType, Delegate handler, Exception exception)
            {
                UnityEngine.Debug.LogError($"[EventHub] Error: Error in {handler.Method.Name} for event {eventType.Name}: {exception}");
            }

            void ILogger.LogError(Exception exception)
            {
                UnityEngine.Debug.LogError($"[EventHub] Error: {exception}");
            }

            void ILogger.LogError(string message)
            {
                UnityEngine.Debug.LogError($"[EventHub] Error: {message}");
            }

            void ILogger.LogInfo(string message)
            {
                UnityEngine.Debug.Log($"[EventHub] Warning: {message}");
            }

            void ILogger.LogWarning(string message)
            {
                UnityEngine.Debug.LogWarning($"[EventHub] {message}");
            }
        }

        public static bool Enabled = true;

        public static ILogger Logger { get => logger; set => logger = value; }
        private static ILogger logger = defaultLogger;

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

        private static ILogger GetLogger() 
        {
            if (logger != null) return logger;
            return defaultLogger;
        }
    }
}

#endif