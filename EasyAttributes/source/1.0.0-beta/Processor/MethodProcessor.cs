namespace EasyAttributes.Core
{
    /// <summary>
    /// 方法处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IMethodContext"/> 时调用子类重写的强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class MethodProcessor<TAttr> : Processor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override void Before(IContext context, TAttr attribute)
        {
            if (context is IMethodContext methodContext)
                Before(methodContext, attribute);
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual void Before(IMethodContext context, TAttr attribute) { }

        public sealed override IProcessorHandle Process(IContext context, TAttr attribute)
        {
            if (context is IMethodContext methodContext)
                return Process(methodContext, attribute);
            return ProcessorHandle.Continue;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract IProcessorHandle Process(IMethodContext context, TAttr attribute);

        public sealed override void After(IContext context, TAttr attribute)
        {
            if (context is IMethodContext methodContext)
                After(methodContext, attribute);
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual void After(IMethodContext context, TAttr attribute) { }
    }
}