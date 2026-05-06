using System;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志记录器配置类
    /// </summary>
    public sealed class LoggerConfig
    {
        /// <summary>
        /// 是否显式抛出异常
        /// </summary>
        /// <remarks>
        /// <para>默认为 true</para>
        /// <para>对于日志库本身的异常将根据该属性采用不同策略，显式抛出异常或者通过 <see cref="OnError"/> 回调自行处理。</para>
        /// </remarks>
        public bool ThrowOnError { get; private set; }

        /// <summary>
        /// 异常自行处理回调
        /// </summary>
        /// <remarks>
        /// <para>默认为 null</para>
        /// <para>无论 <see cref="ThrowOnError"/> 为何值都会触发回调，但通常配合 <see cref="ThrowOnError"/>=false 使用。</para>
        /// </remarks>
        public Action<Exception> OnError { get; private set; }

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
            private bool throwOnError = true;
            private Action<Exception> onError = null;
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
            /// 设置是否显式抛出异常
            /// </summary>
            /// <param name="throwOnError">是否抛出异常</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>默认为 true</para>
            /// <para>对于日志库本身的异常将根据该属性采用不同策略，显式抛出异常或者通过 <see cref="OnError"/> 回调自行处理。</para>
            /// </remarks>
            public Builder SetThrowOnError(bool throwOnError)
            {
                CheckBuilt();
                this.throwOnError = throwOnError;
                return this;
            }

            /// <summary>
            /// 设置异常自行处理回调
            /// </summary>
            /// <param name="onError">异常自行处理回调</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// <para>默认为 null</para>
            /// <para>无论 <see cref="ThrowOnError"/> 为何值都会触发回调，但通常配合 <see cref="ThrowOnError"/>=false 使用。</para>
            /// </remarks>
            public Builder SetOnError(Action<Exception> onError)
            {
                CheckBuilt();
                this.onError = onError;
                return this;
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
                CheckBuilt();
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
                CheckBuilt();
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
                CheckBuilt();
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
                CheckBuilt();
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
                CheckBuilt();
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
                CheckBuilt();
                this.formatProvider = formatProvider;
                return this;
            }

            /// <summary>
            /// 构建日志记录器配置实例
            /// </summary>
            /// <returns>日志记录器配置实例</returns>
            public LoggerConfig Build()
            {
                CheckBuilt();
                built = true;
                return new LoggerConfig()
                {
                    ThrowOnError = throwOnError,
                    OnError = onError,
                    Levels = levels,
                    Formatter = formatter,
                    FormatProvider = formatProvider
                };
            }

            private void CheckBuilt()
            {
                if (built)
                    throw new InvalidOperationException("The builder cannot be reused.");
            }
        }
    }
}