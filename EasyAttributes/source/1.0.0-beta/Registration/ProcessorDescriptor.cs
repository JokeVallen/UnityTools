using System;

namespace EasyAttributes.Core
{
    internal sealed class ProcessorDescriptor
    {
        public Type AttributeType { get; }
        public Type ProcessorType { get; }
        public bool IsAsync { get; }
        internal int RegistrationOrder { get; set; }

        public ProcessorDescriptor(Type attributeType, Type processorType)
        {
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));
            if (processorType == null)
                throw new ArgumentNullException(nameof(processorType));

            if (!typeof(EasyAttribute).IsAssignableFrom(attributeType))
                throw new ArgumentException($"'{attributeType.FullName}' must inherit from {nameof(EasyAttribute)}.", nameof(attributeType));

            var isSyncProcessor = typeof(IProcessor).IsAssignableFrom(processorType);
            var isAsyncProcessor = typeof(IProcessorAsync).IsAssignableFrom(processorType);

            if (!isSyncProcessor && !isAsyncProcessor)
                throw new ArgumentException($"'{processorType.FullName}' must implement {nameof(IProcessor)} or {nameof(IProcessorAsync)}.", nameof(processorType));

            AttributeType = attributeType;
            ProcessorType = processorType;
            IsAsync = isAsyncProcessor;
        }
    }
}
