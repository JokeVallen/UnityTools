using System;
using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 字段上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注字段的运行时信息。</para>
    /// </remarks>
    public interface IFieldContext : IContext
    {
        /// <summary>目标对象</summary>
        object Target { get; }
        /// <summary>字段元数据</summary>
        FieldInfo Field { get; }
        /// <summary>字段值</summary>
        object Value { get; }
        /// <summary>原始Getter委托</summary>
        Func<object> GetOriginal { get; }
        /// <summary>原始Setter委托</summary>
        Action<object> SetOriginal { get; }
    }
}
