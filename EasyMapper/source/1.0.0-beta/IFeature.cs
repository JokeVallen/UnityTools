namespace EasyMapper
{
    /// <summary>
    /// 蓝图特征描述接口
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// 是否可通过算法溯源
        /// </summary>
        /// <remarks>
        /// <para>标识是否可从令牌通过算法直接生成数据源。</para>
        /// </remarks>
        bool IsTraceable { get; }
    }
}