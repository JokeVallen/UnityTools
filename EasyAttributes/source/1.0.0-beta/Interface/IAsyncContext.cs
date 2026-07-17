using System.Threading;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 异步上下文
    /// </summary>
    /// <remarks>
    /// <para>提供异步操作所需的取消令牌。</para>
    /// <para>
    /// 实现此接口的上下文可在异步处理器中通过 <see cref="CancellationToken"/> 获取令牌，
    /// 以支持取消操作。
    /// </para>
    /// <para>
    /// 示例：
    /// <code>
    /// if (context is IAsyncContext asyncCtx)
    /// {
    ///     var token = asyncCtx.CancellationToken;
    ///     // 使用 token 执行异步操作
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public interface IAsyncContext
    {
        /// <summary>
        /// 异步操作取消令牌
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}
