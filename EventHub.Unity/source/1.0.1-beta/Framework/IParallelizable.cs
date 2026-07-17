using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub
{
    /// <summary>
    /// 事件并发发布接口
    /// </summary>
    [Preserve]
    public interface IParallelizable
    {
        /// <summary>
        /// 异步并发发布异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        /// <param name="cancellationToken">取消令牌</param>
        UniTask PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
    }
}