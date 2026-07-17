namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 附带跳过视图功能的中间件
    /// </summary>
    public interface ISkippableMiddleware
    {
        /// <summary>
        /// 是否跳过对指定视图的处理
        /// </summary>
        /// <param name="view">视图</param>
        /// <returns>跳过则返回 true，否则返回 false。</returns>
        bool ShouldSkip(IView view);
    }
}
