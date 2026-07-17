namespace EasyAttributes
{
    /// <summary>
    /// 执行器接口
    /// </summary>
    public interface IExecutor
    {
        /// <summary>
        /// 执行处理器链
        /// </summary>
        /// <param name="context">上下文实例</param>
        /// <returns>处理器执行句柄</returns>
        IProcessorHandle Execute(IContext context);
    }
}
