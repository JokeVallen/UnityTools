using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 会话实例快照
    /// </summary>
    public readonly struct ViewSessionSnapshot
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// 扩展包快照数组
        /// </summary>
        public ExtensionSnapshot[] Extensions { get; }

        /// <summary>
        /// 视图显示管线快照
        /// </summary>
        public ViewPipelineEngineSnapshot OpenViewPipelineEngineSnapshot { get; }

        /// <summary>
        /// 视图隐藏管线快照
        /// </summary>
        public ViewPipelineEngineSnapshot CloseViewPipelineEngineSnapshot { get; }

        /// <summary>
        /// 当前视图显示管线中的活动数量
        /// </summary>
        public int ActiveOpenedOperations { get; }

        /// <summary>
        /// 当前视图隐藏管线中的活动数量
        /// </summary>
        public int ActiveClosedOperations { get; }

        internal static readonly ViewSessionSnapshot Empty = new ViewSessionSnapshot();

        internal ViewSessionSnapshot(Guid key, ExtensionSnapshot[] extensions, ViewPipelineEngineSnapshot openViewPipelineEngineSnapshot, ViewPipelineEngineSnapshot closeViewPipelineEngineSnapshot, int activeOpenedOperations, int activeClosedOperations)
        {
            Key = key;
            Extensions = extensions;
            OpenViewPipelineEngineSnapshot = openViewPipelineEngineSnapshot;
            CloseViewPipelineEngineSnapshot = closeViewPipelineEngineSnapshot;
            ActiveOpenedOperations = activeOpenedOperations;
            ActiveClosedOperations = activeClosedOperations;
        }
    }
}
