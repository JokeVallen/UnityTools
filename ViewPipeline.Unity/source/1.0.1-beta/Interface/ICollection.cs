namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 集合接口
    /// </summary>
    public interface ICollection<TElement>
    {
        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns>实例</returns>
        TElement Acquire();

        /// <summary>
        /// 归还实例
        /// </summary>
        /// <param name="element">实例</param>
        void Return(TElement element);
    }
}
