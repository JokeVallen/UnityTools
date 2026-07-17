namespace EasyAttributes.Core
{
    /// <summary>
    /// 事件处理器基类
    /// </summary>
    /// <remarks>
    /// <para>当上下文为 <see cref="IEventContext"/> 时调用子类重写的强类型方法，否则跳过。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class EventProcessor<TAttr> : Processor<TAttr> where TAttr : EasyAttribute
    {
        public sealed override void Before(IContext context, TAttr attribute)
        {
            if (context is IEventContext eventContext)
                Before(eventContext, attribute);
        }

        /// <summary>前置处理（可重写）</summary>
        protected virtual void Before(IEventContext context, TAttr attribute) { }

        public sealed override IProcessorHandle Process(IContext context, TAttr attribute)
        {
            if (context is IEventContext eventContext)
                return Process(eventContext, attribute);
            return ProcessorHandle.Continue;
        }

        /// <summary>核心处理（必须实现）</summary>
        protected abstract IProcessorHandle Process(IEventContext context, TAttr attribute);

        public sealed override void After(IContext context, TAttr attribute)
        {
            if (context is IEventContext eventContext)
                After(eventContext, attribute);
        }

        /// <summary>后置处理（可重写）</summary>
        protected virtual void After(IEventContext context, TAttr attribute) { }
    }
}