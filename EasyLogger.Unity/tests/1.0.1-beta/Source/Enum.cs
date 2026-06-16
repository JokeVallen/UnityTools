using System;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志级别（位域）
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        /// 缺省值
        /// </summary>
        None = 0,

        /// <summary>
        /// 最小日志级别
        /// </summary>
        /// <remarks>
        /// <para>注意：不承诺该值在未来版本不会发生改变。</para>
        /// </remarks>
        Min = Trace,

        /// <summary>
        /// 最大日志级别
        /// </summary>
        /// <remarks>
        /// <para>注意：不承诺该值在未来版本不会发生改变。</para>
        /// </remarks>
        Max = Fatal,

        /// <summary>
        /// 堆栈跟踪信息
        /// </summary>
        /// <remarks>
        /// 建议场景：记录方法调用和执行路径等详细信息，通常用于调试和性能分析
        /// </remarks>
        Trace = 1 << 0,

        /// <summary>
        /// 普通信息
        /// </summary>
        /// <remarks>
        /// 建议场景：系统正常运行时使用
        /// </remarks>
        Info = 1 << 1,

        /// <summary>
        /// 警告
        /// </summary>
        /// <remarks>
        /// 建议场景：系统仍然可以继续运行，但可能存在潜在问题时使用
        /// </remarks>
        Warning = 1 << 2,

        /// <summary>
        /// 常规错误
        /// </summary>
        /// <remarks>
        /// 建议场景：系统仍然可以继续运行时使用
        /// </remarks>
        Error = 1 << 3,

        /// <summary>
        /// 严重错误
        /// </summary>
        /// <remarks>
        /// 建议场景：系统崩溃或无法继续运行时使用
        /// </remarks>
        Fatal = 1 << 4
    }
}
