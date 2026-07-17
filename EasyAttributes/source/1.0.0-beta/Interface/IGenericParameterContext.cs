using System;
using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 泛型参数上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注泛型类型参数的运行时信息。</para>
    /// </remarks>
    public interface IGenericParameterContext : IContext
    {
        /// <summary>目标对象</summary>
        object Target { get; }
        /// <summary>泛型参数类型</summary>
        Type GenericParameter { get; }
        /// <summary>声明成员</summary>
        MemberInfo DeclaringMember { get; }
    }
}
