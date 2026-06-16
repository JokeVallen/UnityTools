using System;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 文件日志记录器配置
    /// </summary>
    public sealed class FileLoggerConfig
    {
        /// <summary>
        /// 日志文件目录
        /// </summary>
        /// <remarks>
        /// 默认为 "Logs"
        /// </remarks>
        public string LogDirectory { get; private set; }

        /// <summary>
        /// 日志文件名前缀
        /// </summary>
        /// <remarks>
        /// 默认为 "log"
        /// </remarks>
        public string FileNamePrefix { get; private set; }

        /// <summary>
        /// 日志文件最大大小（字节）
        /// </summary>
        /// <remarks>
        /// 默认为 10MB
        /// </remarks>
        public long MaxFileSizeBytes { get; private set; }

        /// <summary>
        /// 用于判断是否刷新缓冲区内容的最大大小（字节）
        /// </summary>
        /// <remarks>
        /// 默认为 1024 字节
        /// </remarks>
        public long MaxFlushBufferSizeBytes { get; private set; }

        /// <summary>
        /// 最大备份日志文件数量
        /// </summary>
        /// <remarks>
        /// 默认为 10
        /// </remarks>
        public int MaxBackupFiles { get; private set; }

        /// <summary>
        /// 是否自动刷新日志到文件
        /// </summary>
        /// <remarks>
        /// 默认为 true
        /// </remarks>
        public bool AutoFlush { get; private set; }

        /// <summary>
        /// 是否使用异步方式写日志
        /// </summary>
        /// <remarks>
        /// 默认为 false
        /// </remarks>
        public bool UseAsync { get; private set; }

        /// <summary>
        /// 刷新日志到文件的时间间隔（毫秒）
        /// </summary>
        /// <remarks>
        /// 默认为 5000 ms
        /// </remarks>
        public int FlushIntervalMilliseconds { get; private set; }

        /// <summary>
        /// 日志记录器配置
        /// </summary>
        public LoggerConfig Config { get; private set; }

        /// <summary>
        /// 协程代理
        /// </summary>
        public ICoroutineProxy CoroutineProxy { get; private set; }

        private FileLoggerConfig() { }

        /// <summary>
        /// 构建器
        /// </summary>
        public class Builder
        {
            private string logDirectory = "Logs";
            private string fileNamePrefix = "log";
            private long maxFileSizeBytes = 10 * 1024 * 1024;
            private long maxFlushBufferSizeBytes = 1024;
            private int maxBackupFiles = 10;
            private bool autoFlush = true;
            private bool useAsync = false;
            private int flushIntervalMilliseconds = 5000;
            private readonly LoggerConfig config;
            private readonly ICoroutineProxy coroutineProxy;
            private bool built;

            private Builder(LoggerConfig config, ICoroutineProxy coroutineProxy)
            {
                this.config = config;
                this.coroutineProxy = coroutineProxy;
            }

            /// <summary>
            /// 创建构建器实例
            /// </summary>
            /// <returns>构建器实例</returns>
            public static Builder Create(LoggerConfig config, ICoroutineProxy coroutineProxy)
            {
                if (config == null) throw new ArgumentNullException(nameof(config));
                if (coroutineProxy == null) throw new ArgumentNullException(nameof(coroutineProxy));
                return new Builder(config, coroutineProxy);
            }

            /// <summary>
            /// 设置日志文件目录
            /// </summary>
            /// <param name="logDirectory">日志文件目录</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// 默认值为 "Logs"
            /// </remarks>
            public Builder SetLogDirectory(string logDirectory)
            {
                ThrowErrorIfBuilt();
                this.logDirectory = logDirectory;
                return this;
            }

            /// <summary>
            /// 设置日志文件名前缀
            /// </summary>
            /// <param name="fileNamePrefix">日志文件名前缀</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// 默认值为 "log"
            /// </remarks>
            public Builder SetFileNamePrefix(string fileNamePrefix)
            {
                ThrowErrorIfBuilt();
                this.fileNamePrefix = fileNamePrefix;
                return this;
            }

            /// <summary>
            /// 设置日志文件最大大小（字节）
            /// </summary>
            /// <param name="maxFileSizeBytes">日志文件最大大小（字节）</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// 默认值为 10MB
            /// </remarks>
            public Builder SetMaxFileSizeBytes(long maxFileSizeBytes)
            {
                ThrowErrorIfBuilt();
                this.maxFileSizeBytes = Math.Max(maxFileSizeBytes, 1);
                return this;
            }

            /// <summary>
            /// 设置用于判断是否刷新缓冲区内容的最大大小（字节）
            /// </summary>
            /// <param name="maxFlushBufferSizeBytes">用于判断是否刷新缓冲区内容的最大大小（字节）</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// 默认值为 1024 字节
            /// </remarks>
            public Builder SetMaxFlushBufferSizeBytes(long maxFlushBufferSizeBytes)
            {
                ThrowErrorIfBuilt();
                this.maxFlushBufferSizeBytes = Math.Max(maxFlushBufferSizeBytes, 1);
                return this;
            }

            /// <summary>
            /// 设置最大备份日志文件数量
            /// </summary>
            /// <param name="maxBackupFiles">最大备份日志文件数量</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// 默认值为 10
            /// </remarks>
            public Builder SetMaxBackupFiles(int maxBackupFiles)
            {
                ThrowErrorIfBuilt();
                this.maxBackupFiles = Math.Max(maxBackupFiles, 0);
                return this;
            }

            /// <summary>
            /// 设置是否自动刷新日志到文件
            /// </summary>
            /// <param name="autoFlush">是否自动刷新日志到文件</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// 默认值为 true
            /// </remarks>
            public Builder SetAutoFlush(bool autoFlush)
            {
                ThrowErrorIfBuilt();
                this.autoFlush = autoFlush;
                return this;
            }

            /// <summary>
            /// 设置是否使用异步方式写日志
            /// </summary>
            /// <param name="useAsync">是否使用异步方式写日志</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// 默认值为 false
            /// </remarks>
            public Builder SetUseAsync(bool useAsync)
            {
                ThrowErrorIfBuilt();
                this.useAsync = useAsync;
                return this;
            }

            /// <summary>
            /// 设置刷新日志到文件的时间间隔（毫秒）
            /// </summary>
            /// <param name="flushIntervalMilliseconds">刷新日志到文件的时间间隔（毫秒）</param>
            /// <returns>构建器实例</returns>
            /// <remarks>
            /// 默认值为 5000 ms
            /// </remarks>
            public Builder SetFlushIntervalMilliseconds(int flushIntervalMilliseconds)
            {
                ThrowErrorIfBuilt();
                this.flushIntervalMilliseconds = flushIntervalMilliseconds;
                return this;
            }

            /// <summary>
            /// 构建配置实例
            /// </summary>
            /// <returns>配置实例</returns>
            public FileLoggerConfig Build()
            {
                ThrowErrorIfBuilt();
                built = true;
                return new FileLoggerConfig
                {
                    LogDirectory = logDirectory,
                    FileNamePrefix = fileNamePrefix,
                    MaxFileSizeBytes = maxFileSizeBytes,
                    MaxFlushBufferSizeBytes = maxFlushBufferSizeBytes,
                    MaxBackupFiles = maxBackupFiles,
                    AutoFlush = autoFlush,
                    UseAsync = useAsync,
                    FlushIntervalMilliseconds = flushIntervalMilliseconds,
                    Config = config,
                    CoroutineProxy = coroutineProxy
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
