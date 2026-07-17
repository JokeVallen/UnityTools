namespace EasyAttributes.Core
{
    internal interface IParameterContextWriter : IContextWriter
    {
        IParameterContext Context { get; }
        void SetValue(object value);
    }
}
