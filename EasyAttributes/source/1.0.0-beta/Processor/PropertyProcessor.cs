namespace EasyAttributes.Core
{
    /// <summary>
    /// 属性处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IPropertyContext"/> 时调用子类重写的强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class PropertyProcessor<TAttr> : Processor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override void Before(IContext context, TAttr attribute)
        {
            if (context is IPropertyContext propertyContext)
                Before(propertyContext, attribute);
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual void Before(IPropertyContext context, TAttr attribute) { }

        public sealed override IProcessorHandle Process(IContext context, TAttr attribute)
        {
            if (context is IPropertyContext propertyContext)
                return Process(propertyContext, attribute);
            return ProcessorHandle.Continue;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract IProcessorHandle Process(IPropertyContext context, TAttr attribute);

        public sealed override void After(IContext context, TAttr attribute)
        {
            if (context is IPropertyContext propertyContext)
                After(propertyContext, attribute);
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual void After(IPropertyContext context, TAttr attribute) { }
    }
}