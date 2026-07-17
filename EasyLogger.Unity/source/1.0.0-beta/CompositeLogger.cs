using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 复合日志记录器
    /// </summary>
    /// <remarks>
    /// <para>可以将日志同时输出到多个日志记录器中。</para>
    /// <para>非线程安全</para>
    /// </remarks>
    public sealed class CompositeLogger : LoggerBase, IDisposable, IUnityThreadLogger
    {
        private readonly List<ILogger> loggers = new List<ILogger>();
        private readonly List<int> flags = new List<int>();
        private bool disposed;

        /// <param name="config">日志记录器配置</param>
        /// <param name="loggers">单个或多个日志记录器</param>
        public CompositeLogger(LoggerConfig config, params ILogger[] loggers) : base(config)
        {
            if (loggers != null && loggers.Length > 0)
            {
                this.loggers.AddRange(loggers);
                for (int i = 0; i < this.loggers.Count; i++)
                    flags.Add(0);
            }
        }

        /// <summary>
        /// 添加日志记录器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <remarks>
        /// <para>附带查重检测</para>
        /// </remarks>
        public void Add(ILogger logger)
        {
            if (disposed)
            {
                HandleError(new ObjectDisposedException(nameof(CompositeLogger)));
                return;
            }

            if (logger == null || ReferenceEquals(logger, this) || loggers.Contains(logger)) return;
            loggers.Add(logger);
            flags.Add(0);
        }

        /// <summary>
        /// 移除日志记录器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        public void Remove(ILogger logger)
        {
            if (disposed)
            {
                HandleError(new ObjectDisposedException(nameof(CompositeLogger)));
                return;
            }

            if (logger == null) return;
            int index = loggers.IndexOf(logger);
            if (index == -1) return;

            loggers.RemoveAt(index);
            flags.RemoveAt(index);
        }

        /// <summary>
        /// 清空日志记录器
        /// </summary>
        public void Clear()
        {
            if (disposed)
            {
                HandleError(new ObjectDisposedException(nameof(CompositeLogger)));
                return;
            }

            loggers.Clear();
            flags.Clear();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        public void DisposeOnUnityThread() => DisposeOnUnityThreadInternal();

        /// <inheritdoc/>
        protected override void DoLog(LogLevel level, string message, params object[] args)
        {
            if (disposed)
            {
                HandleError(new ObjectDisposedException(nameof(CompositeLogger)));
                return;
            }

            foreach (var logger in loggers)
            {
                try
                {
                    logger.Log(level, message, args);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            }
        }

        private void DisposeInternal()
        {
            if (disposed) return;

            int count = loggers.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var logger = loggers[i];
                var flag = flags[i];
                if (logger == NullLogger.Instance || !(logger is IDisposable)) continue;
                DisposeLogger(ref logger, ref flag, 1 << 0, 1 << 1);
                loggers[i] = logger;
                flags[i] = flag;
            }

            if (loggers.All(log => log == NullLogger.Instance))
            {
                loggers.Clear();
                flags.Clear();
                disposed = true;
            }
        }

        public void DisposeOnUnityThreadInternal()
        {
            if (disposed) return;

            int count = loggers.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var logger = loggers[i];
                var flag = flags[i];
                if (logger == NullLogger.Instance || !(logger is IUnityThreadLogger)) continue;
                DisposeUnityThreadLogger(ref logger, ref flag, 1 << 1, 1 << 0);
                loggers[i] = logger;
                flags[i] = flag;
            }

            if (loggers.All(log => log == NullLogger.Instance))
            {
                loggers.Clear();
                flags.Clear();
                disposed = true;
            }
        }

        private static void DisposeUnityThreadLogger(ref ILogger logger, ref int flags, int unitFlag, int otherUnitFlag)
        {
            if ((flags & unitFlag) == 0 && logger is IUnityThreadLogger unityThreadLogger)
            {
                try
                {
                    unityThreadLogger.DisposeOnUnityThread();
                }
                catch (Exception)
                {
                    // 忽略释放异常
                }
                finally
                {
                    flags |= unitFlag;
                    if (!(logger is IDisposable) || (flags & otherUnitFlag) != 0) logger = NullLogger.Instance;
                }
            }
        }

        private static void DisposeLogger(ref ILogger logger, ref int flags, int unitFlag, int otherUnitFlag)
        {
            if ((flags & unitFlag) == 0 && logger is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception)
                {
                    // 忽略释放异常
                }
                finally
                {
                    flags |= unitFlag;
                    if (!(logger is IUnityThreadLogger) || (flags & otherUnitFlag) != 0) logger = NullLogger.Instance;
                }
            }
        }
    }
}
