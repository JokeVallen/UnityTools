using System;

namespace EasyAttributes.Core
{
    internal interface IConstructorContextWriter : IContextWriter
    {
        IConstructorContext Context { get; }
        void SetException(Exception exception);
    }
}
