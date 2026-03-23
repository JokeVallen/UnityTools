#if UNITY_EDITOR

using System;

namespace EditorTools.NameModifier
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    /// <remarks>
    /// <para>
    /// 定义名称修改器工具所使用的日志输出契约。默认实现为
    /// <c>NameModifierDefaultLogger</c>，通过 <see cref="NameModifierConfig.Logger"/>
    /// 可替换为自定义实现，以便对接项目自身的日志系统。
    /// </para>
    /// <para>
    /// 所有方法的启用与否由 <see cref="NameModifierConfig"/> 中的
    /// <c>LogEnabled</c> 开关统一控制（默认实现层面）；自定义实现可自行决定过滤策略。
    /// </para>
    /// </remarks>
    public interface INameModifierLogger
    {
        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <remarks>
        /// 对应 <c>Debug.Log</c> 级别，用于输出常规信息，例如操作完成提示。
        /// </remarks>
        void Log(object message);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <remarks>
        /// 对应 <c>Debug.LogWarning</c> 级别，用于输出非致命的异常情况提示。
        /// </remarks>
        void LogWarning(object message);

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <remarks>
        /// 对应 <c>Debug.LogError</c> 级别，用于输出已捕获的错误信息。
        /// </remarks>
        void LogError(object message);

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <remarks>
        /// 对应 <c>Debug.LogException</c> 级别，用于输出完整的异常堆栈信息。
        /// </remarks>
        void LogException(Exception exception);
    }
}

#endif