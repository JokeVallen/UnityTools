using System;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class ReturnValueContext : Context<EasyAttribute>, IReturnValueContext, IReturnValueContextWriter, IAsyncContext
    {
        public object Target => target;
        public MethodInfo Method => method;
        public ParameterInfo ReturnParameter => returnParameter;
        public object Result => result;
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;
        IReturnValueContext IReturnValueContextWriter.Context => this;

        private readonly object target;
        private readonly MethodInfo method;
        private readonly ParameterInfo returnParameter;
        private readonly CancellationToken cancellationToken;
        private object result;

        public ReturnValueContext(
            EasyAttribute attribute,
            MethodInfo method,
            object target,
            CancellationToken cancellationToken = default)
            : base(attribute)
        {
            this.method = method ?? throw new ArgumentNullException(nameof(method));
            returnParameter = method.ReturnParameter;
            this.target = target;
            this.cancellationToken = cancellationToken;
        }

        void IReturnValueContextWriter.SetResult(object result) => this.result = result;
    }
}