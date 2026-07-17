namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 附带跳过中间件处理功能的视图
    /// </summary>
    public interface ISkippableView
    {
        /// <summary>
        /// 是否跳过执行指定中间件
        /// </summary>
        /// <param name="middleware">中间件</param>
        /// <returns>跳过则返回 true，否则返回 false。</returns>
        bool ShouldSkip(IViewMiddleware middleware);
    }
}
