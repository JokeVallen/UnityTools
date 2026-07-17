using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    internal static class ExecutionPolicy
    {
        private static readonly Dictionary<Guid, IExecutionPolicy> policies = new Dictionary<Guid, IExecutionPolicy>();

        public static void Register(Guid key, IExecutionPolicy policy)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            policies[key] = policy;
        }

        public static bool ShouldSkipMiddleware(Guid key, IView view, IViewMiddleware middleware)
        {
            if (!policies.TryGetValue(key, out var policy)) return false;
            return policy.ShouldSkipMiddleware(view, middleware);
        }

        public static bool ShouldSkipView(Guid key, IViewMiddleware middleware, IView view)
        {
            if (!policies.TryGetValue(key, out var policy)) return false;
            return policy.ShouldSkipView(middleware, view);
        }

        public static bool ShouldTerminate(Guid key, IView view)
        {
            if (!policies.TryGetValue(key, out var policy)) return false;
            return policy.ShouldTerminate(view);
        }

        public static bool ShouldTerminate(Guid key, IViewMiddleware middleware)
        {
            if (!policies.TryGetValue(key, out var policy)) return false;
            return policy.ShouldTerminate(middleware);
        }

        public static async UniTask DisposeAsync(Guid key)
        {
            if (!policies.TryGetValue(key, out var policy)) return;
            if (policy is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            if (policy is IDisposable disposable)
                disposable.Dispose();
            policies.Remove(key);
        }
    }
}