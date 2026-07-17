#if !EVENTHUB_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// 可中断事件接口
    /// </summary>
    /// <remarks>
    /// <para>该接口作为事件分发器特定方法的中断检测途径，例如 <see cref="EventDispatcher.PublishInterruptableEvents{TEvent}(TEvent)"/>。</para>
    /// <para>该接口的语义是当处于特定事件发布方法执行过程中，如果某个尚未执行的事件要求中断自身及后续事件的执行，则可通过实现该接口来获得该功能。</para>
    /// </remarks>
    [Preserve]
    public interface IInterruptableEvent
    {
        /// <summary>
        /// 是否中断执行
        /// </summary>
        bool Interrupted { get; }
    }
}

#endif