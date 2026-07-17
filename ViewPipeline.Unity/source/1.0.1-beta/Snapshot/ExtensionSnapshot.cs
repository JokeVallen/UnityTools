using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 扩展包快照
    /// </summary>
    public readonly struct ExtensionSnapshot
    {
        /// <summary>
        /// 扩展包类型
        /// </summary>
        public Type ExtensionType { get; }

        /// <summary>
        /// 扩展包是否已初始化
        /// </summary>
        public bool IsInitialized { get; }

        internal static readonly ExtensionSnapshot Empty = new ExtensionSnapshot();

        /// <param name="extensionType">扩展包类型</param>
        /// <param name="isInitialized">扩展包是否已初始化</param>
        public ExtensionSnapshot(Type extensionType, bool isInitialized)
        {
            ExtensionType = extensionType;
            IsInitialized = isInitialized;
        }
    }
}
