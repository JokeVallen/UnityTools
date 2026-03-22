namespace UGUI.Layout.Extension
{
    /// <summary>
    /// 关键帧映射模式
    /// </summary>
    public enum KeyframeMappingMode
    {
        /// <summary>
        /// 直接映射
        /// </summary>
        /// <remarks>
        /// <para>关键帧与布局元素一一对应，超出时按 PostWrapMode 循环。</para>
        /// </remarks>
        Direct,
        /// <summary>
        /// 曲线插值
        /// </summary>
        /// <remarks>
        /// <para>布局元素索引归一化后在曲线上连续采样，X 轴线性展开，Y 轴按曲线值起伏。</para>
        /// </remarks>
        Interpolated,
        /// <summary>
        /// 按比例分配
        /// </summary>
        /// <remarks>
        /// <para>布局元素均匀映射到关键帧索引上，取对应关键帧的坐标值。</para>
        /// </remarks>
        Proportional
    }

    /// <summary>
    /// 布局元素位置计算模式
    /// </summary>
    public enum PositionMode
    {
        /// <summary>
        /// 布局元素尺寸模式（默认）
        /// </summary>
        /// <remarks>
        /// <para><c>pos = effectiveSize × factor × scale</c>，偏移量以布局元素自身尺寸为单位缩放。</para>
        /// <para>factor 为 1 时元素偏移恰好等于自身宽/高，适合尺寸一致的场景。</para>
        /// </remarks>
        ByElementSize,
        /// <summary>
        /// 像素模式
        /// </summary>
        /// <remarks>
        /// <para><c>pos = factor × scale</c>，曲线直接描述像素偏移，与元素尺寸无关。</para>
        /// <para>不同尺寸的元素在相同关键帧下落在相同位置，适合需要精确像素定位的场景。</para>
        /// </remarks>
        ByPixel
    }

    /// <summary>
    /// Proportional 模式的分组分配策略
    /// </summary>
    public enum ProportionalDistributeMode
    {
        /// <summary>
        /// 四舍五入（默认）
        /// </summary>
        /// <remarks>
        /// <para>布局元素索引映射到最近的关键帧，分组边界处可能出现相邻组数量差 1。</para>
        /// </remarks>
        RoundToNearest,
        /// <summary>
        /// 均匀分配
        /// </summary>
        /// <remarks>
        /// <para>基于 Bresenham 算法均匀分配，各组数量差不超过 1，分布最均匀。</para>
        /// </remarks>
        Uniform,
        /// <summary>
        /// 前密后疏
        /// </summary>
        /// <remarks>
        /// <para>靠前的关键帧分配更多布局元素。</para>
        /// </remarks>
        FloorBias,
        /// <summary>
        /// 前疏后密
        /// </summary>
        /// <remarks>
        /// <para>靠后的关键帧分配更多布局元素。</para>
        /// </remarks>
        CeilBias
    }
}