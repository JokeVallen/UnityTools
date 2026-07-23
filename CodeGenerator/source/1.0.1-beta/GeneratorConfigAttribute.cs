using System;

namespace CodeGenerator
{
    /// <summary>
    /// 代码生成器配置 Attribute
    /// </summary>
    /// <remarks>
    /// <para>将实现类标注为代码生成器，并声明其模板路径与输出路径。</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GeneratorConfigAttribute : Attribute
    {
        /// <summary>
        /// 模板路径
        /// </summary>
        public string TemplatePath { get; }

        /// <summary>
        /// 输出路径
        /// </summary>
        public string OutputPath { get; }

        /// <param name="templatePath">模板路径</param>
        /// <param name="outputPath">输出路径</param>
        public GeneratorConfigAttribute(string templatePath, string outputPath)
        {
            TemplatePath = templatePath;
            OutputPath = outputPath;
        }
    }
}
