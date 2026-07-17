namespace EasyAttributes.Core
{
    internal interface IFieldContextWriter : IContextWriter
    {
        IFieldContext Context { get; }
        void SetValue(object value);
    }
}
