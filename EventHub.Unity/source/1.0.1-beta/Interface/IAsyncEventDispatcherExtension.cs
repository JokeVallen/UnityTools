#if !EVENTHUB_EXTENSION_ENABLE

namespace EventHub.Unity
{
    /// <summary>
    /// 异步事件分发器扩展接口
    /// </summary>
    public interface IAsyncEventDispatcherExtension
    {
        /// <summary>
        /// 取消订阅指定异步事件类型的所有事件
        /// </summary>
        /// <typeparam name="TEvent">异步事件类型</typeparam>
        /// <returns>取消订阅的事件数量</returns>
        int UnsubscribeAsyncEvents<TEvent>();

        /// <summary>
        /// 取消订阅所有异步事件
        /// </summary>
        /// <returns>取消订阅的事件数量</returns>
        int UnsubscribeAll();
    }
}

#endif