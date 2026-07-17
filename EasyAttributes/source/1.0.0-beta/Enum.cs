namespace EasyAttributes.Core
{
    /// <summary>
    /// 属性访问器
    /// </summary>
    /// <remarks>
    /// <para>指示属性当前是读取还是写入操作。</para>
    /// </remarks>
    public enum PropertyAccessor 
    {
        /// <summary>读取</summary>
        Get,
        /// <summary>写入</summary>
        Set
    }

    /// <summary>
    /// 事件访问器
    /// </summary>
    /// <remarks>
    /// <para>指示事件当前是添加还是移除处理程序。</para>
    /// </remarks>
    public enum EventAccessor 
    {
        /// <summary>添加</summary>
        Add,
        /// <summary>移除</summary>
        Remove
    }
}
