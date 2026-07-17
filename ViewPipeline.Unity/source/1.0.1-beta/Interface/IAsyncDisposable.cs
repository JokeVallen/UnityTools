using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 异步释放资源的接口
    /// </summary>
    public interface IAsyncDisposable
    {
        /// <summary>
        /// 异步释放资源
        /// </summary>
        /// <returns>异步释放操作的任务实例</returns>
        UniTask DisposeAsync();
    }
}
