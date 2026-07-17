using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 类型上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注类型的运行时信息。</para>
    /// </remarks>
    public interface ITypeContext : IContext
    {
        /// <summary>目标对象（始终为null）</summary>
        object Target { get; }
        /// <summary>类型</summary>
        Type Type { get; }
    }
}
