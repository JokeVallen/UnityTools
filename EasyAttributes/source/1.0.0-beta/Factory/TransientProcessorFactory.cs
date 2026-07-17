using System;

namespace EasyAttributes.Core
{
    internal sealed class TransientProcessorFactory : IProcessorFactory
    {
        public static readonly IProcessorFactory Default = new TransientProcessorFactory();
        private TransientProcessorFactory() { }

        public object Create(Type processorType)
        {
            if (processorType == null)
                throw new ArgumentNullException(nameof(processorType));

            try
            {
                return Activator.CreateInstance(processorType);
            }
            catch (Exception ex)
            {
                throw new ExecutorException($"Failed to create processor instance of type '{processorType.FullName}'. Ensure the type has a public parameterless constructor.", ex);
            }
        }
    }
}
