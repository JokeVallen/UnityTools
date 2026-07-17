namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 表示管线当前的拓扑执行流向
    /// </summary>
    public enum PipelineDirection
    {
        /// <summary> 打开/激活视图流向 </summary>
        Open,

        /// <summary> 关闭/隐藏视图流向 </summary>
        Close
    }

    /// <summary>
    /// 验证器：严重等级
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// 普通信息
        /// </summary>
        Info,

        /// <summary>
        /// 警告
        /// </summary>
        Warning,

        /// <summary>
        /// 错误
        /// </summary>
        Error,
    }
}
