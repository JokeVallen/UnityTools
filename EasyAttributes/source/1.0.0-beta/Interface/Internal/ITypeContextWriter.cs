namespace EasyAttributes.Core
{
    internal interface ITypeContextWriter : IContextWriter
    {
        ITypeContext Context { get; }
    }
}
