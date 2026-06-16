#region 测试辅助类
using System;
using System.Collections.Generic;
using EasyLogger.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

internal class TestLogHandler : ILogHandler
{
    public readonly List<(LogType type, string message)> Logs = new List<(LogType type, string message)>();
    public void LogFormat(LogType logType, Object context, string format, params object[] args)
        => Logs.Add((logType, string.Format(format, args)));
    public void LogException(Exception exception, Object context)
        => Logs.Add((LogType.Exception, exception.ToString()));
}

internal class TestableLogger : LoggerBase
{
    public readonly List<(LogLevel Level, string Message)> Logs = new List<(LogLevel Level, string Message)>();

    public TestableLogger(LoggerConfig config) : base(config) { }

    protected override void DoLog(LogLevel level, string message, params object[] args)
    {
        string formatted = FormatMessageByFormatProvider(message, args);
        formatted = FormatMessageByFormatter(level, formatted);
        Logs.Add((level, formatted));
    }
}

internal sealed class LogDriver : MonoBehaviour, ICoroutineProxy
{
    public static LogDriver Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("LogDriver");
                DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.DontSave;
                instance = go.AddComponent<LogDriver>();
            }
            return instance;
        }
    }
    private static LogDriver instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(this);
            return;
        }
        hideFlags = HideFlags.DontSave;
    }

    private void LateUpdate()
    {
        LogUtility.Flush();
    }

    private void OnDestroy()
    {
        LogUtility.Flush();
    }
}
#endregion

public class SilentTestLogger : EasyLogger.Unity.ILogger
{
    public void Log(LogLevel level, string message, params object[] args) { }
    public void Trace(string message, params object[] args) { }
    public void Info(string message, params object[] args) { }
    public void Warning(string message, params object[] args) { }
    public void Error(string message, params object[] args) { }
    public void Fatal(string message, params object[] args) { }
}

public static class Extension
{
    public static void Info(this EasyLogger.Unity.ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Info, message, args);
    }

    public static void Warning(this EasyLogger.Unity.ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Warning, message, args);
    }

    public static void Error(this EasyLogger.Unity.ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Error, message, args);
    }

    public static void Trace(this EasyLogger.Unity.ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Trace, message, args);
    }

    public static void Fatal(this EasyLogger.Unity.ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Fatal, message, args);
    }
}