namespace EasyAttributes.Core
{
    /// <summary>
    /// 字段处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IFieldContext"/> 时调用子类重写的强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class FieldProcessor<TAttr> : Processor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override void Before(IContext context, TAttr attribute)
        {
            if (context is IFieldContext fieldContext)
                Before(fieldContext, attribute);
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual void Before(IFieldContext context, TAttr attribute) { }

        public sealed override IProcessorHandle Process(IContext context, TAttr attribute)
        {
            if (context is IFieldContext fieldContext)
                return Process(fieldContext, attribute);
            return ProcessorHandle.Continue;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract IProcessorHandle Process(IFieldContext context, TAttr attribute);

        public sealed override void After(IContext context, TAttr attribute)
        {
            if (context is IFieldContext fieldContext)
                After(fieldContext, attribute);
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual void After(IFieldContext context, TAttr attribute) { }
    }
}