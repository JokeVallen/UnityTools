using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 异步返回值处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IReturnValueContext"/> 时调用子类重写的异步强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class AsyncReturnValueProcessor<TAttr> : AsyncProcessor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override Task BeforeAsync(IContext context, TAttr attribute)
        {
            if (context is IReturnValueContext returnValueContext)
                return BeforeAsync(returnValueContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual Task BeforeAsync(IReturnValueContext context, TAttr attribute) => Task.CompletedTask;

        public sealed override Task<IProcessorHandle> ProcessAsync(IContext context, TAttr attribute)
        {
            if (context is IReturnValueContext returnValueContext)
                return ProcessAsync(returnValueContext, attribute);
            return ContextDefaults.ContinueTask;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract Task<IProcessorHandle> ProcessAsync(IReturnValueContext context, TAttr attribute);

        public sealed override Task AfterAsync(IContext context, TAttr attribute)
        {
            if (context is IReturnValueContext returnValueContext)
                return AfterAsync(returnValueContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual Task AfterAsync(IReturnValueContext context, TAttr attribute) => Task.CompletedTask;
    }
}