namespace CodeGenerator
{
    /// <summary>
    /// 同步代码生成器中介者的带上下文能力接口
    /// </summary>
    /// <typeparam name="TGenerator">代码生成器类型</typeparam>
    public interface IMediatorWithContext<TGenerator> where TGenerator : IGenerator
    {
        /// <summary>
        /// 运行所有代码生成器
        /// </summary>
        /// <param name="context">上下文</param>
        void RunAll(ITypedContext context);

        /// <summary>
        /// 运行指定代码生成器
        /// </summary>
        /// <typeparam name="T">代码生成器类型</typeparam>
        /// <param name="context">上下文</param>
        void Run<T>(ITypedContext context) where T : TGenerator;
    }
}
