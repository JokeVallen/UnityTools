using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 提供异步释放资源的能力。
    /// </summary>
    public interface IAsyncDisposable
    {
        /// <summary>
        /// 异步释放资源。
        /// </summary>
        /// <returns>表示异步释放操作的任务。</returns>
        UniTask DisposeAsync();
    }
}
