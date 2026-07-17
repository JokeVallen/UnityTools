#if !EVENTHUB_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System.Threading;

namespace EventHub.Unity
{
    public static partial class EventDispatcher
    {
        /// <summary>
        /// 异步并行发布异步事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        public static async UniTask PublishParallelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            await GetParallelizable().PublishParallelAsync(@event, cancellationToken);
        }
    }
}

#endif