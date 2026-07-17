namespace EasyMapper
{
    /// <summary>
    /// 映射架构的萃取蓝图接口
    /// </summary>
    /// <typeparam name="TSource">数据源类型</typeparam>
    /// <typeparam name="TToken">令牌类型</typeparam>
    public interface IBlueprint<TSource, TToken>
    {
        /// <summary>
        /// 将数据源提炼为令牌
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns>令牌</returns>
        TToken Refine(TSource source);

        /// <summary>
        /// 通过令牌还原数据源
        /// </summary>
        /// <param name="token">令牌</param>
        /// <returns>数据源</returns>
        TSource Restore(TToken token);
    }
}