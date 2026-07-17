#if !EVENTHUB_EXTENSION_ENABLE

using System;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal interface IExceptionCatchable
    {
        event Action<Type, Delegate, Exception> OnError;
        void CatchError(Type eventType, Delegate handler, Exception exception);
    }
}

#endif