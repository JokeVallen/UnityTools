namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 可拍完整快照的组件接口
    /// </summary>
    /// <typeparam name="TSnapshot">照片类型</typeparam>
    public interface IFullSnapshotable<TSnapshot>
    {
        /// <summary>
        /// 获取完整快照
        /// </summary>
        /// <returns>完整快照</returns>
        TSnapshot GetFullSnapshot();
    }
}
