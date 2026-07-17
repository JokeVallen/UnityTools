namespace EasyAttributes.Core
{
    internal interface IEventContextWriter : IContextWriter
    {
        IEventContext Context { get; }
    }
}
