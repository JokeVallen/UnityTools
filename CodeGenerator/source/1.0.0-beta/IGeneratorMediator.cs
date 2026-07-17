namespace CodeGenerator
{
    /// <summary>
    /// 代码生成器中介者
    /// </summary>
    /// <typeparam name="TGenerator">代码生成器类型</typeparam>
    /// <remarks>
    /// <para>负责扫描和托管代码生成器并执行它们。</para>
    /// </remarks>
    public interface IGeneratorMediator<TGenerator> where TGenerator : IGenerator
    {
        /// <summary>
        /// 扫描代码生成器
        /// </summary>
        void Rescan();

        /// <summary>
        /// 清理代码生成器
        /// </summary>
        void Clear();

        /// <summary>
        /// 运行所有代码生成器
        /// </summary>
        void RunAll();

        /// <summary>
        /// 运行指定代码生成器
        /// </summary>
        /// <typeparam name="T">代码生成器类型</typeparam>
        void Run<T>() where T : TGenerator;
    }
}