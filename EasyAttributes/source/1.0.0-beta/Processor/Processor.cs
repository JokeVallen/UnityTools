namespace EasyAttributes.Core
{
    /// <summary>
    /// 同步处理器基类
    /// </summary>
    /// <remarks>
    /// <para>继承此类并实现 <see cref="Process"/> 即可定义特定属性类型的同步处理逻辑。</para>
    /// <para>
    /// 桥接层自动从 <see cref="IContext.Attribute"/> 提取强类型属性，类型不匹配时静默跳过。
    /// </para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public abstract class Processor<TAttr> : IProcessor<TAttr> where TAttr : EasyAttribute
    {
        /// <summary>前置处理（可重写）</summary>
        public virtual void Before(IContext context, TAttr attribute) { }
        /// <summary>核心处理（必须实现）</summary>
        public abstract IProcessorHandle Process(IContext context, TAttr attribute);
        /// <summary>后置处理（可重写）</summary>
        public virtual void After(IContext context, TAttr attribute) { }

        void IProcessor.Before(IContext context)
        {
            if (context.Attribute is TAttr attr)
                Before(context, attr);
        }

        IProcessorHandle IProcessor.Process(IContext context)
        {
            if (context.Attribute is TAttr attr)
                return Process(context, attr);
            return ProcessorHandle.Continue;
        }

        void IProcessor.After(IContext context)
        {
            if (context.Attribute is TAttr attr)
                After(context, attr);
        }
    }
}
