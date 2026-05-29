namespace EasyProgress.Core
{
    /// <summary>
    /// 可重置接口
    /// </summary>
    /// <remarks>
    /// <para>实现此接口的类型支持将内部状态重置为初始值，常用于对象池复用。</para>
    /// </remarks>
    public interface IResettable
    {
        /// <summary>重置内部状态</summary>
        void Reset();
    }
}
