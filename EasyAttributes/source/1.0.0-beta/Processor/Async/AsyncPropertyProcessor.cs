using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 异步属性处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IPropertyContext"/> 时调用子类重写的异步强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class AsyncPropertyProcessor<TAttr> : AsyncProcessor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override Task BeforeAsync(IContext context, TAttr attribute)
        {
            if (context is IPropertyContext propertyContext)
                return BeforeAsync(propertyContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual Task BeforeAsync(IPropertyContext context, TAttr attribute) => Task.CompletedTask;

        public sealed override Task<IProcessorHandle> ProcessAsync(IContext context, TAttr attribute)
        {
            if (context is IPropertyContext propertyContext)
                return ProcessAsync(propertyContext, attribute);
            return ContextDefaults.ContinueTask;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract Task<IProcessorHandle> ProcessAsync(IPropertyContext context, TAttr attribute);

        public sealed override Task AfterAsync(IContext context, TAttr attribute)
        {
            if (context is IPropertyContext propertyContext)
                return AfterAsync(propertyContext, attribute);
            return Task.CompletedTask;
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual Task AfterAsync(IPropertyContext context, TAttr attribute) => Task.CompletedTask;
    }
}