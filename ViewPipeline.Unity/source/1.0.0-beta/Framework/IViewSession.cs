using Cysharp.Threading.Tasks;
using System.Threading;

namespace ViewPipeline.Unity
{
    /// <summary>
    /// 视图会话接口
    /// </summary>
    public interface IViewSession
    {
        /// <summary>
        /// 将一个已经构建完毕、填充好数据的 View 纳入架构管线并激活显示。
        /// </summary>
        UniTask OpenViewAsync(IView view, CancellationToken cancellationToken);

        /// <summary>
        /// 将指定的 View 从架构管线中移出并隐藏。
        /// </summary>
        UniTask CloseViewAsync(IView view, CancellationToken cancellationToken);
    }
}
