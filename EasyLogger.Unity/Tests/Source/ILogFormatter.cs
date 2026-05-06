namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志格式化接口
    /// </summary>
    /// <remarks>
    /// <para>职责是对单个日志信息进行格式化处理，不包括格式化参数，通常可以用它来附加额外信息或者注入单独的显示样式。</para>
    /// </remarks>
    public interface ILogFormatter
    {
        /// <summary>
        /// 格式化日志信息
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志信息</param>
        /// <returns>格式化后的完整日志行</returns>
        string Format(LogLevel level, string message);
    }
}