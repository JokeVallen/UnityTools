using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 异步处理器基类
    /// </summary>
    /// <remarks>
    /// <para>继承此类并实现 <see cref="ProcessAsync"/> 即可定义特定属性类型的异步处理逻辑。</para>
    /// <para>
    /// 桥接层自动从 <see cref="IContext.Attribute"/> 提取强类型属性，类型不匹配时静默跳过。
    /// </para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class AsyncProcessor<TAttr> : IProcessorAsync<TAttr> where TAttr : EasyAttribute
    {
        /// <summary>异步前置处理（可重写）</summary>
        public virtual Task BeforeAsync(IContext context, TAttr attribute) => Task.CompletedTask;
        /// <summary>异步核心处理（必须实现）</summary>
        public abstract Task<IProcessorHandle> ProcessAsync(IContext context, TAttr attribute);
        /// <summary>异步后置处理（可重写）</summary>
        public virtual Task AfterAsync(IContext context, TAttr attribute) => Task.CompletedTask;

        Task IProcessorAsync.BeforeAsync(IContext context)
        {
            if (context.Attribute is TAttr attr)
                return BeforeAsync(context, attr);
            return Task.CompletedTask;
        }

        Task<IProcessorHandle> IProcessorAsync.ProcessAsync(IContext context)
        {
            if (context.Attribute is TAttr attr)
                return ProcessAsync(context, attr);
            return ContextDefaults.ContinueTask;
        }

        Task IProcessorAsync.AfterAsync(IContext context)
        {
            if (context.Attribute is TAttr attr)
                return AfterAsync(context, attr);
            return Task.CompletedTask;
        }
    }
}
