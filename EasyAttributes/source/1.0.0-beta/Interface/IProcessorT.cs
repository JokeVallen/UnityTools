namespace EasyAttributes.Core
{
    /// <summary>
    /// 泛型同步处理器
    /// </summary>
    /// <remarks>
    /// <para>定义特定属性类型的同步处理器契约。</para>
    /// </remarks>
    /// <typeparam name="TAttr">属性类型</typeparam>
    public interface IProcessor<TAttr> : IProcessor where TAttr : EasyAttribute
    {
        /// <summary>前置处理</summary>
        void Before(IContext context, TAttr attribute);
        /// <summary>核心处理</summary>
        IProcessorHandle Process(IContext context, TAttr attribute);
        /// <summary>后置处理</summary>
        void After(IContext context, TAttr attribute);
    }
}