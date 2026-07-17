using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 会话构建器的快照
    /// </summary>
    public readonly struct ViewSessionBuilderSnapshot
    {
        /// <summary>
        /// 会话唯一标识
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// 是否已执行构建
        /// </summary>
        public bool Built { get; }

        /// <summary>
        /// 上下文的类型
        /// </summary>
        public Type ContextType { get; }

        /// <summary>
        /// 静态中间件快照数组（视图打开方向）
        /// </summary>
        public MiddlewareSnapshot[] StaticOpenMiddlewares { get; }

        /// <summary>
        /// 静态中间件快照数组（视图关闭方向）
        /// </summary>
        public MiddlewareSnapshot[] StaticCloseMiddlewares { get; }

        /// <summary>
        /// 扩展包快照数组
        /// </summary>
        public ExtensionSnapshot[] Extensions { get; }

        /// <summary>
        /// 动态中间件流式供应器快照数组（视图打开方向）
        /// </summary>
        public DynamicMiddlewareProviderSnapshot[] OpenDynamicMiddlewareProviders { get; }

        /// <summary>
        /// 动态中间件流式供应器快照数组（视图关闭方向）
        /// </summary>
        public DynamicMiddlewareProviderSnapshot[] CloseDynamicMiddlewareProviders { get; }

        internal static readonly ViewSessionBuilderSnapshot Empty = new ViewSessionBuilderSnapshot();

        internal ViewSessionBuilderSnapshot(Guid key, bool built, Type contextType, MiddlewareSnapshot[] staticOpenMiddlewares, MiddlewareSnapshot[] staticCloseMiddlewares, ExtensionSnapshot[] extensions, DynamicMiddlewareProviderSnapshot[] openDynamicMiddlewareProviders, DynamicMiddlewareProviderSnapshot[] closeDynamicMiddlewareProviders)
        {
            Key = key;
            Built = built;
            ContextType = contextType;
            StaticOpenMiddlewares = staticOpenMiddlewares;
            StaticCloseMiddlewares = staticCloseMiddlewares;
            Extensions = extensions;
            OpenDynamicMiddlewareProviders = openDynamicMiddlewareProviders;
            CloseDynamicMiddlewareProviders = closeDynamicMiddlewareProviders;
        }
    }
}
