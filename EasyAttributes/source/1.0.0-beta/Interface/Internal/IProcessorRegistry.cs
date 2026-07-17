using System;
using System.Collections.Generic;

namespace EasyAttributes.Core
{
    internal interface IProcessorRegistry
    {
        IReadOnlyList<ProcessorDescriptor> GetDescriptors(Type attributeType);
        bool HasProcessors(Type attributeType);
    }
}
