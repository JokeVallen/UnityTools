using System.Threading;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity
{
    /// <summary>
    /// 最高层视口行为契约（最小原语：只有可显示、可隐藏两个原子动作）
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// 驱动视口进入显示管线。
        /// </summary>
        UniTask ShowAsync(CancellationToken cancellationToken);

        /// <summary>
        /// 驱动视口进入隐藏管线。
        /// </summary>
        UniTask HideAsync(CancellationToken cancellationToken);
    }
}
