using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class ParameterContext : Context<EasyAttribute>, IParameterContext, IParameterContextWriter, IAsyncContext
    {
        public object Target => target;
        public MethodInfo Method => method;
        public ParameterInfo Parameter => parameter;
        public int ParameterIndex => parameterIndex;
        public object Value => value;
        public IReadOnlyList<object> Arguments => arguments;
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;
        IParameterContext IParameterContextWriter.Context => this;

        private readonly object target;
        private readonly MethodInfo method;
        private readonly ParameterInfo parameter;
        private readonly int parameterIndex;
        private readonly CancellationToken cancellationToken;
        private readonly object[] arguments;
        private object value;

        public ParameterContext(
            EasyAttribute attribute,
            MethodInfo method,
            ParameterInfo parameter,
            object target,
            object[] arguments,
            CancellationToken cancellationToken = default)
            : base(attribute)
        {
            this.method = method ?? throw new ArgumentNullException(nameof(method));
            this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            parameterIndex = parameter.Position;
            this.target = target;
            this.arguments = arguments;
            this.value = arguments?[parameterIndex];
            this.cancellationToken = cancellationToken;
        }

        void IParameterContextWriter.SetValue(object value) => this.value = value;
    }
}