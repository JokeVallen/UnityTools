namespace EasyLogger.Unity
{
    /// <summary>
    /// Unity 线程日志记录器
    /// </summary>
    /// <remarks>
    /// <para>用于标识记录器内部使用 Unity API 的日志记录器</para>
    /// <para>注意：任何内部使用 Unity API 的日志记录器都应该实现该接口，这决定了最终的关闭策略。</para>
    /// </remarks>
    public interface IUnityThreadLogger
    {
        /// <summary>
        /// 在 Unity 线程上释放
        /// </summary>
        void DisposeOnUnityThread();
    }
}