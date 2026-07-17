namespace ViewPipeline.Unity.Core
{
    /// <summary>可验证接口</summary>
    public interface IValidatable
    {
        /// <summary>
        /// 获取验证器
        /// </summary>
        /// <returns>验证器</returns>
        IValidator GetValidator();
    }
}
