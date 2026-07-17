namespace EasyAttributes.Core
{
    internal interface IReturnValueContextWriter : IContextWriter
    {
        IReturnValueContext Context { get; }
        void SetResult(object result);
    }
}
