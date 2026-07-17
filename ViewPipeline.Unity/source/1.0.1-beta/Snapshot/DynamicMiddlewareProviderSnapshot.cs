using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 动态中间件供应器快照
    /// </summary>
    public readonly struct DynamicMiddlewareProviderSnapshot
    {
        /// <summary>
        /// 动态中间件供应器类型
        /// </summary>
        public Type ProviderType { get; }

        internal static readonly DynamicMiddlewareProviderSnapshot Empty = new DynamicMiddlewareProviderSnapshot();

        /// <param name="providerType">动态中间件供应器类型</param>
        public DynamicMiddlewareProviderSnapshot(Type providerType)
        {
            ProviderType = providerType;
        }
    }
}
