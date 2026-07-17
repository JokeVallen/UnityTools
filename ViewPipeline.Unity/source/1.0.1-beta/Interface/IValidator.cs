namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 验证器接口
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// 执行验证
        /// </summary>
        /// <returns>验证结果</returns>
        ValidationResult Validate();
    }
}
