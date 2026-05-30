namespace CoroutineRunner
{
    /// <summary>
    /// 协程控制状态枚举
    /// </summary>
    public enum CoroutineState
    {
        /// <summary> 正在运行 </summary>
        Running,
        /// <summary> 已暂停 </summary>
        Paused,
        /// <summary> 正常执行完成 </summary>
        Completed,
        /// <summary> 已被外部或异常取消 </summary>
        Canceled
    }
}