namespace EasyAttributes.Core
{
    internal interface IPropertyContextWriter : IContextWriter
    {
        IPropertyContext Context { get; }
        void SetValue(object value);
    }
}
