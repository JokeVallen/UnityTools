using System;
using System.Collections.Generic;

namespace EasyAttributes.Core
{
    internal class DefaultExecutor : IExecutor
    {
        protected readonly IProcessorRegistry registry;
        protected readonly IProcessorFactory factory;
        protected readonly IExceptionHandler exceptionHandler;
        protected readonly IReadOnlyDictionary<Type, IFeature> features;

        public DefaultExecutor(IProcessorRegistry registry, IProcessorFactory factory, IExceptionHandler exceptionHandler, IReadOnlyDictionary<Type, IFeature> features)
        {
            this.registry = registry;
            this.factory = factory;
            this.exceptionHandler = exceptionHandler ?? NullExceptionHandler.Instance;
            this.features = features;
        }

        public IProcessorHandle Execute(IContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!context.IsEnabled)
                return ProcessorHandle.Continue;

            var descriptors = registry.GetDescriptors(context.Attribute.GetType());
            if (descriptors.Count == 0)
                return ProcessorHandle.Continue;

            if (features != null && context is IContextWriter writer) 
            {
                foreach (var kv in features)
                {
                    if(!context.Features.ContainsKey(kv.Key))
                        writer.SetFeature(kv.Key, kv.Value);
                }
            }

            var list = ExecutorHelper.GetDescriptorList(descriptors);
            var executed = new List<IProcessor>(list.Count);
            IProcessorHandle finalHandle = ProcessorHandle.Continue;
            bool receivedAbort = false;

            for (int i = 0; i < list.Count; i++)
            {
                var descriptor = list[i];
                var processor = factory.Create(descriptor.ProcessorType) as IProcessor;
                if (processor == null) continue;

                try
                {
                    processor.Before(context);
                }
                catch (Exception ex)
                {
                    var wrapped = new ProcessorBeforeException(descriptor.ProcessorType, context, ex);
                    if (!ExecutorHelper.HandleException(exceptionHandler, wrapped))
                        throw wrapped;
                    break;
                }

                executed.Add(processor);

                IProcessorHandle handle;
                try
                {
                    handle = processor.Process(context);
                }
                catch (Exception ex)
                {
                    var wrapped = new ProcessorExecuteException(descriptor.ProcessorType, context, ex);
                    if (!ExecutorHelper.HandleException(exceptionHandler, wrapped))
                    {
                        RunAfterCallbacks(executed, context);
                        throw wrapped;
                    }
                    handle = ProcessorHandle.Continue;
                }

                finalHandle = handle;

                if (handle.IsAborted)
                {
                    receivedAbort = true;
                    if (!handle.SkipAfterCallbacks)
                        RunAfterCallbacks(executed, context);
                    return handle;
                }
            }

            if (!receivedAbort)
                RunAfterCallbacks(executed, context);

            return finalHandle;
        }

        protected virtual void RunAfterCallbacks(List<IProcessor> executed, IContext context)
        {
            for (int i = executed.Count - 1; i >= 0; i--)
            {
                var processor = executed[i];
                try
                {
                    processor.After(context);
                }
                catch (Exception ex)
                {
                    var wrapped = new ProcessorAfterException(processor.GetType(), context, ex);
                    if (!ExecutorHelper.HandleException(exceptionHandler, wrapped))
                        throw wrapped;
                }
            }
        }
    }
}