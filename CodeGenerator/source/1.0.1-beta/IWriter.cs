namespace CodeGenerator
{
    /// <summary>
    /// 写入器
    /// </summary>
    /// <typeparam name="TContent">待写入内容的类型</typeparam>
    /// <remarks>
    /// <para>负责将内容写入输出路径。</para>
    /// </remarks>
    public interface IWriter<TContent>
    {
        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="outputPath">输出路径</param>
        /// <param name="content">待写入内容</param>
        void Write(string outputPath, TContent content);
    }
}