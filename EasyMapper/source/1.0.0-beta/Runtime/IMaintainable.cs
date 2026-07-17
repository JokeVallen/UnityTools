namespace EasyMapper.Runtime
{
    /// <summary> 可维护接口 </summary>
    /// <remarks>
    /// <para> 提供当前映射条目数量和显式清理能力。 </para>
    /// </remarks>
    public interface IMaintainable
    {
        /// <summary> 已注册的条目总数 </summary>
        int Count { get; }

        /// <summary> 清空所有映射 </summary>
        void Cleanup();
    }
}