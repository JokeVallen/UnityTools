using System.Threading;
using System.Threading.Tasks;

namespace CodeGenerator
{
    /// <summary>
    /// 异步代码生成器中介者的带上下文能力接口
    /// </summary>
    /// <typeparam name="TGenerator">代码生成器类型</typeparam>
    public interface IAsyncMediatorWithContext<TGenerator> where TGenerator : IGenerator
    {
        /// <summary>
        /// 异步运行所有代码生成器
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task RunAllAsync(ITypedContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步运行指定代码生成器
        /// </summary>
        /// <typeparam name="T">代码生成器类型</typeparam>
        /// <param name="context">上下文</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task RunAsync<T>(ITypedContext context, CancellationToken cancellationToken = default) where T : TGenerator;
    }
}
