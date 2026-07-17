using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 构造函数上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注构造函数的运行时信息。</para>
    /// </remarks>
    public interface IConstructorContext : IContext
    {
        /// <summary>目标对象</summary>
        object Target { get; }
        /// <summary>构造函数元数据</summary>
        ConstructorInfo Constructor { get; }
        /// <summary>参数列表</summary>
        IReadOnlyList<ParameterInfo> Parameters { get; }
        /// <summary>参数值</summary>
        object[] Arguments { get; }
        /// <summary>原始调用委托</summary>
        Func<object> Proceed { get; }
        /// <summary>异常</summary>
        Exception Exception { get; }
    }
}
