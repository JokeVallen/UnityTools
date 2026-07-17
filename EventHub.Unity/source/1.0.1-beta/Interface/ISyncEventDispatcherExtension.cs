#if !EVENTHUB_EXTENSION_ENABLE

namespace EventHub.Unity
{
    /// <summary>
    /// 同步事件分发器扩展接口
    /// </summary>
    public interface ISyncEventDispatcherExtension
    {
        /// <summary>
        /// 取消订阅指定同步事件类型的所有事件
        /// </summary>
        /// <typeparam name="TEvent">同步事件类型</typeparam>
        /// <returns>取消订阅的事件数量</returns>
        int UnsubscribeSyncEvents<TEvent>();

        /// <summary>
        /// 取消订阅所有同步事件
        /// </summary>
        /// <returns>取消订阅的事件数量</returns>
        int UnsubscribeAll();
    }
}

#endif