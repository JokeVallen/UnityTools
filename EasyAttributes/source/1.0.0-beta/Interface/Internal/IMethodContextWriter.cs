using System;

namespace EasyAttributes.Core
{
    internal interface IMethodContextWriter : IContextWriter
    {
        IMethodContext Context { get; }
        void SetResult(object result);
        void SetException(Exception exception);
    }
}
