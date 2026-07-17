using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EasyAttributes.Core
{
    internal static class ContextDefaults
    {
        internal static readonly IReadOnlyDictionary<string, object> EmptyItems
        = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(0));

        internal static readonly IReadOnlyDictionary<Type, IFeature> EmptyFeatures
        = new ReadOnlyDictionary<Type, IFeature>(new Dictionary<Type, IFeature>(0));

        internal static readonly Task<IProcessorHandle> ContinueTask
        = Task.FromResult(ProcessorHandle.Continue);
    }
}
