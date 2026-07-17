using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace EasyLogger.Unity
{
    /// <summary>
    /// 文件日志记录器
    /// </summary>
    /// <remarks>
    /// <para>非线程安全</para>
    /// </remarks>
    public sealed class FileLogger : LoggerBase, IDisposable, IUnityThreadLogger
    {
        private readonly FileLoggerConfig config;
        private readonly StringBuilder buffer = new StringBuilder();
        private StreamWriter writer;
        private string currentFilePath;
        private long bufferByteSize;
        private Coroutine flushCoroutine;
        private bool Disposed => (flags & 1 << 0) != 0 && (flags & 1 << 1) != 0;
        private int flags;

        /// <param name="config">配置实例</param>
        public FileLogger(FileLoggerConfig config) : base(config?.Config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            RotateLogFile();
            TryStartFlushTimerCoroutine();
        }

        /// <summary>
        /// 刷新日志文件
        /// </summary>
        /// <remarks>
        /// 立即将缓冲区内容写入文件
        /// </remarks>
        public void Flush()
        {
            if (Disposed)
            {
                HandleError(new ObjectDisposedException(nameof(FileLogger)));
                return;
            }

            FlushBufferInternal();
            try
            {
                writer?.Flush();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        public void DisposeOnUnityThread()
        {
            if (Disposed) return;
            DisposeOnUnityThreadInternal();
        }

        private void RotateLogFile()
        {
            try
            {
                if (!Directory.Exists(config.LogDirectory))
                    Directory.CreateDirectory(config.LogDirectory);

                if (writer != null)
                {
                    FlushBufferInternal();
                    writer.Flush();
                    writer.Dispose();
                    writer = null;
                }

                string dateStr = DateTime.Now.ToString("yyyyMMdd");
                currentFilePath = Path.Combine(config.LogDirectory, $"{config.FileNamePrefix}_{dateStr}.log");

                if (File.Exists(currentFilePath))
                {
                    var fileInfo = new FileInfo(currentFilePath);
                    if (fileInfo.Length >= config.MaxFileSizeBytes)
                    {
                        string backupPath = Path.Combine(config.LogDirectory, $"{config.FileNamePrefix}_{dateStr}_{DateTime.Now:HHmmss}.log");
                        int counter = 1;
                        while (File.Exists(backupPath))
                        {
                            backupPath = Path.Combine(config.LogDirectory, $"{config.FileNamePrefix}_{dateStr}_{DateTime.Now:HHmmss}_{counter}.log");
                            counter++;
                        }
                        File.Move(currentFilePath, backupPath);
                        CleanupOldBackups();
                    }
                }

                writer = new StreamWriter(currentFilePath, true, Encoding.UTF8);
                if (config.AutoFlush && !config.UseAsync) writer.AutoFlush = true;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(config.LogDirectory, $"{config.FileNamePrefix}_*.log");
                if (backupFiles.Length > config.MaxBackupFiles)
                {
                    Array.Sort(backupFiles);
                    for (int i = 0; i < backupFiles.Length - config.MaxBackupFiles; i++)
                    {
                        try
                        {
                            File.Delete(backupFiles[i]);
                        }
                        catch (Exception ex)
                        {
                            HandleError(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        /// <inheritdoc/>
        protected override void DoLog(LogLevel level, string message, params object[] args)
        {
            if (Disposed)
            {
                HandleError(new ObjectDisposedException(nameof(FileLogger)));
                return;
            }

            string logMessage = FormatMessageByFormatProvider(message, args);
            logMessage = FormatMessageByFormatter(level, logMessage);

            try
            {
                if (config.UseAsync)
                {
                    TryStartFlushTimerCoroutine();
                    bufferByteSize += Encoding.UTF8.GetByteCount(logMessage + Environment.NewLine);
                    buffer.AppendLine(logMessage);
                    if (buffer.Length > config.MaxFlushBufferSizeBytes)
                        FlushBufferInternal();
                }
                else
                {
                    if (writer == null)
                    {
                        bufferByteSize += Encoding.UTF8.GetByteCount(logMessage + Environment.NewLine);
                        buffer.AppendLine(logMessage);
                        return;
                    }

                    FlushBufferInternal();
                    writer.WriteLine(logMessage);
                }

                if (writer != null)
                {
                    long currentLength = writer.BaseStream.Length + (config.UseAsync ? bufferByteSize : 0);
                    if (currentLength >= config.MaxFileSizeBytes) RotateLogFile();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private IEnumerator OnTiming(int intervalMilliseconds)
        {
            var wait = intervalMilliseconds > 0 ? new WaitForSeconds(intervalMilliseconds / 1000f) : null;
            while (!Disposed)
            {
                yield return wait;
                FlushBufferInternal();
            }
        }

        private void FlushBufferInternal()
        {
            try
            {
                if (writer != null && buffer.Length > 0)
                {
                    writer.Write(buffer.ToString());
                    buffer.Clear();
                    bufferByteSize = 0;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void TryStartFlushTimerCoroutine()
        {
            if (!config.UseAsync) return;
            if (flushCoroutine != null) return;
            if (LogDriver.Instance == null) return;
            flushCoroutine = LogDriver.Instance.StartCoroutine(OnTiming(config.FlushIntervalMilliseconds));
        }

        private void TryStopFlushTimerCoroutine()
        {
            if (flushCoroutine == null) return;
            if (LogDriver.Instance == null) return;
            LogDriver.Instance.StopCoroutine(flushCoroutine);
            flushCoroutine = null;
        }

        private void DisposeInternal()
        {
            FlushBufferInternal();
            writer?.Flush();
            writer?.Dispose();
            writer = null;
            flags |= 1 << 0;
        }

        private void DisposeOnUnityThreadInternal()
        {
            TryStopFlushTimerCoroutine();
            flags |= 1 << 1;
        }
    }
}