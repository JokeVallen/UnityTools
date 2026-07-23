namespace CoroutineRunner
{
    /// <summary>
    /// 可池化接口
    /// </summary>
    /// <remarks>
    /// <para>请实现具体的池化扩展接口 <see cref="IPoolableYieldInstruction"/> 或 <see cref="IPoolableYieldInstruction{T}"/>。</para>
    /// </remarks>
    public interface IPoolable { }

    /// <summary>
    /// 可池化指令的非泛型扩展接口
    /// </summary>
    public interface IPoolableYieldInstruction : IPoolable
    {
        /// <summary>
        /// 重置指令
        /// </summary>
        /// <param name="value">用于重置的数据</param>
        void Reset(object value);
    }

    /// <summary>
    /// 可池化指令的扩展接口
    /// </summary>
    /// <typeparam name="T">用于重置的数据类型</typeparam>
    /// <remarks>
    /// <para>如果你的自定义指令需要池化支持可实现该接口。</para>
    /// </remarks>
    public interface IPoolableYieldInstruction<T> : IPoolable
    {
        /// <summary>
        /// 重置指令
        /// </summary>
        /// <param name="value">用于重置的数据</param>
        void Reset(T value);
    }
}