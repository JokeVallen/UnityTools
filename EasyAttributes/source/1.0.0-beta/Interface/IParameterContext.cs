using System.Collections.Generic;
using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 参数上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注方法参数的运行时信息。</para>
    /// </remarks>
    public interface IParameterContext : IContext
    {
        /// <summary>目标对象</summary>
        object Target { get; }
        /// <summary>方法元数据</summary>
        MethodInfo Method { get; }
        /// <summary>参数元数据</summary>
        ParameterInfo Parameter { get; }
        /// <summary>参数索引</summary>
        int ParameterIndex { get; }
        /// <summary>参数值</summary>
        object Value { get; }
        /// <summary>所有参数值</summary>
        IReadOnlyList<object> Arguments { get; }
    }
}
