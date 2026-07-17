using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 方法上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注方法的完整运行时信息。</para>
    /// <para>
    /// 处理器可通过 <see cref="Proceed"/> 委托调用原始方法，
    /// 通过 <see cref="Result"/> 和 <see cref="Exception"/> 获取执行后的返回值或异常。
    /// </para>
    /// </remarks>
    public interface IMethodContext : IContext
    {
        /// <summary>目标对象</summary>
        object Target { get; }
        /// <summary>方法元数据</summary>
        MethodInfo Method { get; }
        /// <summary>参数列表</summary>
        IReadOnlyList<ParameterInfo> Parameters { get; }
        /// <summary>参数值</summary>
        object[] Arguments { get; }
        /// <summary>原始方法调用委托</summary>
        Func<object> Proceed { get; }
        /// <summary>返回值</summary>
        object Result { get; }
        /// <summary>异常</summary>
        Exception Exception { get; }
    }
}
