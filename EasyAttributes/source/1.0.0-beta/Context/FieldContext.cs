using System;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class FieldContext : Context<EasyAttribute>, IFieldContext, IFieldContextWriter, IAsyncContext
    {
        public object Target => target;
        public FieldInfo Field => field;
        public object Value => value;
        public Func<object> GetOriginal { get; set; }
        public Action<object> SetOriginal { get; set; }
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;
        IFieldContext IFieldContextWriter.Context => this;

        private readonly object target;
        private readonly FieldInfo field;
        private readonly CancellationToken cancellationToken;
        private object value;

        public FieldContext(EasyAttribute attribute, FieldInfo field, object target, object value, CancellationToken cancellationToken = default) : base(attribute)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
            this.target = target;
            this.value = value;
            this.cancellationToken = cancellationToken;
        }

        void IFieldContextWriter.SetValue(object value) => this.value = value;
    }
}