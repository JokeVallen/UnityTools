using System;
using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 属性上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注属性的运行时信息。</para>
    /// </remarks>
    public interface IPropertyContext : IContext
    {
        /// <summary>目标对象</summary>
        object Target { get; }
        /// <summary>属性元数据</summary>
        PropertyInfo Property { get; }
        /// <summary>访问器</summary>
        PropertyAccessor Accessor { get; }
        /// <summary>属性值</summary>
        object Value { get; }
        /// <summary>原始Getter委托</summary>
        Func<object> GetOriginal { get; }
        /// <summary>原始Setter委托</summary>
        Action<object> SetOriginal { get; }
    }
}
