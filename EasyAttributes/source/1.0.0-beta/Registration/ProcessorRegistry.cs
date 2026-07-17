using System;
using System.Collections.Generic;

namespace EasyAttributes.Core
{
    internal sealed class ProcessorRegistry : IProcessorRegistry
    {
        private readonly Dictionary<Type, List<ProcessorDescriptor>> map = new Dictionary<Type, List<ProcessorDescriptor>>();

        private int registrationCounter;
        private bool isSealed;

        internal void Register(ProcessorDescriptor descriptor)
        {
            if (isSealed)
                throw new InvalidOperationException("The processor registry has been sealed and cannot accept new registrations.");

            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            descriptor.RegistrationOrder = registrationCounter++;

            if (!map.TryGetValue(descriptor.AttributeType, out var list))
            {
                list = new List<ProcessorDescriptor>();
                map[descriptor.AttributeType] = list;
            }

            list.Add(descriptor);
        }

        internal void Seal()
        {
            foreach (var list in map.Values)
                list.Sort((a, b) => a.RegistrationOrder.CompareTo(b.RegistrationOrder));

            isSealed = true;
        }

        public IReadOnlyList<ProcessorDescriptor> GetDescriptors(Type attributeType)
        {
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));

            return map.TryGetValue(attributeType, out var list)
                ? list
                : (IReadOnlyList<ProcessorDescriptor>)Array.Empty<ProcessorDescriptor>();
        }

        public bool HasProcessors(Type attributeType)
        {
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));

            return map.ContainsKey(attributeType);
        }
    }
}
