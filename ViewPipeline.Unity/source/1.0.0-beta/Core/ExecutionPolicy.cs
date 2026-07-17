using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    internal static class ExecutionPolicy
    {
        private static readonly Dictionary<Guid, IMiddlewareExecutionPolicy> middlewareExecutionPolicies = new Dictionary<Guid, IMiddlewareExecutionPolicy>();

        public static void Register(Guid key, IMiddlewareExecutionPolicy policy) 
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            middlewareExecutionPolicies[key] = policy;
        }

        public static bool ShouldSkip(Guid key, IView view, IViewMiddleware middleware) 
        {
            if (middlewareExecutionPolicies.Count == 0) return false;
            if (!middlewareExecutionPolicies.TryGetValue(key, out var policy)) return false;
            return policy.ShouldSkip(view, middleware);
        }

        public static async UniTask DisposeAsync(Guid key)
        {
            if (!middlewareExecutionPolicies.TryGetValue(key, out var policy)) return;
            if (policy is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
            (policy as IDisposable)?.Dispose();
            middlewareExecutionPolicies.Remove(key);
        }
    }
}
