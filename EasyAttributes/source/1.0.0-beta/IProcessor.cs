namespace EasyAttributes
{
    /// <summary>
    /// 属性处理器接口
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// 在 <see cref="Process"/> 之前执行的前置回调
        /// </summary>
        /// <param name="context">当前执行上下文</param>
        void Before(IContext context);

        /// <summary>
        /// 执行核心处理逻辑
        /// </summary>
        /// <param name="context">当前执行上下文</param>
        /// <returns>处理器句柄</returns>
        IProcessorHandle Process(IContext context);

        /// <summary>
        /// 在 <see cref="Process"/> 之后执行的后置回调
        /// </summary>
        /// <param name="context">当前执行上下文</param>
        void After(IContext context);
    }
}
