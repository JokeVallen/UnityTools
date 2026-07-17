namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 默认管线执行上下文
    /// </summary>
    internal sealed class DefaultPipelineContext : IPipelineContext, IResstable
    {
        public static readonly DefaultPipelineContext Empty = new DefaultPipelineContext();
        private DefaultPipelineContext() { }
        /// <inheritdoc/>
        public void Reset() { }
    }
}
