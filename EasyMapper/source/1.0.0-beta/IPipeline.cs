namespace EasyMapper
{
    /// <summary>
    /// 映射架构的处理流水线接口
    /// </summary>
    /// <typeparam name="TSource">数据源类型</typeparam>
    /// <typeparam name="TToken">令牌类型</typeparam>
    /// <remarks>
    /// <para>负责通过调度萃取蓝图及其它模块完成映射处理工作。</para>
    /// </remarks>
    public interface IPipeline<TSource, TToken> where TToken : struct
    {
        /// <summary>
        /// 输入数据源生成令牌
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns>令牌</returns>
        TToken Import(TSource source);

        /// <summary>
        /// 输入令牌返回数据源
        /// </summary>
        /// <param name="token">令牌</param>
        /// <returns>数据源</returns>
        TSource Export(TToken token);
    }
}