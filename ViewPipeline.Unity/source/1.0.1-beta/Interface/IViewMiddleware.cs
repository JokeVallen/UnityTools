using System.Threading;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 中间件扩展接口
    /// </summary>
    /// <remarks>
    /// <para>通过实现该接口可以接入扩展包或其它自定义的扩展。</para>
    /// </remarks>
    public interface IViewMiddleware
    {
        /// <summary>
        /// 异步执行中间件的切面拦截或流转控制逻辑
        /// </summary>
        /// <param name="view">当前操作的视图实例</param>
        /// <param name="executor">当前管线的流转驱动器</param>
        /// <param name="token">异步取消令牌</param>
        /// <returns>异步任务句柄</returns>
        UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token);
    }
}
