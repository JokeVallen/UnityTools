using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 异步方法处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IMethodContext"/> 时调用子类重写的异步强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class AsyncMethodProcessor<TAttr> : AsyncProcessor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override Task BeforeAsync(IContext context, TAttr attribute)
        {
            if (context is IMethodContext methodContext)
                return BeforeAsync(methodContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual Task BeforeAsync(IMethodContext context, TAttr attribute) => Task.CompletedTask;

        public sealed override Task<IProcessorHandle> ProcessAsync(IContext context, TAttr attribute)
        {
            if (context is IMethodContext methodContext)
                return ProcessAsync(methodContext, attribute);
            return ContextDefaults.ContinueTask;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract Task<IProcessorHandle> ProcessAsync(IMethodContext context, TAttr attribute);

        public sealed override Task AfterAsync(IContext context, TAttr attribute)
        {
            if (context is IMethodContext methodContext)
                return AfterAsync(methodContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual Task AfterAsync(IMethodContext context, TAttr attribute) => Task.CompletedTask;
    }
}