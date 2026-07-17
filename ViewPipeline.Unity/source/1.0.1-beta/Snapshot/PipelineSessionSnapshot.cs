using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 管道会话的快照
    /// </summary>
    public readonly struct PipelineSessionSnapshot
    {
        /// <summary>
        /// 视图会话唯一标识
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// 最大执行索引
        /// </summary>
        public int MaxExecutedIndex { get; }

        /// <summary>
        /// 是否已执行完毕
        /// </summary>
        public bool IsTerminalReached { get; }

        /// <summary>
        /// 执行流转方向
        /// </summary>
        public PipelineDirection Direction { get; }

        /// <summary>
        /// 是否被中断执行
        /// </summary>
        public bool IsAborted { get; }

        internal static readonly PipelineSessionSnapshot Empty = new PipelineSessionSnapshot();

        internal PipelineSessionSnapshot(Guid key, int maxExecutedIndex, bool isTerminalReached, PipelineDirection direction, bool isAborted)
        {
            Key = key;
            MaxExecutedIndex = maxExecutedIndex;
            IsTerminalReached = isTerminalReached;
            Direction = direction;
            IsAborted = isAborted;
        }
    }
}
