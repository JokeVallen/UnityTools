using EasyAttributes.Core.Extension;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    internal class DefaultExecutorAsync : IExecutorAsync
    {
        protected readonly IProcessorRegistry registry;
        protected readonly IProcessorFactory factory;
        protected readonly IExceptionHandler exceptionHandler;
        protected readonly IReadOnlyDictionary<Type, IFeature> features;

        public DefaultExecutorAsync(IProcessorRegistry registry, IProcessorFactory factory, IExceptionHandler exceptionHandler, IReadOnlyDictionary<Type, IFeature> features)
        {
            this.registry = registry;
            this.factory = factory;
            this.exceptionHandler = exceptionHandler ?? NullExceptionHandler.Instance;
            this.features = features;
        }

        public async Task<IProcessorHandle> ExecuteAsync(IContext context, CancellationToken cancellationToken = default)
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
                    if (!context.Features.ContainsKey(kv.Key))
                        writer.SetFeature(kv.Key, kv.Value);
                }
            }

            var list = ExecutorHelper.GetDescriptorList(descriptors);
            var executed = new List<(object instance, bool isAsync)>(list.Count);
            IProcessorHandle finalHandle = ProcessorHandle.Continue;
            bool receivedAbort = false;

            for (int i = 0; i < list.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var descriptor = list[i];
                var instance = factory.Create(descriptor.ProcessorType);
                bool isAsync = instance is IProcessorAsync;

                try
                {
                    if (isAsync)
                        await ((IProcessorAsync)instance).BeforeAsync(context).ConfigureAwait(false);
                    else if (instance is IProcessor syncProc)
                        syncProc.Before(context);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    var wrapped = new ProcessorBeforeException(descriptor.ProcessorType, context, ex);
                    if (!ExecutorHelper.HandleException(exceptionHandler, wrapped))
                        throw wrapped;
                    break;
                }

                executed.Add((instance, isAsync));

                IProcessorHandle handle;
                try
                {
                    if (isAsync)
                        handle = await ((IProcessorAsync)instance).ProcessAsync(context).ConfigureAwait(false);
                    else
                        handle = ((IProcessor)instance).Process(context);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    var wrapped = new ProcessorExecuteException(descriptor.ProcessorType, context, ex);
                    if (!ExecutorHelper.HandleException(exceptionHandler, wrapped))
                    {
                        await RunAfterCallbacksAsync(executed, context, CancellationToken.None).ConfigureAwait(false);
                        throw wrapped;
                    }
                    handle = ProcessorHandle.Continue;
                }

                finalHandle = handle;

                if (handle.IsAborted)
                {
                    receivedAbort = true;
                    if (!handle.SkipAfterCallbacks)
                        await RunAfterCallbacksAsync(executed, context, cancellationToken).ConfigureAwait(false);
                    return handle;
                }
            }

            if (!receivedAbort)
                await RunAfterCallbacksAsync(executed, context, cancellationToken).ConfigureAwait(false);

            return finalHandle;
        }

        private Task RunAfterCallbacksAsync(List<(object instance, bool isAsync)> executed, IContext context, CancellationToken cancellationToken)
        {
            if (executed.Count == 0)
                return Task.CompletedTask;

            return RunAfterCallbacksCoreAsync(executed, context, cancellationToken);
        }

        protected virtual async Task RunAfterCallbacksCoreAsync(List<(object instance, bool isAsync)> executed, IContext context, CancellationToken cancellationToken)
        {
            for (int i = executed.Count - 1; i >= 0; i--)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var (instance, isAsync) = executed[i];
                try
                {
                    if (isAsync)
                        await ((IProcessorAsync)instance).AfterAsync(context).ConfigureAwait(false);
                    else if (instance is IProcessor syncProc)
                        syncProc.After(context);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    var wrapped = new ProcessorAfterException(instance.GetType(), context, ex);
                    if (!ExecutorHelper.HandleException(exceptionHandler, wrapped))
                        throw wrapped;
                }
            }
        }
    }
}