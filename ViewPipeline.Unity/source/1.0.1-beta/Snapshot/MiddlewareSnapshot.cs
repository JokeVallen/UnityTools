using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 中间件快照
    /// </summary>
    public readonly struct MiddlewareSnapshot
    {
        /// <summary>
        /// 中间件类型
        /// </summary>
        public Type MiddlewareType { get; }

        internal static readonly MiddlewareSnapshot Empty = new MiddlewareSnapshot();

        /// <param name="middlewareType">中间件类型</param>
        public MiddlewareSnapshot(Type middlewareType)
        {
            MiddlewareType = middlewareType;
        }
    }
}
