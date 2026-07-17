namespace EasyAttributes
{
    /// <summary>
    /// 处理器句柄接口
    /// </summary>
    public interface IProcessorHandle
    {
        /// <summary>
        /// 是否中止后续处理器的执行
        /// </summary>
        bool IsAborted { get; }

        /// <summary>
        /// 中止时是否跳过所有 <c>OnAfter</c> 回调
        /// </summary>
        bool SkipAfterCallbacks { get; }

        /// <summary>
        /// 中止时向调用方返回的替代结果
        /// </summary>
        object Result { get; }
    }
}
