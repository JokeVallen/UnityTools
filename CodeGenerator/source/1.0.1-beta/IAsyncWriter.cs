using System.Threading;
using System.Threading.Tasks;

namespace CodeGenerator
{
    /// <summary>
    /// 异步写入器接口
    /// </summary>
    /// <typeparam name="TContent">内容类型</typeparam>
    /// <remarks>
    /// <para>负责将内容写入输出路径。</para>
    /// </remarks>
    public interface IAsyncWriter<TContent>
    {
        /// <summary>
        /// 异步写入内容
        /// </summary>
        /// <param name="outputPath">输出路径</param>
        /// <param name="content">内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task WriteAsync(string outputPath, TContent content, CancellationToken cancellationToken = default);
    }
}