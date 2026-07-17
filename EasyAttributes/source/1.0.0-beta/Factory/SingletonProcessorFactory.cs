using System;
using System.Collections.Concurrent;

namespace EasyAttributes.Core
{
    internal sealed class SingletonProcessorFactory : IProcessorFactory
    {
        public static readonly IProcessorFactory Default = new SingletonProcessorFactory();
        private readonly ConcurrentDictionary<Type, object> cache = new ConcurrentDictionary<Type, object>();

        private SingletonProcessorFactory() { }

        public object Create(Type processorType)
        {
            if (processorType == null)
                throw new ArgumentNullException(nameof(processorType));

            return cache.GetOrAdd(processorType, tp =>
            {
                try
                {
                    return Activator.CreateInstance(tp);
                }
                catch (Exception ex)
                {
                    throw new ExecutorException($"Failed to create processor instance of type '{tp.FullName}'. Ensure the type has a public parameterless constructor.", ex);
                }
            });
        }
    }
}