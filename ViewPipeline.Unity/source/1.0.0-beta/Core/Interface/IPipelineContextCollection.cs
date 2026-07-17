namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 管道上下文集合接口
    /// </summary>
    public interface IPipelineContextCollection
    {
        /// <summary>
        /// 获取上下文实例
        /// </summary>
        /// <returns>上下文实例</returns>
        IPipelineContext Acquire();

        /// <summary>
        /// 归还上下文实例
        /// </summary>
        /// <param name="context">上下文实例</param>
        void Return(IPipelineContext context);
    }
}
