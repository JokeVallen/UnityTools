namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 可自拍组件接口
    /// </summary>
    /// <typeparam name="TSnapshot">照片类型</typeparam>
    public interface ISelfSnapshotable<TSnapshot>
    {
        /// <summary>
        /// 获取自拍
        /// </summary>
        /// <returns>照片</returns>
        TSnapshot GetSelfSnapshot();
    }
}
