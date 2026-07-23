using System.Threading;
using System.Threading.Tasks;

namespace CodeGenerator
{
    /// <summary>
    /// 带上下文的异步代码生成器接口
    /// </summary>
    /// <typeparam name="TTemplate">模板内容类型</typeparam>
    /// <typeparam name="TContent">输出内容类型</typeparam>
    public interface IAsyncGeneratorWithContext<TTemplate, TContent> : IGenerator
    {
        /// <summary>
        /// 异步生成代码
        /// </summary>
        /// <param name="template">模板内容</param>
        /// <param name="context">上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>附带输出内容的异步任务实例</returns>
        Task<TContent> GenerateAsync(TTemplate template, ITypedContext context, CancellationToken cancellationToken = default);
    }
}
