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
#endregion