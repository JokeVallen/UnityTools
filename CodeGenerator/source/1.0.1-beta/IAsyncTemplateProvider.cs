using System.Threading;
using System.Threading.Tasks;

namespace CodeGenerator
{
    /// <summary>
    /// 异步模板提供者接口
    /// </summary>
    /// <typeparam name="TTemplate">模板内容的类型</typeparam>
    /// <remarks>
    /// <para>负责从模板路径读取模板内容。</para>
    /// </remarks>
    public interface IAsyncTemplateProvider<TTemplate>
    {
        /// <summary>
        /// 异步获取模板内容
        /// </summary>
        /// <param name="templatePath">模板路径</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>模板内容</returns>
        Task<TTemplate> GetTemplateAsync(string templatePath, CancellationToken cancellationToken = default);
    }
}