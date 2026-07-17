using System.Threading;
using System.Threading.Tasks;

namespace CodeGenerator
{
    /// <summary>
    /// 代码生成器中介者异步接口
    /// </summary>
    /// <typeparam name="TGenerator">代码生成器类型</typeparam>
    /// <remarks>
    /// <para>负责扫描和托管代码生成器并执行它们。</para>
    /// </remarks>
    public interface IGeneratorMediatorAsync<TGenerator> where TGenerator : IGenerator
    {
        /// <summary>
        /// 异步扫描代码生成器
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task RescanAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步清理代码生成器
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步运行所有代码生成器
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task RunAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步运行指定代码生成器
        /// </summary>
        /// <typeparam name="T">代码生成器类型</typeparam>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task RunAsync<T>(CancellationToken cancellationToken = default) where T : TGenerator;
    }
}