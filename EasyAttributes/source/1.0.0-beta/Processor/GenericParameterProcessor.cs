namespace EasyAttributes.Core
{
    /// <summary>
    /// 泛型参数处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IGenericParameterContext"/> 时调用子类重写的强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class GenericParameterProcessor<TAttr> : Processor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override void Before(IContext context, TAttr attribute)
        {
            if (context is IGenericParameterContext genericParameterContext)
                Before(genericParameterContext, attribute);
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual void Before(IGenericParameterContext context, TAttr attribute) { }

        public sealed override IProcessorHandle Process(IContext context, TAttr attribute)
        {
            if (context is IGenericParameterContext genericParameterContext)
                return Process(genericParameterContext, attribute);
            return ProcessorHandle.Continue;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract IProcessorHandle Process(IGenericParameterContext context, TAttr attribute);

        public sealed override void After(IContext context, TAttr attribute)
        {
            if (context is IGenericParameterContext genericParameterContext)
                After(genericParameterContext, attribute);
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual void After(IGenericParameterContext context, TAttr attribute) { }
    }
}