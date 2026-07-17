using System;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class PropertyContext : Context<EasyAttribute>, IPropertyContext, IPropertyContextWriter, IAsyncContext
    {
        public object Target => target;
        public PropertyInfo Property => property;
        public PropertyAccessor Accessor => accessor;
        public object Value => value;
        public Func<object> GetOriginal { get; set; }
        public Action<object> SetOriginal { get; set; }
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;
        IPropertyContext IPropertyContextWriter.Context => this;

        private readonly object target;
        private readonly PropertyInfo property;
        private readonly PropertyAccessor accessor;
        private readonly CancellationToken cancellationToken;
        private object value;

        public PropertyContext(EasyAttribute attribute, PropertyInfo property, PropertyAccessor accessor, object target, object value, CancellationToken cancellationToken = default) : base(attribute)
        {
            this.property = property ?? throw new ArgumentNullException(nameof(property));
            this.accessor = accessor;
            this.target = target;
            this.value = value;
            this.cancellationToken = cancellationToken;
        }

        void IPropertyContextWriter.SetValue(object value) => this.value = value;
    }
}