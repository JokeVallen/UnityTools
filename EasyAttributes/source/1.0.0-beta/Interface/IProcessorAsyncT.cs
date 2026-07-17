using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 泛型异步处理器
    /// </summary>
    /// <remarks>
    /// <para>定义特定属性类型的异步处理器契约。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public interface IProcessorAsync<TAttr> : IProcessorAsync where TAttr : EasyAttribute
    {
        /// <summary>异步前置处理</summary>
        Task BeforeAsync(IContext context, TAttr attribute);
        /// <summary>异步核心处理</summary>
        Task<IProcessorHandle> ProcessAsync(IContext context, TAttr attribute);
        /// <summary>异步后置处理</summary>
        Task AfterAsync(IContext context, TAttr attribute);
    }
}
