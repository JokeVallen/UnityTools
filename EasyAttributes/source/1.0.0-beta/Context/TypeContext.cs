using System;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class TypeContext : Context<EasyAttribute>, ITypeContext, ITypeContextWriter, IAsyncContext
    {
        public object Target => null;
        public Type Type => type;
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;
        ITypeContext ITypeContextWriter.Context => this;

        private readonly Type type;
        private readonly CancellationToken cancellationToken;

        public TypeContext(EasyAttribute attribute, Type type, CancellationToken cancellationToken = default) : base(attribute)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            this.cancellationToken = cancellationToken;
        }
    }
}