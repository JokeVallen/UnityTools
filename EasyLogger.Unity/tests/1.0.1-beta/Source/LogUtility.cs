using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 日志工具类
    /// </summary>
    public static class LogUtility
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        public static ILogger Logger => logger == null ? defaultLogger : logger;
        private static bool Disposed => Logger == NullLogger.Instance;
        private static ILogger logger;
        private static ILogger defaultLogger;
        private static int flags;
        private static readonly ConcurrentQueue<LogMessage> queue = new ConcurrentQueue<LogMessage>();

        static LogUtility()
        {
            var config = LoggerConfig.Builder.Create().Build();
            defaultLogger = new ConsoleLogger(config);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.quitting += OnEditorApplicationQuit;
#else
            UnityEngine.Application.quitting += OnApplicationQuit;
#endif
            UnityDebugHandler.Initialize();
        }

        /// <summary>
        /// 配置自定义日志记录器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <remarks>
        /// <para>默认使用 <see cref="ConsoleLogger"/></para>
        /// </remarks>
        public static void Configure(ILogger logger)
        {
            ThrowErrorIfDisposed();
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            LogUtility.logger = logger;
        }

        /// <summary>
        /// 将暂存在日志处理队列中的日志刷新到日志记录器中进行处理
        /// </summary>
        public static void Flush()
        {
            ThrowErrorIfDisposed();
            FlushInternal();
        }

        /// <summary>
        /// 启用自动刷新消息缓冲区机制
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <remarks>
        /// <para>该方法基于 <see cref="MonoBehaviour"/> 的协程，请注意方法调用线程和时机。</para>
        /// </remarks>
        public static void EnableAutoFlush(float interval)
        {
            if (Proxy.Instance == null)
            {
                var go = new GameObject(nameof(LogUtility));
                go.AddComponent<Proxy>();
                go.hideFlags = HideFlags.HideAndDontSave;
            }
            Proxy.Instance.StartTimer(interval);
        }

        /// <summary>
        /// 禁用自动刷新消息缓冲区机制
        /// </summary>
        public static void DisableAutoFlush()
        {
            if (Proxy.Instance != null)
                Proxy.Instance.StopTimer();
        }

        /// <summary>
        /// 记录普通日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        public static void Info(object message, params object[] args)
        {
            EnqueueMessage(LogLevel.Info, message, Optional<LogContext>.None, args);
        }

        /// <summary>
        /// 记录普通日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="context">日志上下文</param>
        /// <param name="args">格式化参数</param>
        public static void Info(object message, LogContext context, params object[] args)
        {
            EnqueueMessage(LogLevel.Info, message, context, args);
        }

        /// <summary>
        /// 记录警告日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        public static void Warning(object message, params object[] args)
        {
            EnqueueMessage(LogLevel.Warning, message, Optional<LogContext>.None, args);
        }

        /// <summary>
        /// 记录警告日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="context">日志上下文</param>
        /// <param name="args">格式化参数</param>
        public static void Warning(object message, LogContext context, params object[] args)
        {
            EnqueueMessage(LogLevel.Warning, message, context, args);
        }

        /// <summary>
        /// 记录错误日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        public static void Error(object message, params object[] args)
        {
            EnqueueMessage(LogLevel.Error, message, Optional<LogContext>.None, args);
        }

        /// <summary>
        /// 记录错误日志信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="context">日志上下文</param>
        /// <param name="args">格式化参数</param>
        public static void Error(object message, LogContext context, params object[] args)
        {
            EnqueueMessage(LogLevel.Error, message, context, args);
        }

        /// <summary>
        /// 记录严重错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        public static void Fatal(object message, params object[] args)
        {
            EnqueueMessage(LogLevel.Fatal, message, Optional<LogContext>.None, args);
        }

        /// <summary>
        /// 记录严重错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="context">日志上下文</param>
        /// <param name="args">格式化参数</param>
        public static void Fatal(object message, LogContext context, params object[] args)
        {
            EnqueueMessage(LogLevel.Fatal, message, context, args);
        }

        /// <summary>
        /// 仅在TRACE模式下输出的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="args">格式化参数</param>
        [System.Diagnostics.Conditional("TRACE")]
        public static void Trace(object message, params object[] args)
        {
            EnqueueMessage(LogLevel.Trace, message, Optional<LogContext>.None, args);
        }

        /// <summary>
        /// 仅在TRACE模式下输出的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="context">日志上下文</param>
        /// <param name="args">格式化参数</param>
        [System.Diagnostics.Conditional("TRACE")]
        public static void Trace(object message, LogContext context, params object[] args)
        {
            EnqueueMessage(LogLevel.Trace, message, context, args);
        }

#if UNITY_EDITOR
        private static void OnEditorApplicationQuit()
        {
            DisposeOnUnityThreadInternal();
            DisposeInternal();
            UnityDebugHandler.Dispose();
        }
#else
        private static void OnApplicationQuit()
        {
            DisposeOnUnityThreadInternal();
            DisposeInternal();
            UnityDebugHandler.Dispose();
        }
#endif

        private static void DisposeOnUnityThreadInternal()
        {
            if (Disposed) return;
            FlushInternal();
            DisposeUnityThreadLogger(ref logger, ref flags, 1 << 0, 1 << 1);
            DisposeUnityThreadLogger(ref defaultLogger, ref flags, 1 << 2, 1 << 3);

            if (Proxy.Instance != null)
            {
                if (Application.isPlaying) GameObject.Destroy(Proxy.Instance.gameObject);
                else GameObject.DestroyImmediate(Proxy.Instance.gameObject);
            }
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
            if (Logger is ILoggerWithContext loggerWithContext)
                loggerWithContext.Log(logMessage.Level, logMessage.Message, logMessage.Context.HasValue ? logMessage.Context.Value : default, logMessage.Args);
            else
                Logger.Log(logMessage.Level, logMessage.Message, logMessage.Args);
        }

        private static void EnqueueMessage(LogLevel level, object message, Optional<LogContext> context, params object[] args)
        {
            ThrowErrorIfDisposed();
            queue.Enqueue(new LogMessage(level, GetString(message), context, args));
        }

        private static string GetString(object message)
        {
            if (message == null) return "Null";
            if (message is IFormattable formattable)
                return formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
            return message.ToString();
        }

        private static void ThrowErrorIfDisposed()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(LogUtility));
        }

        [AddComponentMenu("")]
        [DisallowMultipleComponent]
        private class Proxy : MonoBehaviour
        {
            public static Proxy Instance => instance;
            private static Proxy instance;
            private Coroutine coroutine;

            public void StartTimer(float interval)
            {
                if (coroutine != null) return;
                coroutine = StartCoroutine(PermanentCoroutineLoop(interval));
            }

            public void StopTimer()
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                coroutine = null;
            }

            private void Awake()
            {
                if (ReferenceEquals(instance, null)) instance = this;
                else Destroy(gameObject);
                if (ReferenceEquals(this, Instance))
                    DontDestroyOnLoad(gameObject);
            }

            private void OnDestroy()
            {
                if (ReferenceEquals(this, instance))
                    instance = null;
            }

            private IEnumerator PermanentCoroutineLoop(float interval)
            {
                var waitForSeconds = new WaitForSeconds(interval);
                while (true)
                {
                    LogUtility.Flush();
                    yield return waitForSeconds;
                }
            }
        }
    }
}
