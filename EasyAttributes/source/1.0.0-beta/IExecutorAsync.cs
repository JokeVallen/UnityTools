using System.Threading;
using System.Threading.Tasks;

namespace EasyAttributes
{
    /// <summary>
    /// 执行器异步接口
    /// </summary>
    public interface IExecutorAsync
    {
        /// <summary>
        /// 异步执行处理器链
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="cancellationToken">用于取消异步操作的令牌</param>
        /// <returns>附带处理器句柄的异步任务实例</returns>
        Task<IProcessorHandle> ExecuteAsync(IContext context, CancellationToken cancellationToken = default);
    }
}
