namespace EasyAttributes.Core
{
    internal interface IGenericParameterContextWriter : IContextWriter
    {
        IGenericParameterContext Context { get; }
    }
}
