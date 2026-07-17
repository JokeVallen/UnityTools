using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 异步类型处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="ITypeContext"/> 时调用子类重写的异步强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class AsyncTypeProcessor<TAttr> : AsyncProcessor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override Task BeforeAsync(IContext context, TAttr attribute)
        {
            if (context is ITypeContext typeContext)
                return BeforeAsync(typeContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual Task BeforeAsync(ITypeContext context, TAttr attribute) => Task.CompletedTask;

        public sealed override Task<IProcessorHandle> ProcessAsync(IContext context, TAttr attribute)
        {
            if (context is ITypeContext typeContext)
                return ProcessAsync(typeContext, attribute);
            return ContextDefaults.ContinueTask;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract Task<IProcessorHandle> ProcessAsync(ITypeContext context, TAttr attribute);

        public sealed override Task AfterAsync(IContext context, TAttr attribute)
        {
            if (context is ITypeContext typeContext)
                return AfterAsync(typeContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual Task AfterAsync(ITypeContext context, TAttr attribute) => Task.CompletedTask;
    }
}