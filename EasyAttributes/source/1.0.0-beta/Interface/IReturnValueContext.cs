using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 返回值上下文
    /// </summary>
    /// <remarks>
    /// <para>提供被标注方法返回值的运行时信息。</para>
    /// </remarks>
    public interface IReturnValueContext : IContext
    {
        /// <summary>目标对象</summary>
        object Target { get; }
        /// <summary>方法元数据</summary>
        MethodInfo Method { get; }
        /// <summary>返回参数元数据</summary>
        ParameterInfo ReturnParameter { get; }
        /// <summary>返回值</summary>
        object Result { get; }
    }
}
