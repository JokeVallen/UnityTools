using System;

namespace EasyProgress
{
    /// <summary>
    /// 进度节点非泛型接口
    /// </summary>
    public interface IProgressNode { }

    /// <summary>
    /// 进度节点泛型接口
    /// </summary>
    /// <typeparam name="T">进度值类型</typeparam>
    /// <remarks>
    /// <para>表示一个可被观察进度的实体。</para>
    /// </remarks>
    public interface IProgressNode<T> : IProgressNode
    {
        /// <summary>当前进度值</summary>
        T Progress { get; }

        /// <summary>进度变化事件</summary>
        /// <remarks>事件参数为节点本身和新的进度值</remarks>
        event Action<IProgressNode<T>, T> OnProgressChanged;
    }
}
