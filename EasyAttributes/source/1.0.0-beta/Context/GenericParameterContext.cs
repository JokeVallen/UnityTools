using System;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class GenericParameterContext : Context<EasyAttribute>, IGenericParameterContext, IGenericParameterContextWriter, IAsyncContext
    {
        public object Target => target;
        public Type GenericParameter => genericParameter;
        public MemberInfo DeclaringMember => declaringMember;
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;
        IGenericParameterContext IGenericParameterContextWriter.Context => this;

        private readonly object target;
        private readonly Type genericParameter;
        private readonly MemberInfo declaringMember;
        private readonly CancellationToken cancellationToken;

        public GenericParameterContext(
            EasyAttribute attribute,
            Type genericParameter,
            MemberInfo declaringMember,
            object target,
            CancellationToken cancellationToken = default)
            : base(attribute)
        {
            this.genericParameter = genericParameter ?? throw new ArgumentNullException(nameof(genericParameter));
            this.declaringMember = declaringMember ?? throw new ArgumentNullException(nameof(declaringMember));
            this.target = target;
            this.cancellationToken = cancellationToken;
        }
    }
}