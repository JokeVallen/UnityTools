namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 可终止执行流程的功能接口
    /// </summary>
    public interface ITerminable
    {
        /// <summary>
        /// 是否应该终止执行流程
        /// </summary>
        /// <returns>终止则返回 true，否则返回 false。</returns>
        bool ShouldTerminate();
    }
}
