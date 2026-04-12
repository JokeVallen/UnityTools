#if !EVENTHUB_EXTENSION_ENABLE

using System;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    [Preserve]
    public interface ILogger
    {
        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="originalHandler">原生事件委托</param>
        /// <param name="exception">异常</param>
        void LogError(Type eventType, Delegate originalHandler, Exception exception);

        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="exception">异常</param>
        void LogError(Exception exception);

        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="message">错误信息</param>
        void LogError(string message);

        /// <summary>
        /// 记录警告
        /// </summary>
        /// <param name="message">警告信息</param>
        void LogWarning(string message);

        /// <summary>
        /// 记录普通信息
        /// </summary>
        /// <param name="message">普通信息</param>
        void LogInfo(string message);
    }
}

#endif