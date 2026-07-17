using System.Collections.Generic;

namespace EasyAttributes.Core
{
    internal static class ExecutorHelper
    {
        internal static IReadOnlyList<ProcessorDescriptor> GetDescriptorList(IReadOnlyList<ProcessorDescriptor> descriptors) => descriptors;
        internal static bool HandleException(IExceptionHandler handler, EasyAttributeException ex) => handler != null && handler.Handle(ex);
    }
}