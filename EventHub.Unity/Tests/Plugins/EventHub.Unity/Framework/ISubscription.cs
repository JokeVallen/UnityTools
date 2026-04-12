using System;
using UnityEngine.Scripting;

namespace EventHub
{
    /// <summary>
    /// 事件订阅句柄接口
    /// </summary>
    /// <remarks>
    /// <para>实现了 IDisposable 接口，允许通过 using 语句自动管理资源释放。</para>
    /// <para>框架级接口采用版本接口，稳定版本的接口不会在后续版本出现更改，且新版本兼容旧版本。</para>
    /// </remarks>
    [Preserve]
    public interface ISubscription : IDisposable
    {
        /// <summary>
        /// 取消订阅
        /// </summary>
        void Unsubscribe();

        /// <summary>
        /// 句柄是否已释放
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 事件类型
        /// </summary>
        Type EventType { get; }

        /// <summary>
        /// 优先级
        /// </summary>
        int Priority { get; }
    }
}
