using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class MethodContext : Context<EasyAttribute>, IMethodContext, IMethodContextWriter, IAsyncContext
    {
        public object Target => target;
        public MethodInfo Method => method;
        public IReadOnlyList<ParameterInfo> Parameters => parameters;
        public object[] Arguments => arguments;
        public Func<object> Proceed { get; set; }
        public object Result => result;
        public Exception Exception => exception;
        IMethodContext IMethodContextWriter.Context => this;
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;

        private readonly object target;
        private readonly MethodInfo method;
        private IReadOnlyList<ParameterInfo> parameters;
        private readonly CancellationToken cancellationToken;
        private readonly object[] arguments;
        private object result;
        private Exception exception;

        public MethodContext(EasyAttribute attribute, MethodInfo method, object target, object[] arguments, CancellationToken cancellationToken = default) : base(attribute)
        {
            this.method = method ?? throw new ArgumentNullException(nameof(method));
            parameters = Array.AsReadOnly(method.GetParameters());
            this.target = target;
            this.arguments = arguments;
            this.cancellationToken = cancellationToken;
        }

        void IMethodContextWriter.SetResult(object result) => this.result = result;
        void IMethodContextWriter.SetException(Exception exception) => this.exception = exception;
    }
}