namespace FSM.Runtime
{
    /// <summary>
    /// 附带上下文的可重置接口
    /// </summary>
    /// <typeparam name="TContext">上下文类型</typeparam>
    public interface IRestableWithContext<TContext>
    {
        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="context">上下文</param>
        void Reset(TContext context);
    }
}
