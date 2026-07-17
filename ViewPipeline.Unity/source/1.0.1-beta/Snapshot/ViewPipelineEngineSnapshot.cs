using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 管道的快照
    /// </summary>
    public readonly struct ViewPipelineEngineSnapshot
    {
        /// <summary>
        /// 会话唯一标识
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// 流转方向
        /// </summary>
        public PipelineDirection Direction { get; }

        /// <summary>
        /// 静态中间件的快照数组
        /// </summary>
        public MiddlewareSnapshot[] StaticMiddlewares { get; }

        /// <summary>
        /// 动态中间件流式供应器的快照数组
        /// </summary>
        public DynamicMiddlewareProviderSnapshot[] DynamicProviders { get; }

        /// <summary>
        /// 中间件的快照数组
        /// </summary>
        public MiddlewareSnapshot[] Middlewares { get; }

        /// <summary>
        /// 管线当前活动数量
        /// </summary>
        public int ActiveOperations { get; }

        internal static readonly ViewPipelineEngineSnapshot Empty = new ViewPipelineEngineSnapshot();

        internal ViewPipelineEngineSnapshot(Guid key, PipelineDirection direction, MiddlewareSnapshot[] staticMiddlewares, DynamicMiddlewareProviderSnapshot[] dynamicProviders, MiddlewareSnapshot[] middlewares, int activeOperations)
        {
            Key = key;
            Direction = direction;
            StaticMiddlewares = staticMiddlewares;
            DynamicProviders = dynamicProviders;
            Middlewares = middlewares;
            ActiveOperations = activeOperations;
        }
    }
}
