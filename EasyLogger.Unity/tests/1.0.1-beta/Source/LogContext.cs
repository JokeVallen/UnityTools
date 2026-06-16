using System;
using System.Runtime.CompilerServices;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志上下文
    /// </summary>
    public readonly struct LogContext
    {
        /// <summary>
        /// 堆栈信息
        /// </summary>
        public string StackTrace { get; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 成员名称
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// 行号
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// 自定义数据
        /// </summary>
        public object UserData { get; }

        private LogContext(string stackTrace, string filePath, string memberName, int lineNumber, object userData) 
        {
            StackTrace = stackTrace;
            FilePath = filePath;
            MemberName = memberName;
            LineNumber = lineNumber;
            UserData = userData;
        }

        /// <summary>
        /// 捕获调用信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="memberName">成员名称</param>
        /// <param name="lineNumber">行号</param>
        /// <returns>日志上下文</returns>
        public static LogContext Capture([CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            return new LogContext(null, filePath, memberName, lineNumber, null);
        }

        /// <summary>
        /// 捕获调用信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="memberName">成员名称</param>
        /// <param name="lineNumber">行号</param>
        /// <returns>日志上下文</returns>
        public static LogContext CaptureWithStackTrace([CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            return new LogContext(Environment.StackTrace, filePath, memberName, lineNumber, null);
        }

        /// <summary>
        /// 捕获调用信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="memberName">成员名称</param>
        /// <param name="lineNumber">行号</param>
        /// <param name="userData">自定义数据</param>
        /// <returns>日志上下文</returns>
        public static LogContext CaptureWithUserData([CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0, object userData = null)
        {
            return new LogContext(null, filePath, memberName, lineNumber, userData);
        }

        /// <summary>
        /// 捕获调用信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="memberName">成员名称</param>
        /// <param name="lineNumber">行号</param>
        /// <param name="userData">自定义数据</param>
        /// <returns>日志上下文</returns>
        public static LogContext CaptureWithStackTraceAndUserData([CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0, object userData = null)
        {
            return new LogContext(Environment.StackTrace, filePath, memberName, lineNumber, userData);
        }
    }
}
