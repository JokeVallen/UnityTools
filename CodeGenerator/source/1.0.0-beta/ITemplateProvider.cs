namespace CodeGenerator
{
    /// <summary>
    /// 模板提供者
    /// </summary>
    /// <typeparam name="TTemplate">模板内容的类型</typeparam>
    /// <remarks>
    /// <para>负责从模板路径读取模板内容。</para>
    /// </remarks>
    public interface ITemplateProvider<TTemplate>
    {
        /// <summary>
        /// 获取模板内容
        /// </summary>
        /// <param name="templatePath">模板路径</param>
        /// <returns>模板内容</returns>
        TTemplate GetTemplate(string templatePath);
    }
}