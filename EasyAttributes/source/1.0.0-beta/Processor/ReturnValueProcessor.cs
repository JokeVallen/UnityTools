namespace EasyAttributes.Core
{
    /// <summary>
    /// 返回值处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IReturnValueContext"/> 时调用子类重写的强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class ReturnValueProcessor<TAttr> : Processor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override void Before(IContext context, TAttr attribute)
        {
            if (context is IReturnValueContext returnValueContext)
                Before(returnValueContext, attribute);
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual void Before(IReturnValueContext context, TAttr attribute) { }

        public sealed override IProcessorHandle Process(IContext context, TAttr attribute)
        {
            if (context is IReturnValueContext returnValueContext)
                return Process(returnValueContext, attribute);
            return ProcessorHandle.Continue;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract IProcessorHandle Process(IReturnValueContext context, TAttr attribute);

        public sealed override void After(IContext context, TAttr attribute)
        {
            if (context is IReturnValueContext returnValueContext)
                After(returnValueContext, attribute);
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual void After(IReturnValueContext context, TAttr attribute) { }
    }
}