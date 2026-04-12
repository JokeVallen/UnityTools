#if !EVENTHUB_EXTENSION_ENABLE

using System;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal static class ExceptionCatcher
    {
        private class DefaultExceptionCatcher : IExceptionCatchable
        {
            public event Action<Type, Delegate, Exception> OnError
            {
                add
                {
                    lock (errorEventLock)
                    {
                        ErrorEvents += value;
                    }
                }
                remove
                {
                    lock (errorEventLock)
                    {
                        ErrorEvents -= value;
                    }
                }
            }
            private event Action<Type, Delegate, Exception> ErrorEvents;
            private readonly object errorEventLock = new object();

            public void CatchError(Type eventType, Delegate handler, Exception exception)
            {
                ErrorEvents?.Invoke(eventType, handler, exception);
            }
        }

        public static event Action<Type, Delegate, Exception> OnError
        {
            add { if (Enabled) GetCatcher().OnError += value; }
            remove { if (Enabled) GetCatcher().OnError -= value; }
        }

        public static bool Enabled = true;

        public static IExceptionCatchable Catcher { set => catcher = value; }
        private static IExceptionCatchable catcher = defaultCatcher;

        private static readonly IExceptionCatchable defaultCatcher = new DefaultExceptionCatcher();

        private static IExceptionCatchable GetCatcher()
        {
            if (catcher != null) return catcher;
            return defaultCatcher;
        }

        public static void CatchError(Type eventType, Delegate handler, Exception exception)
        {
            if (!Enabled) return;

            try
            {
                var catcher = GetCatcher();
                catcher.CatchError(eventType, handler, exception);
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"The method '{nameof(CatchError)}' triggered an exception: {ex.Message}.");
            }
        }
    }
}

#endif