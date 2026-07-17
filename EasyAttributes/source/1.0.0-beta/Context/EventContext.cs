using System;
using System.Reflection;
using System.Threading;

namespace EasyAttributes.Core
{
    internal sealed class EventContext : Context<EasyAttribute>, IEventContext, IEventContextWriter, IAsyncContext
    {
        public object Target => target;
        public EventInfo Event => @event;
        public EventAccessor Accessor => accessor;
        public Delegate Handler => handler;
        public Action<Delegate> AddOriginal { get; set; }
        public Action<Delegate> RemoveOriginal { get; set; }
        CancellationToken IAsyncContext.CancellationToken => cancellationToken;
        IEventContext IEventContextWriter.Context => this;

        private readonly object target;
        private readonly EventInfo @event;
        private readonly EventAccessor accessor;
        private readonly Delegate handler;
        private readonly CancellationToken cancellationToken;

        public EventContext(
            EasyAttribute attribute,
            EventInfo @event,
            EventAccessor accessor,
            object target,
            Delegate handler,
            CancellationToken cancellationToken = default)
            : base(attribute)
        {
            this.@event = @event ?? throw new ArgumentNullException(nameof(@event));
            this.accessor = accessor;
            this.target = target;
            this.handler = handler;
            this.cancellationToken = cancellationToken;
        }
    }
}