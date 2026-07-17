using System.Threading;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity
{
    /// <summary>
    /// 视图会话接口
    /// </summary>
    public interface IViewSession
    {
        /// <summary>
        /// 打开/激活指定视图
        /// </summary>
        /// <returns>异步任务实例</returns>
        UniTask OpenViewAsync(IView view, CancellationToken cancellationToken);

        /// <summary>
        /// 关闭/隐藏指定视图
        /// </summary>
        /// <returns>异步任务实例</returns>
        UniTask CloseViewAsync(IView view, CancellationToken cancellationToken);
    }
}
