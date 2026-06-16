using System;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志记录器配置类
    /// </summary>
    public sealed class LoggerConfig
    {
        /// <summary>
        /// 日志级别位域值
        /// </summary>
        /// <remarks>
        /// <para>默认为全日志级别覆盖</para>
        /// <para>该属性决定了日志记录器能够记录的日志级别。</para>
        /// </remarks>
        public LogLevel Levels { get; private set; }

        /// <summary>
        /// 日志格式化器
        /// </summary>
        /// <remarks>
        /// <para>默认值为 <see cref="DefaultLogFormatter"/></para>
        /// <para>职责是对单个日志信息进行格式化处理，不包括格式化参数，通常可以用它来附加额外信息或者注入单独的显示样式。</para>
        /// </remarks>
        public ILogFormatter Formatter { get; private set; }

        /// <summary>
        /// 格式化提供者
        /// </summary>
        /// <remarks>
        /// <para>默认值为 null</para>
        /// <para>C#官方接口，常见于 <see cref="string.Format(IFormatProvider, string, object)"/> 等系列 API 方法。</para>
        /// </remarks>
        public IFormatProvider FormatProvider { get; private set; }

        private LoggerConfig() { }

        /// <summary>
        /// 构建器
        /// </summary>
        public class Builder
        {
            private LogLevel levels = (LogLevel)(((long)LogLevel.Max << 1) - 1);
            private ILogFormatter formatter = new DefaultLogFormatter();
            private IFormatProvider formatProvider;
            private bool built;

            private Builder() { }

            /// <summary>
            /// 创建构建器实例
            /// </summary>
            /// <returns>构建器实例</returns>
            public static Builder Create()
            {
                return new Builder();
            }

            /// <summary>
            /// 设置日志级别位域值
            /// </summary>
            /// <param name="levels">日志级别位域值</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>默认为全日志级别覆盖</para>
            /// <para>可以任意组合该日志记录器可记录的日志级别。</para>
            /// </remarks>
            public Builder SetLevels(LogLevel levels)
            {
                ThrowErrorIfBuilt();
                this.levels = levels;
                return this;
            }

            /// <summary>
            /// 设置日志最小级别
            /// </summary>
            /// <param name="minLevel">日志最小级别</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>日志级别覆盖范围：[<paramref name="minLevel"/>, <see cref="LogLevel.Max"/>]</para>
            /// </remarks>
            public Builder SetMinLevel(LogLevel minLevel)
            {
                ThrowErrorIfBuilt();
                levels = (LogLevel)(((int)minLevel - 1) ^ (((long)LogLevel.Max << 1) - 1));
                return this;
            }

            /// <summary>
            /// 设置日志最大级别
            /// </summary>
            /// <param name="maxLevel">日志最大级别</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>日志级别覆盖范围：[<see cref="LogLevel.Min"/>, <paramref name="maxLevel"/>]</para>
            /// </remarks>
            public Builder SetMaxLevel(LogLevel maxLevel)
            {
                ThrowErrorIfBuilt();
                levels = (LogLevel)(((int)LogLevel.Min - 1) ^ (((long)maxLevel << 1) - 1));
                return this;
            }

            /// <summary>
            /// 设置日志最小和最大级别
            /// </summary>
            /// <param name="minLevel">日志最小级别</param>
            /// <param name="maxLevel">日志最大级别</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>日志级别覆盖范围：[<paramref name="minLevel"/>, <paramref name="maxLevel"/>]</para>
            /// </remarks>
            public Builder SetMinMaxLevel(LogLevel minLevel, LogLevel maxLevel)
            {
                ThrowErrorIfBuilt();
                levels = (LogLevel)(((int)minLevel - 1) ^ (((long)maxLevel << 1) - 1));
                return this;
            }

            /// <summary>
            /// 设置日志格式化器
            /// </summary>
            /// <param name="formatter">日志格式化器</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>默认值为 <see cref="DefaultLogFormatter"/></para>
            /// <para>职责是对单个日志信息进行格式化处理，不包括格式化参数，通常可以用它来附加额外信息或者注入单独的显示样式。</para>
            /// </remarks>
            public Builder SetFormatter(ILogFormatter formatter)
            {
                ThrowErrorIfBuilt();
                this.formatter = formatter;
                return this;
            }

            /// <summary>
            /// 设置格式化提供者
            /// </summary>
            /// <param name="formatProvider">格式化提供者</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>默认值为 null</para>
            /// <para>C#官方接口，常见于 <see cref="string.Format(IFormatProvider, string, object)"/> 等系列 API 方法。</para>
            /// </remarks>
            public Builder SetFormatProvider(IFormatProvider formatProvider)
            {
                ThrowErrorIfBuilt();
                this.formatProvider = formatProvider;
                return this;
            }

            /// <summary>
            /// 构建日志记录器配置实例
            /// </summary>
            /// <returns>日志记录器配置实例</returns>
            public LoggerConfig Build()
            {
                ThrowErrorIfBuilt();
                built = true;
                return new LoggerConfig()
                {
                    Levels = levels,
                    Formatter = formatter,
                    FormatProvider = formatProvider
                };
            }

            private void ThrowErrorIfBuilt()
            {
                if (built)
                    throw new InvalidOperationException("[EasyLogger] The builder cannot be reused.");
            }
        }
    }
}