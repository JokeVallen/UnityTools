using System;
using System.Collections.Concurrent;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志工具类
    /// </summary>
    /// <remarks>
    /// <para>非线程安全</para>
    /// </remarks>
    public static class Debug
    {
        private static ILogger Logger => logger ?? defaultLogger;
        private static bool Disposed => Logger == NullLogger.Instance;
        private static ILogger logger;
        private static ILogger defaultLogger;
        private static int flags;
        private static readonly ConcurrentQueue<LogMessage> queue = new ConcurrentQueue<LogMessage>();

        static Debug()
        {
            var config = LoggerConfig.Builder.Create().Build();
            defaultLogger = new ConsoleLogger(config);

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
#else
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
#endif
        }

        /// <summary>
        /// 在 Unity 线程上释放资源
        /// </summary>
        /// <remarks>
        /// <para>该方法用于释放依赖 Unity API 的日志记录器的相关资源，日志记录器可以实现 <see cref="IUnityThreadLogger"/> 接口提供对 Unity 资源的释放功能。</para>
        /// </remarks>
        public static void DisposeOnUnityThread()
        {
            if (Disposed) return;
            DisposeOnUnityThreadInternal();
        }

        /// <summary>
        /// 用于自行控制日志库工具激活时机的方法
        /// </summary>
        /// <remarks>
        /// <para>默认是在该类的静态成员首次被访问时进行激活，若需要预热可以在项目更早的时机调用该方法显式激活。</para>
        /// </remarks>
        public static void HelloWorld()
        {
            if (Disposed) return;
            UnityDebugHandler.HelloWorld();
        }

        /// <summary>
        /// 配置自定义日志记录器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <returns>旧版本的日志记录器，若不存在则返回 null。</returns>
        /// <remarks>
        /// <para>默认使用 <see cref="ConsoleLogger"/></para>
        /// </remarks>
        public static ILogger Configure(ILogger logger)
        {
            if (Disposed) return null;
            if (logger == null) return null;
            var old = Debug.logger;
            Debug.logger = logger;
            return old;
        }

        /// <summary>
        /// 将暂存在日志处理队列中的日志刷新到日志记录器中进行处理
        /// </summary>
        public static void Flush()
        {
            if (Disposed) return;
            FlushInternal();
        }

        /// <summary>
        /// 记录普通日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        public static void Info(string message, params object[] args)
        {
            EnqueueLog(LogLevel.Info, message, args);
        }

        /// <summary>
        /// 记录警告日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        public static void Warning(string message, params object[] args)
        {
            EnqueueLog(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// 记录错误日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        public static void Error(string message, params object[] args)
        {
            EnqueueLog(LogLevel.Error, message, args);
        }

        /// <summary>
        /// 记录严重错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        public static void Fatal(string message, params object[] args)
        {
            EnqueueLog(LogLevel.Fatal, message, args);
        }

        /// <summary>
        /// 仅在TRACE模式下输出的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        [System.Diagnostics.Conditional("TRACE")]
        public static void Trace(string message, params object[] args)
        {
            EnqueueLog(LogLevel.Trace, message, args);
        }

#if UNITY_EDITOR
        private static void OnBeforeAssemblyReload()
        {
            DisposeOnUnityThreadInternal();
            DisposeInternal();
        }
#else
    private static void OnProcessExit(object sender, EventArgs e)
    {
        DisposeInternal();
    }
#endif

        private static void DisposeOnUnityThreadInternal()
        {
            if (Disposed) return;
            FlushInternal();
            DisposeUnityThreadLogger(ref logger, ref flags, 1 << 0, 1 << 1);
            DisposeUnityThreadLogger(ref defaultLogger, ref flags, 1 << 2, 1 << 3);
        }

        private static void DisposeInternal()
        {
            if (Disposed) return;
            FlushInternal();
            DisposeLogger(ref logger, ref flags, 1 << 1, 1 << 0);
            DisposeLogger(ref defaultLogger, ref flags, 1 << 3, 1 << 2);
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

        private static void FlushInternal()
        {
            int count = queue.Count;
            while (count > 0)
            {
                count--;
                if (!queue.TryDequeue(out var logMessage)) continue;
                PublishWithLogger(logMessage);
            }
        }

        private static void PublishWithLogger(LogMessage logMessage)
        {
            switch (logMessage.Level)
            {
                case LogLevel.Info:
                    Logger.Info(logMessage.Message, logMessage.Args);
                    break;
                case LogLevel.Warning:
                    Logger.Warning(logMessage.Message, logMessage.Args);
                    break;
                case LogLevel.Error:
                    Logger.Error(logMessage.Message, logMessage.Args);
                    break;
                case LogLevel.Fatal:
                    Logger.Fatal(logMessage.Message, logMessage.Args);
                    break;
                case LogLevel.Trace:
                    Logger.Trace(logMessage.Message, logMessage.Args);
                    break;
                default:
                    Logger.Info(logMessage.Message, logMessage.Args);
                    break;
            }
        }

        private static void EnqueueLog(LogLevel level, string message, params object[] args)
        {
            if (Disposed) return;
            queue.Enqueue(new LogMessage(level, message, args));
        }
    }
}