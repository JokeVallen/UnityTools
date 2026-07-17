using System.Threading.Tasks;

namespace EasyAttributes
{
    /// <summary>
    /// 属性处理器异步接口
    /// </summary>
    public interface IProcessorAsync
    {
        /// <summary>
        /// 在 <see cref="ProcessAsync"/> 之前执行的异步前置回调
        /// </summary>
        /// <param name="context">当前执行上下文</param>
        Task BeforeAsync(IContext context);

        /// <summary>
        /// 异步执行核心处理逻辑
        /// </summary>
        /// <param name="context">当前执行上下文</param>
        /// <returns>处理器句柄</returns>
        Task<IProcessorHandle> ProcessAsync(IContext context);

        /// <summary>
        /// 在 <see cref="ProcessAsync"/> 之后执行的异步后置回调
        /// </summary>
        /// <param name="context">当前执行上下文</param>
        Task AfterAsync(IContext context);
    }
}
