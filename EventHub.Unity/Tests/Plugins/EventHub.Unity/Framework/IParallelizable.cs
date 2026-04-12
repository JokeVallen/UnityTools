using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Scripting;

namespace EventHub
{
    /// <summary>
    /// 事件并行发布接口
    /// </summary>
    /// <remarks>
    /// <para>框架级接口采用版本接口，稳定版本的接口不会在后续版本出现更改，且新版本兼容旧版本。</para>
    /// </remarks>
    [Preserve]
    public interface IParallelizable
    {
        /// <summary>
        /// 异步并行发布异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        /// <param name="cancellationToken">取消令牌</param>
        UniTask PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
    }
}