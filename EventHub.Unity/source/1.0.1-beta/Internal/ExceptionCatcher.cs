#if !EVENTHUB_EXTENSION_ENABLE

using System;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal static class ExceptionCatcher
    {
        private class NullExceptionCatcher : IExceptionCatchable
        {
            public static readonly NullExceptionCatcher Instance = new NullExceptionCatcher();

            public event Action<Type, Delegate, Exception> OnError { add { } remove { } }
            public void CatchError(Type eventType, Delegate handler, Exception exception) { }

#if EVENTHUB_TESTS
            public void Clear()
            {
                
            }
#endif

            private NullExceptionCatcher() { }
        }

        private class DefaultExceptionCatcher : IExceptionCatchable
        {
            public event Action<Type, Delegate, Exception> OnError
            {
                add
                {
                    if (disposed) return;
                    lock (errorEventLock)
                    {
                        ErrorEvents += value;
                    }
                }
                remove
                {
                    if (disposed) return;
                    lock (errorEventLock)
                    {
                        ErrorEvents -= value;
                    }
                }
            }
            private event Action<Type, Delegate, Exception> ErrorEvents;
            private readonly object errorEventLock = new object();
            private bool disposed;

            public void CatchError(Type eventType, Delegate handler, Exception exception)
            {
                if (disposed) return;
                ErrorEvents?.Invoke(eventType, handler, exception);
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                ErrorEvents = null;
            }

#if EVENTHUB_TESTS
            public void Clear()
            {
                lock (errorEventLock)
                {
                    ErrorEvents = null;
                }
            }

            public void Reset()
            {
                lock (errorEventLock)
                {
                    ErrorEvents = null;
                }
                disposed = false;
            }
#endif
        }

        public static event Action<Type, Delegate, Exception> OnError
        {
            add { if (Enabled) GetCatcher().OnError += value; }
            remove { if (Enabled) GetCatcher().OnError -= value; }
        }

        public static bool Enabled = true;

        public static IExceptionCatchable Catcher { set => catcher = value; }
        private static IExceptionCatchable catcher = defaultCatcher;

        private static bool disposed;
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

        public static void Dispose() 
        {
            if (disposed) return;
            disposed = true;
            ((DefaultExceptionCatcher)defaultCatcher).Dispose();
            if (catcher is IDisposable disposable) disposable.Dispose();
            catcher = NullExceptionCatcher.Instance;
        }

#if EVENTHUB_TESTS
        public static void Clear() 
        { 
            GetCatcher().Clear();
        }

        internal static void ResetForTesting()
        {
            disposed = false;
            catcher = defaultCatcher;
            Enabled = true;
            ((DefaultExceptionCatcher)defaultCatcher).Reset();
        }
#endif
    }
}

#endif