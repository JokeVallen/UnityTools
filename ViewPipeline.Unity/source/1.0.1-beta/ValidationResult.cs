namespace ViewPipeline.Unity.Core
{
    /// <summary>验证结果</summary>
    public readonly struct ValidationResult
    {
        /// <summary>
        /// 是否通过验证
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 严重等级
        /// </summary>
        public ValidationSeverity Severity { get; }

        private ValidationResult(bool isValid, string message, ValidationSeverity severity) 
        { 
            IsValid = isValid;
            Message = message;
            Severity = severity;
        }

        /// <summary>
        /// 成功通过验证
        /// </summary>
        /// <returns>验证结果</returns>
        public static ValidationResult Success() => new ValidationResult(true, null, ValidationSeverity.Info);

        /// <summary>
        /// 由于严重错误导致验证不通过
        /// </summary>
        /// <param name="msg">错误信息</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Error(string msg) => new ValidationResult(false, msg, ValidationSeverity.Error);

        /// <summary>
        /// 验证通过但存在警告信息
        /// </summary>
        /// <param name="msg">警告信息</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Warning(string msg) => new ValidationResult(true, msg, ValidationSeverity.Warning);
    }
}
