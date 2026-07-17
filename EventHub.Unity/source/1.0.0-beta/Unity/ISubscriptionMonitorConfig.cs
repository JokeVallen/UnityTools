#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE

using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// 事件订阅句柄监视器配置
    /// </summary>
    [Preserve]
    public interface ISubscriptionMonitorConfig
    {
        /// <summary>
        /// 清理时间间隔，单位 ms，默认 1000 ms
        /// </summary>
        int MilliSecondsDelay { get; set; }

        /// <summary>
        /// 是否在监视器初始化完成时便启动清理计时器，默认 true
        /// </summary>
        bool StartTimerOnInitialize { get; set; }

        /// <summary>
        /// 是否在存在修改时自动存储，默认 true
        /// </summary>
        bool AutoSave { get; set; }

        /// <summary>
        /// 触发自动存储的脏数据个数临界值，仅开启自动存储时该字段有效，默认 5
        /// </summary>
        int AutoSaveDirtyCount { get; set; }
    }
}

#endif