using System;
using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 事件上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注事件的运行时信息。</para>
    /// </remarks>
    public interface IEventContext : IContext
    {
        /// <summary>目标对象</summary>
        object Target { get; }
        /// <summary>事件元数据</summary>
        EventInfo Event { get; }
        /// <summary>访问器</summary>
        EventAccessor Accessor { get; }
        /// <summary>处理程序</summary>
        Delegate Handler { get; }
        /// <summary>原始Add委托</summary>
        Action<Delegate> AddOriginal { get; }
        /// <summary>原始Remove委托</summary>
        Action<Delegate> RemoveOriginal { get; }
    }
}
