using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class ConstructorContext : Context<EasyAttribute>, IConstructorContext, IConstructorContextWriter, IAsyncContext
    {
        public object Target => target;
        public ConstructorInfo Constructor => constructor;
        public IReadOnlyList<ParameterInfo> Parameters => parameters;
        public object[] Arguments => arguments;
        public Func<object> Proceed { get; set; }
        public Exception Exception => exception;
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;
        IConstructorContext IConstructorContextWriter.Context => this;

        private readonly object target;
        private readonly ConstructorInfo constructor;
        private readonly IReadOnlyList<ParameterInfo> parameters;
        private readonly CancellationToken cancellationToken;
        private readonly object[] arguments;
        private Exception exception;

        public ConstructorContext(
            EasyAttribute attribute,
            ConstructorInfo constructor,
            object target,
            object[] arguments,
            CancellationToken cancellationToken = default)
            : base(attribute)
        {
            this.constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            parameters = Array.AsReadOnly(constructor.GetParameters());
            this.target = target;
            this.arguments = arguments;
            this.cancellationToken = cancellationToken;
        }

        void IConstructorContextWriter.SetException(Exception exception) => this.exception = exception;
    }
}