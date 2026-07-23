using System;

namespace CodeGenerator
{
    /// <summary>
    /// 代码生成器 Attribute
    /// </summary>
    /// <remarks>
    /// <para>实现类标注该 Attribute，提供自动扫描和实例化支持。</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratorAttribute : Attribute { }
}
