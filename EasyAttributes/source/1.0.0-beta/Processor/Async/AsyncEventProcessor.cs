using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 异步事件处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IEventContext"/> 时调用子类重写的异步强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class AsyncEventProcessor<TAttr> : AsyncProcessor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override Task BeforeAsync(IContext context, TAttr attribute)
        {
            if (context is IEventContext eventContext)
                return BeforeAsync(eventContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual Task BeforeAsync(IEventContext context, TAttr attribute) => Task.CompletedTask;

        public sealed override Task<IProcessorHandle> ProcessAsync(IContext context, TAttr attribute)
        {
            if (context is IEventContext eventContext)
                return ProcessAsync(eventContext, attribute);
            return ContextDefaults.ContinueTask;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract Task<IProcessorHandle> ProcessAsync(IEventContext context, TAttr attribute);

        public sealed override Task AfterAsync(IContext context, TAttr attribute)
        {
            if (context is IEventContext eventContext)
                return AfterAsync(eventContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual Task AfterAsync(IEventContext context, TAttr attribute) => Task.CompletedTask;
    }
}