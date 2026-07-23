namespace CodeGenerator
{
    /// <summary>
    /// 同步代码生成器接口
    /// </summary>
    /// <typeparam name="TTemplate">模板内容类型</typeparam>
    /// <typeparam name="TContent">输出内容类型</typeparam>
    public interface ISyncGenerator<TTemplate, TContent> : IGenerator
    {
        /// <summary>
        /// 生成代码
        /// </summary>
        /// <param name="template">模板内容</param>
        /// <returns>输出内容</returns>
        TContent Generate(TTemplate template);
    }
}
