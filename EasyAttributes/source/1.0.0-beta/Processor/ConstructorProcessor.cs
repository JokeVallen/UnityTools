namespace EasyAttributes.Core
{
    /// <summary>
    /// 构造函数处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IConstructorContext"/> 时调用子类重写的强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class ConstructorProcessor<TAttr> : Processor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override void Before(IContext context, TAttr attribute)
        {
            if (context is IConstructorContext constructorContext)
                Before(constructorContext, attribute);
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual void Before(IConstructorContext context, TAttr attribute) { }

        public sealed override IProcessorHandle Process(IContext context, TAttr attribute)
        {
            if (context is IConstructorContext constructorContext)
                return Process(constructorContext, attribute);
            return ProcessorHandle.Continue;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract IProcessorHandle Process(IConstructorContext context, TAttr attribute);

        public sealed override void After(IContext context, TAttr attribute)
        {
            if (context is IConstructorContext constructorContext)
                After(constructorContext, attribute);
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual void After(IConstructorContext context, TAttr attribute) { }
    }
}