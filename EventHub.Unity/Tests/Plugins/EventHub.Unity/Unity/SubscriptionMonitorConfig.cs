#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    /// <summary>
    /// 事件订阅句柄监视器配置
    /// </summary>
    [Preserve]
    internal sealed class SubscriptionMonitorConfig : ScriptableObject, ISubscriptionMonitorConfig
    {
        /// <summary>
        /// 全局单例
        /// </summary>
        public static SubscriptionMonitorConfig Instance
        {
            get 
            {
                if (instance == null) Initialize();
                return instance;
            }
        }

        /// <summary>
        /// 清理时间间隔，单位 ms，默认 1000 ms
        /// </summary>
        public int MilliSecondsDelay 
        {
            get => millisecondsDelay;
            set 
            { 
                int newValue = Mathf.Max(0, value);
                bool hasChange = newValue != millisecondsDelay;
                millisecondsDelay = newValue;
                if (hasChange) SetPropertyDirty(1 << 0);
            }
        }

        /// <summary>
        /// 是否在监视器初始化完成时便启动清理计时器，默认 true
        /// </summary>
        public bool StartTimerOnInitialize 
        {
            get => startTimerOnInitialize;
            set 
            {
                bool hasChange = value != startTimerOnInitialize;
                startTimerOnInitialize = value;
                if(hasChange) SetPropertyDirty(1 << 1);
            }
        }

        /// <summary>
        /// 是否在存在修改时自动存储，默认 true
        /// </summary>
        public bool AutoSave 
        {
            get => autoSave;
            set 
            {
                bool hasChange = value != autoSave;
                autoSave = value;
                if (hasChange) SetPropertyDirty(1 << 2);
            }
        }

        /// <summary>
        /// 触发自动存储的脏数据个数临界值，仅开启自动存储时该字段有效，默认 5
        /// </summary>
        public int AutoSaveDirtyCount 
        {
            get => autoSaveDirtyCount;
            set 
            {
                int newValue = Mathf.Max(1, value);
                bool hasChange = newValue != autoSaveDirtyCount;
                autoSaveDirtyCount = newValue;
                if (hasChange) SetPropertyDirty(1 << 3);
            }
        }

        private static SubscriptionMonitorConfig instance;

        [Tooltip("清理时间间隔，单位 ms，默认 1000 ms"), SerializeField] private int millisecondsDelay = 1000;
        [Tooltip("是否在监视器初始化完成时便启动清理计时器，默认 true"), SerializeField] private bool startTimerOnInitialize = true;
        [Tooltip("是否在存在修改时自动存储，默认 true"), SerializeField] private bool autoSave = true;
        [Tooltip("触发自动存储的脏数据个数临界值，仅开启自动存储时该字段有效，默认 5"), SerializeField] private int autoSaveDirtyCount = 5;

        private const string PERSISTENT_DATA_KEY = "SUBSCRIPTION_MONITOR_CONFIG_PERSISTENT_DATA_KEY";
        private int dirty = 0;

        /// <summary>
        /// 保存配置数据
        /// </summary>
        public void Save() 
        {
            SaveInternal();
        }

        /// <summary>
        /// 重新加载配置数据
        /// </summary>
        public void Reload() 
        {
            ReloadInternal();
        }

        /// <summary>
        /// 重置配置数据为默认值
        /// </summary>
        public void ResetDefault() 
        { 
            ResetDefaultInternal();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            try
            {
                if (instance != null) return;
                instance = CreateInstance<SubscriptionMonitorConfig>();
                instance.ReloadInternal();
            }
            catch (Exception ex) 
            {
                EventDispatcherUtility.CatchError($"The method '{nameof(Initialize)}' triggered an exception：{ex.Message}");
                return;
            }
        }

        private void OnDestroy() 
        {
            SaveIfNeed();
        }

        private void SaveInternal() 
        {
            string jsonStr = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(PERSISTENT_DATA_KEY, jsonStr);
            PlayerPrefs.Save();
            dirty = 0;
        }

        private void ReloadInternal() 
        {
            string rawData = PlayerPrefs.GetString(PERSISTENT_DATA_KEY, string.Empty);
            if (!string.IsNullOrWhiteSpace(rawData)) JsonUtility.FromJsonOverwrite(rawData, this);
            dirty = 0;
        }

        private void ResetDefaultInternal() 
        {
            millisecondsDelay = 1000;
            startTimerOnInitialize = true;
            autoSave = true;
            autoSaveDirtyCount = 5;
            dirty = 0;
        }

        private void SetPropertyDirty(int flag) 
        {
            dirty |= flag;
            SaveIfNeed();
        }

        private void SaveIfNeed() 
        {
            if (autoSave && CountBits(dirty) >= autoSaveDirtyCount)
            {
                SaveInternal();
                dirty = 0;
            }
        }

        private static int CountBits(int n)
        {
            int count = 0;
            while (n != 0)
            {
                n &= (n - 1); // 清除最低位的 1
                count++;
            }
            return count;
        }
    }
}

#endif