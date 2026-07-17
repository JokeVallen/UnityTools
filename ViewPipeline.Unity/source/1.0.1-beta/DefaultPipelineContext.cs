namespace ViewPipeline.Unity.Core
{
    internal sealed class DefaultPipelineContext : IPipelineContext, IResettable
    {
        public static readonly DefaultPipelineContext Empty = new DefaultPipelineContext();
        private DefaultPipelineContext() { }
        public void Reset() { }
    }
}
