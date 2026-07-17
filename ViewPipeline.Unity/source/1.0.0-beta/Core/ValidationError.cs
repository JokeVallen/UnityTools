using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 验证错误信息
    /// </summary>
    public readonly struct ValidationError
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 严重等级
        /// </summary>
        public ValidationSeverity Severity { get; }

        /// <param name="message">消息</param>
        /// <param name="severity">严重等级</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> 不能为 null。</exception>
        public ValidationError(string message, ValidationSeverity severity)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Severity = severity;
        }

        /// <inheritdoc/>
        public override string ToString() => $"[{Severity}] {Message}";
    }
}
